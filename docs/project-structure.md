# Project Structure

This guide explains the organization of the Crystal codebase and the purpose of each file and folder.

## Root Directory

```
Crystal/
├── .gitignore
├── Crystal.sln                  # Solution file
├── Directory.Packages.props     # Central package version management
├── LICENSE
├── README.md                    # Main README
├── EMAIL-SERVICE.md             # Email service documentation
├── docs/                        # Documentation folder
│   ├── README.md
│   ├── getting-started.md
│   ├── architecture.md
│   ├── authentication.md
│   ├── launch-profiles.md
│   ├── project-structure.md
│   └── configuration.md
└── src/                         # Source code
```

## Source Code Structure

```
src/
├── Client.React/                # Frontend React application
├── CrystalClient/               # TypeScript API client library
├── Crystal.WebApi/              # Main API entry point
├── Crystal.Core/                # Business logic
├── Crystal.EntityFrameworkCore/ # Data access
└── Crystal.FluentEmail/         # Email services
```

## Frontend: Client.React

```
Client.React/
├── public/                      # Static assets
│   └── vite.svg
│
├── src/
│   ├── main.tsx                 # Entry point
│   ├── App.tsx                  # Root component
│   ├── index.css                # Global styles
│   │
│   ├── components/              # Reusable UI components
│   │   ├── ui/                  # shadcn/ui components
│   │   │   ├── button.tsx
│   │   │   ├── card.tsx
│   │   │   ├── input.tsx
│   │   │   ├── label.tsx
│   │   │   ├── dropdown-menu.tsx
│   │   │   ├── badge.tsx
│   │   │   └── sonner.tsx      # Toast notifications
│   │   ├── Layout/
│   │   │   └── MainLayout.tsx  # App layout with header
│   │   ├── ThemeToggle.tsx     # Dark/light mode toggle
│   │   └── ProtectedRoute.tsx  # Auth guard for routes
│   │
│   ├── features/                # Feature-based organization
│   │   ├── auth/               # Authentication feature
│   │   │   ├── components/
│   │   │   │   └── SignInForm.tsx
│   │   │   └── routes/
│   │   │       ├── SignIn.tsx
│   │   │       ├── SignUp.tsx
│   │   │       ├── SignUpConfirmation.tsx
│   │   │       └── ExternalChallengeCallback.tsx
│   │   │
│   │   ├── account/            # Account management
│   │   │   ├── components/
│   │   │   │   └── ChangePasswordForm.tsx
│   │   │   └── routes/
│   │   │       ├── MyAccount.tsx
│   │   │       ├── ConfirmEmail.tsx
│   │   │       ├── ForgotPassword.tsx
│   │   │       └── ResetPassword.tsx
│   │   │
│   │   └── misc/               # Miscellaneous pages
│   │       └── routes/
│   │           ├── Home.tsx
│   │           └── NotFound.tsx
│   │
│   ├── providers/              # React Context providers
│   │   ├── app.tsx            # Root provider wrapper
│   │   ├── AuthProvider.tsx   # Authentication state
│   │   └── ThemeProvider.tsx  # Theme state
│   │
│   ├── routes/                 # Route configuration
│   │   ├── index.tsx          # Main router
│   │   └── ProtectedRoute.tsx
│   │
│   └── lib/                    # Utilities and helpers
│       ├── utils.ts           # Utility functions (cn, etc.)
│       └── api-error-handler.ts
│
├── .eslintrc.cjs               # ESLint configuration
├── .prettierrc                 # Prettier configuration
├── index.html                  # HTML template
├── package.json                # NPM dependencies
├── tsconfig.json               # TypeScript configuration
├── tsconfig.node.json
├── vite.config.ts              # Vite bundler configuration
├── tailwind.config.js          # Tailwind CSS configuration
├── postcss.config.js           # PostCSS configuration
└── components.json             # shadcn/ui configuration
```

### Key Frontend Files

#### `src/main.tsx`
Application entry point. Renders the root component.

#### `src/App.tsx`
Root component with router and providers.

#### `src/providers/app.tsx`
Wraps app with all providers:
- ThemeProvider
- AuthProvider
- Router
- Toaster (notifications)

#### `src/lib/utils.ts`
Utility functions:
- `cn()` - Class name merging for Tailwind
- Helper functions

#### `vite.config.ts`
Vite configuration:
- Path aliases (`@/`)
- Server port (5173)
- Proxy configuration
- Build settings

#### `tailwind.config.js`
Tailwind configuration:
- Theme colors (primary, secondary, etc.)
- Dark mode strategy
- Custom utilities

## API Client: CrystalClient

