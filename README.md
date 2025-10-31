# Crystal

A modern web application built with ASP.NET Core 9.0 and React, featuring user authentication powered by Crystal.

## Prerequisites

- .NET SDK 9.0 or later - [Download](https://dotnet.microsoft.com/download)
- Node.js 18+ and npm
- Git

## Getting Started

### 1. Open the Solution

Open `Crystal.sln` in your preferred IDE (Visual Studio, Rider, or VS Code).

### 2. Initialize the Database

Open the command line in the `src/WebApi` folder and run:

```bash
dotnet ef database update
```

This creates a SQLite database (`app.db`) in the `src/WebApi` folder with all necessary tables.

### 3. Configure JWT Authentication

The JWT Bearer settings are in `appsettings.json` (`src/WebApi`). The Issuer and Audience are already set to `Crystal.WebApi` and `Crystal.Client`.

**Important:** Set a secure signing key using User Secrets:

From Visual Studio: Right-click the `WebApi` project → `Manage User Secrets`

Or use the command line:
```bash
dotnet user-secrets set "Crystal:JwtBearer:SigningKey" "your-secure-256-bit-key-here"
```

> [!CAUTION]
> Never commit the `SigningKey` to source control. It should remain secret.

More about [managing secrets in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/app-secrets)

### 4. Configure HTTPS Certificates

Both WebApi and Client projects use HTTPS. Trust the development certificates:

**Windows & macOS:**
```bash
dotnet dev-certs https --trust
```

**Linux:**
Trust certificates manually for web browsers ([guide](https://stackoverflow.com/questions/72226270/valid-https-certificate-for-dotnet-development-on-localhost-ubuntu))

For the client certificate on Linux:
```bash
dotnet dev-certs https --format pem -ep ~/.aspnet/https/crystal.client.pem --no-password
```



### 5. Configure Social Logins (Optional)

The app supports Discord and GitHub OAuth. Callback URLs:
- Discord: `https://localhost:7050/api/auth/external/callback/discord`
- GitHub: `https://localhost:7050/api/auth/external/callback/github`

Add your OAuth credentials to User Secrets:

```json
{
  "Crystal:Providers:GitHub:ClientId": "your-github-client-id",
  "Crystal:Providers:GitHub:ClientSecret": "your-github-client-secret",

  "Crystal:Providers:Discord:ClientId": "your-discord-client-id",
  "Crystal:Providers:Discord:ClientSecret": "your-discord-client-secret"
}
```

## Running the Application

The `WebApi` project has two launch profiles:

- **WebApiWithClient** - Runs both WebApi and React client via SPA Proxy (recommended)
- **WebApi** - Runs only the API; run the client separately with `npm run dev` in `src/Client.React`

### Development URLs:
- **WebApi:** https://localhost:7050
- **React Client:** https://localhost:5000
- **Swagger UI:** https://localhost:7050/swagger

## Project Structure

```
Crystal/
├── Crystal.sln           # Solution file
├── Directory.Packages.props  # Centralized NuGet package versions
└── src/
    ├── CrystalClient/       # Shared TypeScript client library
    ├── Client.React/     # React frontend (Vite + TypeScript + Tailwind)
    ├── Crystal.Core/     # Core shared library
    └── WebApi/           # ASP.NET Core backend
```

## Features

- User registration and login (email/password)
- OAuth integration (GitHub, Discord)
- Email confirmation workflow
- Password reset functionality
- JWT authentication
- React SPA with TypeScript
- Tailwind CSS for styling
- SQLite database

## Development Notes

- Email confirmation is disabled in development
- All emails are saved to `src/WebApi/logs/emails` in development mode
- SQLite database: `src/WebApi/app.db`

## Technology Stack

**Backend:**
- ASP.NET Core 9.0
- Entity Framework Core 9.0
- Crystal authentication library
- SQLite database
- Serilog logging

**Frontend:**
- React 18.3
- TypeScript 5.6
- Vite 5.4
- React Router 6
- Tailwind CSS 3.4
- React Hook Form + Zod

## Attribution

This project is based on the [Crystal Starters](https://github.com/damianostre/aufy-starters) template.
Learn more about Crystal at [https://aufy.dev](https://aufy.dev)