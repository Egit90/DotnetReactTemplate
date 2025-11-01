# Architecture Overview

This document explains Crystal's architecture, design patterns, and how all the pieces fit together.

## High-Level Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                        Browser                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  React 18 + TypeScript + Vite + TailwindCSS          │ │
│  │  ─────────────────────────────────────────────────────  │ │
│  │  • shadcn/ui Components                                │ │
│  │  • React Router                                        │ │
│  │  • Axios for HTTP                                      │ │
│  │  • Theme Provider (Dark/Light)                         │ │
│  └────────────────────────────────────────────────────────┘ │
└───────────────────────┬──────────────────────────────────────┘
                        │
                        │ HTTPS / JSON
                        │
┌───────────────────────▼──────────────────────────────────────┐
│                   ASP.NET Core API                           │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Crystal.WebApi (.NET 9)                              │ │
│  │  ────────────────────────                               │ │
│  │  • Minimal API Endpoints                               │ │
│  │  • JWT Bearer Authentication                           │ │
│  │  • CORS Policy                                         │ │
│  │  • Static File Serving (Production)                    │ │
│  └────────────────────────────────────────────────────────┘ │
│                        │                                     │
│  ┌────────────────────▼──────────────────────────────────┐ │
│  │  Crystal.Core (Business Logic)                        │ │
│  │  ─────────────────────────────                         │ │
│  │  • Authentication Services                             │ │
│  │  • JWT Token Service                                   │ │
│  │  • Email Sender Manager                                │ │
│  │  • Endpoint Definitions                                │ │
│  │  • Identity Integration                                │ │
│  └────────────────────────────────────────────────────────┘ │
│                        │                                     │
│  ┌────────────────────▼──────────────────────────────────┐ │
│  │  Crystal.EntityFrameworkCore (Data Access)            │ │
│  │  ──────────────────────────────────────                │ │
│  │  • ApplicationDbContext                                │ │
│  │  • Repository Implementations                          │ │
│  │  • Migrations                                          │ │
│  └────────────────────────────────────────────────────────┘ │
│                        │                                     │
│  ┌────────────────────▼──────────────────────────────────┐ │
│  │  Crystal.FluentEmail (Email Services)                 │ │
│  │  ─────────────────────────────────                     │ │
│  │  • Email Senders                                       │ │
│  │  • Liquid Templates                                    │ │
│  │  • SMTP / Save to Disk                                 │ │
│  └────────────────────────────────────────────────────────┘ │
└───────────────────────┬──────────────────────────────────────┘
                        │
                        │ SQL
                        │
