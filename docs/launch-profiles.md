# Launch Profiles

Crystal includes two launch profiles that determine how the application starts during development. This guide explains what they do and when to use each one.

## Overview

Launch profiles are defined in `src/Crystal.WebApi/Properties/launchSettings.json` and control:
- Which servers start
- What URLs they use
- Environment variables
- Browser behavior

## The Two Profiles

### 1. WebApiWithClient (Full Stack)

**Purpose**: Run both backend API and frontend dev server together

**Configuration**:
```json
{
  "commandName": "Project",
  "launchBrowser": true,
  "applicationUrl": "https://localhost:7050",
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES": "Microsoft.AspNetCore.SpaProxy"
  }
}
```

**What Happens**:
1. Starts the .NET API on `https://localhost:7050`
2. Automatically launches React dev server via SPA Proxy
3. Runs `npm run dev` in the `Client.React` folder
4. Proxies requests between backend and frontend
5. Opens your browser automatically

**When to Use**:
- Default choice for full-stack development
- When working on features that involve both API and UI
- When you want everything to start with one command
- When you want the convenience of automatic setup

**How to Run**:
```bash
cd src/Crystal.WebApi
dotnet run --launch-profile WebApiWithClient
```

Or in Visual Studio/Rider: Select "WebApiWithClient" from the launch profile dropdown.

### 2. WebApi (Backend Only)

**Purpose**: Run only the backend API server

**Configuration**:
```json
{
  "commandName": "Project",
  "launchBrowser": true,
  "applicationUrl": "https://localhost:7050",
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development"
  }
}
```

**What Happens**:
1. Starts only the .NET API on `https://localhost:7050`
2. No frontend dev server is launched
3. Opens browser to API root (usually redirects to Swagger)

**When to Use**:
- When working exclusively on backend code
- When you want to run the React app separately
- When testing API endpoints with Postman, curl, or Swagger
- When debugging backend issues without frontend noise

**How to Run**:
```bash
cd src/Crystal.WebApi
dotnet run --launch-profile WebApi
```

Then separately start the frontend:
```bash
cd src/Client.React
npm run dev
```

## Understanding SPA Proxy

The magic of `WebApiWithClient` comes from the **ASP.NET Core SPA Proxy**.

### How It Works

**1. Configuration in `.csproj`**:
```xml
<PropertyGroup>
    <SpaRoot>../Client.React</SpaRoot>
    <SpaProxyServerUrl>https://localhost:5000</SpaProxyServerUrl>
    <SpaProxyLaunchCommand>npm run dev</SpaProxyLaunchCommand>
</PropertyGroup>
```

**2. Package Reference**:
```xml
<PackageReference Include="Microsoft.AspNetCore.SpaProxy"/>
```

**3. Hosting Startup Assembly**:
```json
"ASPNETCORE_HOSTINGSTARTUPASSEMBLIES": "Microsoft.AspNetCore.SpaProxy"
```

### What SPA Proxy Does

```
┌─────────────────────────────────────────────────────────────┐
│  WebApiWithClient Profile                                    │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  1. .NET API Starts (https://localhost:7050)                │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  2. SPA Proxy Middleware Loads                              │
│     (ASPNETCORE_HOSTINGSTARTUPASSEMBLIES)                   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  3. Launches React Dev Server                               │
│     Runs: npm run dev                                       │
│     In: ../Client.React                                     │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  4. React Dev Server Starts (http://localhost:5173)         │
│     With Vite HMR enabled                                   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  5. Proxy Configured                                        │
│     Browser → https://localhost:7050 → Vite Dev Server      │
└─────────────────────────────────────────────────────────────┘
```

### Benefits

1. **Single Command**: Start everything with one command
2. **Automatic Process Management**: Frontend dev server lifecycle managed automatically
3. **Unified Port**: Access everything through API port (7050)
4. **Hot Module Replacement**: React HMR works seamlessly
5. **Proper CORS**: No CORS issues between frontend and backend

## Comparison Table

| Feature | WebApiWithClient | WebApi (Separate) |
|---------|------------------|-------------------|
| API Server | ✅ Yes | ✅ Yes |
| React Dev Server | ✅ Auto-started | ❌ Manual |
| Browser URL | `https://localhost:7050` | API: `https://localhost:7050`<br>React: `http://localhost:5173` |
| Hot Reload (React) | ✅ Yes | ✅ Yes |
| Process Count | 1 command, 2 processes | 2 commands, 2 processes |
| Use Case | Full-stack dev | Backend-only or separate control |
| Complexity | Simple | More control |

## Custom Configuration

### Change API Port

Edit `launchSettings.json`:
```json
"applicationUrl": "https://localhost:7051;http://localhost:5001"
```

### Change React Port

Edit `vite.config.ts`:
```typescript
export default defineConfig({
  server: {
    port: 5174,
    strictPort: true
  }
})
```

And update `.csproj`:
```xml
<SpaProxyServerUrl>https://localhost:5174</SpaProxyServerUrl>
```

### Disable Browser Launch

Edit `launchSettings.json`:
```json
"launchBrowser": false
```

### Add Custom Environment Variables

```json
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development",
  "CUSTOM_VARIABLE": "value"
}
```

## Troubleshooting

### SPA Proxy Doesn't Start React

**Check**:
1. Node.js is installed: `node --version`
2. Dependencies installed: `npm install` in Client.React
3. `package.json` has `dev` script: `"dev": "vite"`

**Solution**:
```bash
cd src/Client.React
npm install
```

### Port Conflicts

If ports are already in use:

**Option 1**: Kill existing processes
```bash
# macOS/Linux
lsof -ti:7050 | xargs kill -9
lsof -ti:5173 | xargs kill -9

# Windows
netstat -ano | findstr :7050
taskkill /PID <PID> /F
```

**Option 2**: Change ports in configuration (see Custom Configuration above)

### React App Shows 404

This happens if the SPA proxy isn't running. Make sure:
1. Using `WebApiWithClient` profile
2. `Microsoft.AspNetCore.SpaProxy` package is installed
3. `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES` is set

### Slow Startup

The first time you run `WebApiWithClient`, it needs to:
- Install npm packages (if missing)
- Start Vite dev server
- Build initial React bundle

Subsequent starts are faster.

## Production vs Development

### Development (What We've Covered)
- Uses launch profiles
- Runs Vite dev server
- Hot module replacement
- Separate processes

### Production
```bash
# Build React app
cd src/Client.React
npm run build

# Run .NET API (serves built React files)
cd ../Crystal.WebApi
dotnet run --configuration Release
```

In production:
- No SPA Proxy
- React is pre-built static files
- .NET serves static files via `MapFallbackToFile("index.html")`
- Single process

## Best Practices

### For Full-Stack Development
✅ Use `WebApiWithClient`
- Convenient and fast
- Everything managed automatically
- Recommended for most scenarios

### For Backend-Only Work
✅ Use `WebApi` alone
- Lighter weight
- Faster startup
- Use Swagger for API testing

### For Frontend-Only Work
✅ Run React separately
```bash
cd src/Client.React
npm run dev
```
- React on `http://localhost:5173`
- Configure API base URL in React if needed

### For Testing
✅ Use separate processes
- More control over each component
- Easier to debug individual parts
- Better for integration testing

## Summary

- **WebApiWithClient**: One-command full-stack development with SPA Proxy
- **WebApi**: Backend-only, manual frontend control
- **SPA Proxy**: Automatically manages React dev server lifecycle
- **Choose based on**: What you're working on and your workflow preference

For most development work, **WebApiWithClient is recommended** for its simplicity and convenience!
