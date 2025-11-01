# Getting Started

This guide will help you get Crystal up and running on your local machine in minutes.

## Prerequisites

Before you begin, make sure you have:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) - Backend framework
- [Node.js 18+](https://nodejs.org/) - Frontend development
- [Git](https://git-scm.com/) - Version control
- A code editor (VS Code, Visual Studio, Rider, etc.)

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/Egit90/Crystal.git
cd Crystal
```

### 2. Install Backend Dependencies

```bash
cd src/Crystal.WebApi
dotnet restore
```

### 3. Install Frontend Dependencies

```bash
cd ../Client.React
npm install
```

### 4. Configure the Application

Copy the example settings (if needed):

```bash
cd ../Crystal.WebApi
# Create appsettings.Development.json if you need custom settings
```

The default configuration uses SQLite and saves emails to disk, perfect for development.

### 5. Apply Database Migrations

```bash
dotnet ef database update
```

This creates the SQLite database with all necessary tables for authentication.

## Running the Application

You have two options to run the application:

### Option 1: Full Stack (Recommended for Development)

Run both backend and frontend together using the SPA proxy:

```bash
cd src/Crystal.WebApi
dotnet run --launch-profile WebApiWithClient
```

This will:
- Start the .NET API on `https://localhost:7050`
- Automatically launch the React dev server on `https://localhost:5000`
- Open your browser automatically
- Enable hot module replacement (HMR) for React

### Option 2: Separate Processes

#### Terminal 1 - Backend:
```bash
cd src/Crystal.WebApi
dotnet run --launch-profile WebApi
```

#### Terminal 2 - Frontend:
```bash
cd src/Client.React
npm run dev
```

The React app will be available at `http://localhost:5173`.

## First Steps

### 1. Create an Account

1. Navigate to `https://localhost:7050` (or `http://localhost:5173` if running separately)
2. Click "Sign Up"
3. Fill in your email and password
4. Check the console output or `src/Crystal.WebApi/emails/` folder for the confirmation email
5. Click the confirmation link

### 2. Sign In

After confirming your email:
1. Go to "Sign In"
2. Enter your credentials
3. You'll be redirected to the home page

### 3. Try OAuth Login (Optional)

To use GitHub or Discord login, you need to configure OAuth providers:

1. Create a GitHub OAuth App:
   - Go to GitHub Settings > Developer settings > OAuth Apps
   - Create new app with callback: `https://localhost:7050/auth/signin-github`

2. Update `appsettings.json`:
   ```json
   {
     "Crystal": {
       "Providers": {
         "GitHub": {
           "ClientId": "your-client-id",
           "ClientSecret": "your-client-secret"
         }
       }
     }
   }
   ```

## Exploring the Application

### Default Features

- **Authentication**: Email/Password signup and login
- **Email Confirmation**: Check `/emails` folder for confirmation emails
- **Password Reset**: Request password reset from login page
- **Account Management**: Change password, link OAuth accounts
- **Theme Toggle**: Switch between light and dark modes

### Development Tools

- **Swagger UI**: Available at `https://localhost:7050/swagger`
- **Database**: Located at `src/Crystal.WebApi/app.db` (SQLite)
- **Logs**: Check `src/Crystal.WebApi/logs/` folder
- **Emails**: Saved to `src/Crystal.WebApi/emails/` folder

## Common Issues

### Port Already in Use

If port 7050 or 5173 is already in use, you can change them:

**Backend** - Edit `launchSettings.json`:
```json
"applicationUrl": "https://localhost:7051"
```

**Frontend** - Edit `vite.config.ts`:
```typescript
server: {
  port: 5174
}
```

### Database Errors

If you get database errors, try:

```bash
cd src/Crystal.WebApi
dotnet ef database drop
dotnet ef database update
```

### HTTPS Certificate Issues

Trust the .NET development certificate:

```bash
dotnet dev-certs https --trust
```

### NPM Install Fails

Clear npm cache and retry:

```bash
npm cache clean --force
npm install
```

## Next Steps

- Read the [Architecture Overview](./architecture.md) to understand the system
- Learn about [Authentication System](./authentication.md) in detail
- Explore [Project Structure](./project-structure.md) to navigate the codebase
- Check [Configuration Guide](./configuration.md) for advanced settings

## Development Workflow

1. **Make changes** to backend or frontend code
2. **Hot reload** automatically updates the UI (React HMR)
3. **Backend changes** require restarting the API
4. **Database changes** need new migrations:
   ```bash
   dotnet ef migrations add YourMigrationName
   dotnet ef database update
   ```

## Building for Production

### Backend
```bash
cd src/Crystal.WebApi
dotnet publish -c Release
```

### Frontend
```bash
cd src/Client.React
npm run build
```

The frontend will be built into the `dist` folder and served by the .NET API in production.

## Getting Help

- Check the [documentation index](./README.md)
- Review logs in `src/Crystal.WebApi/logs/`
- Check browser console for frontend errors
- Enable debug logging in `appsettings.json`

Happy coding!