┌───────────────────────▼──────────────────────────────────────┐
│            SQLite (Dev) / SQL Server (Prod)                  │
│  ────────────────────────────────────────────               │
│  • AspNetUsers                                               │
│  • AspNetRoles                                               │
│  • AspNetUserLogins (OAuth)                                  │
│  • RefreshTokens                                             │
└──────────────────────────────────────────────────────────────┘
```

## Project Structure

```
Crystal/
├── src/
│   ├── Client.React/              # Frontend React app
│   │   ├── src/
│   │   │   ├── components/        # Reusable UI components
│   │   │   ├── features/          # Feature-based modules
│   │   │   │   ├── auth/          # Authentication pages
│   │   │   │   ├── account/       # Account management
│   │   │   │   └── misc/          # Misc pages (Home, etc.)
│   │   │   ├── providers/         # React Context providers
│   │   │   ├── lib/               # Utilities and helpers
│   │   │   └── routes/            # Route configuration
│   │   └── public/                # Static assets
│   │
│   ├── CrystalClient/             # TypeScript API client
│   │   └── src/
│   │       ├── crystal-client.ts  # Main client class
│   │       ├── crystal-storage.ts # Storage abstraction
│   │       └── types.ts           # TypeScript types
│   │
│   ├── Crystal.WebApi/            # Main API project
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── Data/                  # DbContext and models
│   │   ├── Program.cs             # App entry point
│   │   └── ServicesExtensions.cs  # DI configuration
│   │
│   ├── Crystal.Core/              # Business logic layer
│   │   ├── Abstractions/          # Interfaces
│   │   ├── AuthSchemes/           # JWT handlers
│   │   ├── Endpoints/             # API endpoint definitions
│   │   ├── Extensions/            # Service extensions
│   │   ├── Options/               # Configuration options
│   │   └── Services/              # Business services
│   │       ├── EmailSender/
│   │       ├── JwtTokenService.cs
│   │       └── CrystalSignInManager.cs
│   │
│   ├── Crystal.EntityFrameworkCore/ # Data access layer
│   │   ├── Stores/                # EF Core implementations
│   │   └── ApplicationDbContext.cs
│   │
│   └── Crystal.FluentEmail/       # Email services
│       ├── Templates/             # Liquid email templates
│       ├── EmailConfirmationSender.cs
│       └── PasswordResetSender.cs
│
├── docs/                          # Documentation
├── EMAIL-SERVICE.md
└── README.md
```

## Design Patterns & Principles

### 1. Clean Architecture

**Layers**:
1. **Presentation** (`Client.React`, `Crystal.WebApi`) - UI and API
2. **Application** (`Crystal.Core`) - Business logic
3. **Infrastructure** (`Crystal.EntityFrameworkCore`, `Crystal.FluentEmail`) - External concerns
4. **Domain** (in `Crystal.Core/Abstractions`) - Core interfaces

**Dependency Rule**: Dependencies point inward. Core has no dependencies on infrastructure.

### 2. Dependency Injection

Everything is registered and injected via ASP.NET Core DI:

```csharp
// In ServicesExtensions.cs
services.AddScoped<IRefreshTokenManager, RefreshTokenManager>();
services.AddScoped<IJwtTokenService, JwtTokenService>();
services.AddScoped<ICrystalEmailSenderManager<TUser>, CrystalEmailSenderManager<TUser>>();
```

**Benefits**:
- Testability
- Loose coupling
- Easy to swap implementations

### 3. Repository Pattern

Entity Framework Core acts as the repository:

```csharp
public class RefreshTokenStore : IRefreshTokenStore
{
    private readonly ApplicationDbContext _context;

    public async Task<RefreshToken?> GetByTokenIdAsync(string tokenId)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenId == tokenId);
    }
}
```

### 4. Options Pattern

Configuration via strongly-typed options:

```csharp
public class CrystalJwtBearerOptions
{
    public string SigningKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int AccessTokenExpiresInMinutes { get; set; }
}

// Usage
services.Configure<CrystalJwtBearerOptions>(
    configuration.GetSection("Crystal:JwtBearer")
);
```

### 5. Builder Pattern

Fluent API for configuring Crystal:

```csharp
services.AddCrystal<MyUser>(configuration)
    .AddProvider(GitHubAuthenticationDefaults.AuthenticationScheme, ...)
    .AddProvider(DiscordAuthenticationDefaults.AuthenticationScheme, ...)
    .AddDefaultCorsPolicy()
    .AddEntityFrameworkStore<ApplicationDbContext, MyUser>()
    .AddFluentEmail();
```

### 6. Strategy Pattern

Email senders are swappable strategies:

```csharp
// Development: Save to disk
services.AddScoped<ISender>(_ => new SaveToDiskSender("emails"));

// Production: Send via SMTP
services.AddScoped<ISender>(sp => new MailKitSender(options));
```

### 7. Middleware Pipeline

ASP.NET Core middleware for cross-cutting concerns:

```csharp
app.UseHttpsRedirection();    // HTTPS redirect
app.UseStaticFiles();         // Serve static files
app.UseRouting();             // Route matching
app.UseCors();                // CORS policy
app.UseAuthentication();      // JWT authentication
app.UseAuthorization();       // Authorization
app.MapCrystalEndpoints();    // Map API endpoints
```

## Key Components Explained

### Frontend Architecture

#### React Component Structure

```
src/
├── components/          # Reusable UI components
│   ├── ui/             # shadcn/ui components
│   │   ├── button.tsx
│   │   ├── card.tsx
│   │   ├── input.tsx
│   │   └── ...
│   ├── Layout/         # Layout components
│   │   └── MainLayout.tsx
│   └── ThemeToggle.tsx
│
├── features/           # Feature modules
│   ├── auth/          # Authentication feature
│   │   ├── components/
│   │   │   └── SignInForm.tsx
│   │   └── routes/
│   │       ├── SignIn.tsx
│   │       └── SignUp.tsx
│   └── account/       # Account management
│       ├── components/
│       └── routes/
│
├── providers/          # React Context
│   ├── app.tsx        # Root provider
│   ├── AuthProvider.tsx
│   └── ThemeProvider.tsx
│
└── lib/               # Utilities
    └── utils.ts