```
CrystalClient/
├── src/
│   ├── crystal-client.ts       # Main API client class
│   ├── crystal-storage.ts      # Storage abstraction
│   ├── axios-utils.ts          # Axios configuration
│   ├── types.ts                # TypeScript type definitions
│   └── index.ts                # Public exports
│
├── package.json                # NPM package configuration
├── tsconfig.json               # TypeScript configuration
└── README.md                   # Client library documentation
```

### Key Client Files

#### `crystal-client.ts`
Main `CrystalClient` class with methods:
- `signUp()`, `signIn()`, `signOut()`
- `externalChallenge()`, `signInExternal()`
- `forgotPassword()`, `resetPassword()`
- `confirmEmail()`, `accountInfo()`
- `refreshToken()`, `whoAmI()`
- `initAxiosInterceptors()` - Sets up token handling

#### `crystal-storage.ts`
Storage interface and default localStorage implementation:
- `getToken()`, `setToken()`, `clearToken()`
- `getUser()`, `setUser()`, `clearUser()`

#### `types.ts`
TypeScript interfaces for all API requests and responses.

## Backend: Crystal.WebApi

```
Crystal.WebApi/
├── Properties/
│   └── launchSettings.json     # Launch profiles configuration
│
├── Data/
│   ├── ApplicationDbContext.cs # EF Core DbContext
│   ├── MyUser.cs               # User entity model
│   └── Migrations/             # EF Core migrations
│
├── emails/                     # Email output folder (dev)
├── logs/                       # Application logs
│   └── log.txt
│
├── Program.cs                  # Application entry point
├── ServicesExtensions.cs       # DI setup for Crystal
├── appsettings.json            # Configuration
├── appsettings.Development.json # Dev-specific config
├── app.db                      # SQLite database (dev)
├── Crystal.WebApi.csproj       # Project file
└── .config/
    └── dotnet-tools.json       # .NET tools (EF Core CLI)
```

### Key Backend Files

