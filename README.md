# Crystal

A modern web application built with ASP.NET Core 9.0 and React, featuring user authentication powered by Crystal.

## Prerequisites

- .NET SDK 9.0 or later - [Download](https://dotnet.microsoft.com/download)
- Node.js 18+ and npm
- Git

## Getting Started

### 1. Clone and Open the Solution

Open `Crystal.sln` in your preferred IDE (Visual Studio, Rider, or VS Code).

### 2. Install Dependencies

The project uses npm for frontend dependencies. Navigate to the Client.React folder and install:

```bash
cd src/Client.React
npm install
```

The Crystal Client library also needs its dependencies:

```bash
cd ../CrystalClient
npm install
```

### 3. Initialize the Database

Open the command line in the `src/Crystal.WebApi` folder and run:

```bash
dotnet ef database update
```

This creates a SQLite database (`app.db`) in the `src/Crystal.WebApi` folder with all necessary tables.

### 4. Configure JWT Authentication

The JWT Bearer settings are in `appsettings.json` (`src/Crystal.WebApi`). The Issuer and Audience are already set to `Crystal.WebApi` and `Crystal.Client`.

**Important:** Set a secure signing key using User Secrets:

From Visual Studio: Right-click the `Crystal.WebApi` project → `Manage User Secrets`

Or use the command line in the `src/Crystal.WebApi` folder:
```bash
dotnet user-secrets set "Crystal:JwtBearer:SigningKey" "your-secure-256-bit-key-here"
```

> [!CAUTION]
> Never commit the `SigningKey` to source control. It should remain secret.

More about [managing secrets in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/app-secrets)

### 5. Configure HTTPS Certificates

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

### 6. Configure Social Logins (Optional)

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

### Option 1: Automatic (Recommended for Development)

Run the API with the SPA proxy, which automatically starts both the backend and frontend:

```bash
dotnet run --project src/Crystal.WebApi --launch-profile WebApiWithClient
```

This will:
- Start the API on https://localhost:7050
- Automatically start the React dev server on https://localhost:5000
- Enable hot module reloading for React changes

**Access the application at: https://localhost:7050**

### Option 2: Manual (Run Separately)

If you prefer to run them separately:

**Terminal 1 - Start the React dev server:**
```bash
cd src/Client.React
npm run dev
```

**Terminal 2 - Start the API:**
```bash
dotnet run --project src/Crystal.WebApi --launch-profile WebApi
```

### Development URLs:
- **Application:** https://localhost:7050 (recommended)
- **React Dev Server:** https://localhost:5000 (when running separately)
- **Swagger UI:** https://localhost:7050/swagger

## Building for Production

Build the React client:
```bash
cd src/Client.React
npm run build
```

Build the .NET solution:
```bash
dotnet build
```

## Project Structure

```
Crystal/
├── Crystal.sln              # Solution file
├── Directory.Packages.props # Centralized NuGet package versions
└── src/
    ├── CrystalClient/       # Shared TypeScript client library
    ├── Client.React/        # React frontend (Vite + TypeScript + Tailwind)
    ├── Crystal.Core/        # Core authentication library
    ├── Crystal.WebApi/      # ASP.NET Core backend
    ├── Crystal.EntityFrameworkCore/  # EF Core integration
    └── Crystal.FluentEmail/ # Email sending functionality
```

## Features

- User registration and login (email/password)
- OAuth integration (GitHub, Discord)
- Email confirmation workflow
- Password reset functionality
- JWT authentication with refresh tokens
- React SPA with TypeScript
- Tailwind CSS for styling
- SQLite database

## Development Notes

- Email confirmation is disabled in development
- All emails are saved to `src/Crystal.WebApi/logs/emails` in development mode
- SQLite database: `src/Crystal.WebApi/app.db`
- The application uses SPA proxy in development for seamless integration

## Technology Stack

**Backend:**
- ASP.NET Core 9.0
- Entity Framework Core 9.0
- Crystal authentication library
- SQLite database
- Serilog logging
- FluentEmail

**Frontend:**
- React 18.3
- TypeScript 5.6
- Vite 5.4
- React Router 6
- Tailwind CSS 3.4
- React Hook Form + Zod
- Axios for API calls

## Troubleshooting

### "Certificate not found" error
Make sure you've generated the HTTPS certificates:
```bash
dotnet dev-certs https --trust
dotnet dev-certs https --format pem -ep ~/.aspnet/https/crystal.client.pem --no-password
```

### React client won't start
Make sure you've installed all dependencies:
```bash
cd src/Client.React && npm install
cd ../CrystalClient && npm install
```

### Database errors
Reset and recreate the database:
```bash
cd src/Crystal.WebApi
dotnet ef database drop
dotnet ef database update
```

## Attribution

This project is based on the [Crystal Starters](https://github.com/damianostre/aufy-starters) template.
Learn more about Crystal at [https://aufy.dev](https://aufy.dev)