```

#### State Management

**Authentication State**:
- Managed by `AuthProvider`
- Uses React Context
- Persisted to localStorage via CrystalClient

**Theme State**:
- Managed by `ThemeProvider`
- Uses React Context
- Persisted to localStorage
- Updates `<html>` class for Tailwind dark mode

**Form State**:
- Uses `react-hook-form`
- Validation with built-in validators
- Type-safe with TypeScript

### Backend Architecture

#### Minimal API Endpoints

Crystal uses minimal APIs (not controllers):

```csharp
public class SignInEndpoint<TUser> : IAuthEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/signin", HandleAsync)
            .WithName("SignIn")
            .WithTags("Authentication");
    }

    private async Task<IResult> HandleAsync(
        SignInRequest request,
        IJwtTokenService tokenService,
        /* ... other dependencies ... */)
    {
        // Implementation
    }
}
```

**Registration**:
```csharp
// In ServicesExtensions.cs
services.AddSingleton<IAuthEndpoint, SignInEndpoint<TUser>>();

// In ApplicationBuilderExtensions.cs
public static void MapCrystalEndpoints(this WebApplication app)
{
    var authEndpoints = app.Services.GetServices<IAuthEndpoint>();
    foreach (var endpoint in authEndpoints)
    {
        endpoint.MapEndpoint(app);
    }
}
```

#### Authentication Flow

```
Request
  │
  ├─> JWT Middleware
  │     ├─> Validates token
  │     ├─> Sets ClaimsPrincipal
  │     └─> Returns 401 if invalid
  │
  ├─> Authorization Middleware
  │     └─> Checks [Authorize] attributes
  │
  └─> Endpoint Handler
        └─> Accesses User via HttpContext
```

#### Email Service Architecture

```
ICrystalEmailSenderManager<TUser>
  │
  ├─> ICrystalEmailConfirmationEmailSender<TUser>
  │     └─> EmailConfirmationEmailSender<TUser>
  │           ├─> CrystalFluentEmailFactory
  │           │     └─> Creates IFluentEmail
  │           └─> ISender
  │                 ├─> SaveToDiskSender (dev)
  │                 └─> MailKitSender (prod)
  │
  └─> ICrystalPasswordResetEmailSender<TUser>
        └─> PasswordResetEmailSender<TUser>
```

## Data Flow Examples

### Sign In Flow

```
1. User submits form
   └─> SignInForm.tsx
       └─> authClient.signIn(credentials)

2. API Request
   └─> POST /auth/signin
       └─> SignInEndpoint.HandleAsync()
           ├─> Validates credentials
           ├─> Generates JWT tokens
           └─> Returns TokenResponse

3. Response Handling
   └─> CrystalClient.signIn()
       ├─> Stores access token
       ├─> Stores user info
       ├─> Triggers onSignIn event
       └─> Returns user

4. UI Update
   └─> AuthProvider
       ├─> Updates auth state
       ├─> Triggers re-render
       └─> Navigates to home
```

### Protected API Call

```
1. Component needs data
   └─> useEffect(() => fetchData(), [])

2. API Request
   └─> axios.get('/api/protected')
       └─> Request Interceptor
           └─> Adds: Authorization: Bearer <token>

3. Backend Validation
   └─> JWT Middleware
       ├─> Validates token signature
       ├─> Checks expiration
       └─> Sets HttpContext.User

4. Endpoint Handler
   └─> var userId = User.FindFirst("sub").Value
       └─> Returns data

5. Response
   └─> Component receives data
       └─> Updates state
       └─> Renders UI
```

## Configuration System

### Hierarchy

```
appsettings.json (Base)
  ↓
appsettings.Development.json
  ↓
Environment Variables
  ↓
User Secrets (Development only)
  ↓
Command Line Arguments
```

### Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Crystal": {
    "ClientApp": { ... },
    "JwtBearer": { ... },
    "Providers": { ... }
  },
  "FluentEmail": { ... },
  "Logging": { ... },
  "Serilog": { ... }
}
```