#### `Program.cs`
Application startup:
- Configure services
- Setup middleware pipeline
- Map endpoints
- Run app

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.SetupCrystal(builder.Configuration);
var app = builder.Build();
app.MapCrystalEndpoints();
app.Run();
```

#### `ServicesExtensions.cs`
Crystal configuration:
- `SetupCrystal()` - Main setup method
- OAuth provider setup (GitHub, Discord)
- CORS policy
- EF Core store
- FluentEmail

#### `Data/ApplicationDbContext.cs`
Entity Framework DbContext:
- Inherits from `IdentityDbContext`
- Configures database schema
- DbSet properties

#### `Data/MyUser.cs`
User entity:
- Extends `IdentityUser`
- Implements `ICrystalUser`
- Custom user properties

#### `appsettings.json`
Application configuration:
- Connection strings
- Crystal settings (JWT, OAuth, etc.)
- FluentEmail settings
- Logging configuration

## Core Library: Crystal.Core

```
Crystal.Core/
├── Abstractions/               # Interfaces
│   ├── ICrystalUser.cs
│   ├── ICrystalUserManager.cs
│   ├── IJwtTokenService.cs
│   ├── IRefreshTokenStore.cs
│   └── IRefreshTokenManager.cs
│
├── AuthSchemes/                # JWT authentication handlers
│   ├── CrystalAuthSchemeDefaults.cs
│   ├── CrystalSignInJwtBearerHandler.cs
│   ├── CrystalTokenJwtBearerHandler.cs
│   └── CrystalPolicySignInExternalHandler.cs
│
├── Endpoints/                  # API endpoint definitions
│   ├── IAuthEndpoint.cs
│   ├── IAccountEndpoint.cs
│   ├── Auth/
│   │   ├── SignInEndpoint.cs
│   │   ├── SignUpEndpoint.cs
│   │   ├── SignOutEndpoint.cs
│   │   ├── SignInRefreshEndpoint.cs
│   │   ├── WhoAmIEndpoint.cs
│   │   ├── ExternalChallengeEndpoint.cs
│   │   ├── SignInExternalEndpoint.cs
│   │   └── ...
│   └── Account/
│       ├── AccountInfoEndpoint.cs
│       ├── EmailConfirmEndpoint.cs
│       ├── PasswordChangeEndpoint.cs
│       ├── PasswordResetEndpoint.cs
│       └── ...
│
├── Extensions/                 # Extension methods
│   ├── ServicesExtensions.cs  # DI registration
│   ├── ApplicationBuilderExtensions.cs
│   └── IdentityResultExtensions.cs
│
├── Options/                    # Configuration option classes
│   ├── CrystalOptions.cs
│   ├── CrystalJwtBearerOptions.cs
│   └── CrystalClientAppOptions.cs
│
├── Services/                   # Business logic services
│   ├── JwtTokenService.cs
│   ├── RefreshTokenManager.cs
│   ├── CrystalSignInManager.cs
│   ├── CrystalUserManager.cs
│   └── EmailSender/
│       ├── ICrystalEmailSenderManager.cs
│       ├── ICrystalEmailConfirmationEmailSender.cs
│       ├── ICrystalPasswordResetEmailSender.cs
│       ├── CrystalEmailSenderManager.cs
│       └── EmailType.cs
│
└── Crystal.Core.csproj
```

### Key Core Files

#### `Services/JwtTokenService.cs`
JWT token generation and validation:
- `GenerateTokenAsync()` - Create access & refresh tokens
- Token signing with HMAC-SHA256

#### `Services/CrystalSignInManager.cs`
Custom SignInManager:
- Extends ASP.NET Core `SignInManager`
- Custom sign-in logic

#### `Endpoints/Auth/SignInEndpoint.cs`
Sign-in endpoint implementation:
- Validates credentials
- Generates tokens
- Returns token response

#### `Extensions/ServicesExtensions.cs`
`AddCrystal<TUser>()` method:
- Registers all services
- Configures Identity
- Sets up JWT authentication
- Returns builder for fluent API

## Data Access: Crystal.EntityFrameworkCore

```
Crystal.EntityFrameworkCore/
├── Stores/
│   ├── RefreshTokenStore.cs    # Refresh token repository
│   └── ...
│
├── CrystalServiceBuilderExtensions.cs
└── Crystal.EntityFrameworkCore.csproj
```

### Key EF Core Files

#### `Stores/RefreshTokenStore.cs`
Repository for refresh tokens:
- `GetByTokenIdAsync()`
- `AddAsync()`
- `RemoveAsync()`
- Uses EF Core for data access

## Email Services: Crystal.FluentEmail

```
Crystal.FluentEmail/
├── Templates/                  # Liquid email templates
│   ├── EmailConfirmation.template.html
│   └── PasswordReset.template.html
│
├── CrystalServiceBuilderExtensions.cs
├── CrystalFluentEmailFactory.cs
├── FluentEmailOptions.cs
├── EmailConfirmationEmailSender.cs
├── PasswordResetEmailSender.cs
└── Crystal.FluentEmail.csproj
```

### Key Email Files

#### `EmailConfirmationEmailSender.cs`
Sends email confirmation emails:
- Uses FluentEmail
- Renders Liquid template
- Implements `ICrystalEmailConfirmationEmailSender`

#### `Templates/EmailConfirmation.template.html`
Liquid template for confirmation emails:
- Variables: `{{ConfirmationLink}}`, `{{Email}}`, `{{UserName}}`
- Modern HTML design with inline styles

#### `CrystalFluentEmailFactory.cs`
Factory for creating FluentEmail instances:
- Configures Liquid renderer
- Injects ISender (SMTP or SaveToDisk)

## Central Package Management

### `Directory.Packages.props`
Centralizes NuGet package versions:

```xml
<PackageVersion Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageVersion Include="Serilog.AspNetCore" Version="9.0.0" />
```

Benefits:
- Single source of truth for versions
- Easy to update all projects
- Prevents version conflicts

## Build Output

### Development
```
Crystal.WebApi/
├── bin/Debug/net9.0/
│   ├── Crystal.WebApi.dll
│   └── ...
├── obj/
├── app.db                  # SQLite database
├── logs/                   # Application logs
└── emails/                 # Saved emails (dev)
```

### Production Build
```
Crystal.WebApi/
└── bin/Release/net9.0/
    ├── publish/
    │   ├── Crystal.WebApi.dll
    │   ├── wwwroot/           # Static React files
    │   │   ├── index.html
    │   │   ├── assets/
    │   │   └── ...
    │   └── ...
```

## Configuration Files

### `.gitignore`
Excludes from version control:
- `bin/`, `obj/`
- `node_modules/`
- `*.db`, `*.db-shm`, `*.db-wal`
- `appsettings.Development.json` (if it contains secrets)
- `emails/`, `logs/`

### `Crystal.sln`
Solution file containing all projects.

### `.editorconfig` (optional)
Code style and formatting rules.

## File Naming Conventions

### Backend (C#)
- **PascalCase** for file names: `SignInEndpoint.cs`
- One class per file (usually)
- Interfaces prefixed with `I`: `ICrystalUser.cs`

### Frontend (TypeScript/React)
- **PascalCase** for components: `SignInForm.tsx`
- **camelCase** for utilities: `utils.ts`
- **kebab-case** for CSS: `index.css`

## Summary

Crystal's project structure follows:

- **Feature-based organization** (Frontend)
- **Clean Architecture layers** (Backend)
- **Separation of concerns** (Projects)
- **Consistent naming** (Conventions)
- **Modular design** (Extensibility)

All designed for maintainability and scalability!