### Accessing Configuration

```csharp
// Option 1: Inject IConfiguration
public MyService(IConfiguration configuration)
{
    var value = configuration["Crystal:JwtBearer:SigningKey"];
}

// Option 2: Use Options Pattern (Recommended)
public MyService(IOptions<CrystalJwtBearerOptions> options)
{
    var key = options.Value.SigningKey;
}
```

## Security Architecture

### Defense in Depth

1. **HTTPS Only**: All traffic encrypted
2. **JWT Validation**: Signature verification
3. **Token Expiration**: Short-lived access tokens
4. **HTTP-Only Cookies**: Refresh tokens not accessible to JavaScript
5. **CORS Policy**: Restricts cross-origin requests
6. **SQL Injection**: EF Core parameterized queries
7. **Password Hashing**: ASP.NET Core Identity (PBKDF2)
8. **Email Confirmation**: Prevents fake accounts
9. **Rate Limiting**: Can be added via middleware

### Trust Boundaries

```
Public Internet
  │ HTTPS
  ▼
API Gateway (CORS, HTTPS)
  │
  ▼
Authentication Middleware (JWT)
  │
  ▼
Authorization Middleware (Claims)
  │
  ▼
Endpoint Handlers
  │
  ▼
Database (Encrypted at rest)
```

## Testing Strategy

### Unit Tests
- Test business logic in `Crystal.Core`
- Mock dependencies via interfaces
- Test JWT token generation/validation

### Integration Tests
- Test API endpoints
- Use in-memory database
- Test authentication flows

### End-to-End Tests
- Test full user workflows
- Use Playwright/Cypress
- Test OAuth flows

## Deployment Architecture

### Development

```
Developer Machine
├─> .NET API (https://localhost:7050)
├─> Vite Dev Server (http://localhost:5173)
├─> SQLite Database (file)
└─> Emails saved to disk
```

### Production

```
Azure / AWS / On-Premise
├─> Load Balancer
│   └─> ASP.NET Core App
│       ├─> Serves API
│       ├─> Serves static React files
│       └─> Connects to SQL Server
└─> SQL Server Database
```

## Performance Considerations

### Frontend
- **Code Splitting**: Vite automatically splits bundles
- **Lazy Loading**: Routes loaded on demand
- **Tree Shaking**: Unused code eliminated
- **Asset Optimization**: Images, fonts optimized

### Backend
- **Database Indexing**: Indexes on foreign keys, email
- **Connection Pooling**: EF Core manages connections
- **Async/Await**: Non-blocking I/O operations
- **Caching**: Can add Redis for distributed cache

### API Optimization
- **Minimal APIs**: Lower overhead than controllers
- **Response Compression**: Can enable gzip/brotli
- **CDN**: Static files served from CDN in production

## Extensibility Points

### Add New Authentication Provider

```csharp
services.AddCrystal<MyUser>(configuration)
    .AddProvider(GoogleDefaults.AuthenticationScheme, (auth, opts) =>
    {
        auth.AddGoogle(o => o.Configure(GoogleDefaults.AuthenticationScheme, opts));
    });
```

### Add New Email Type

1. Add to `EmailType` enum
2. Create sender class implementing interface
3. Register in DI
4. Add template

### Add Authorization

```csharp
// Add roles
services.AddIdentityCore<MyUser>()
    .AddRoles<IdentityRole>();

// Require roles
app.MapGet("/admin/users", () => { })
   .RequireAuthorization(policy => policy.RequireRole("Admin"));
```

### Add Custom Endpoints

```csharp
public class MyCustomEndpoint : IAuthEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/my-endpoint", HandleAsync);
    }
}

services.AddSingleton<IAuthEndpoint, MyCustomEndpoint>();
```

## Summary

Crystal's architecture is:

- **Clean**: Separation of concerns, clear boundaries
- **Modular**: Features organized by domain
- **Testable**: Dependency injection, interfaces
- **Extensible**: Easy to add providers, endpoints, features
- **Secure**: Multiple layers of defense
- **Performant**: Optimized for production use
- **Maintainable**: Clear patterns, good documentation

All built on modern .NET 9 and React 18 best practices!
