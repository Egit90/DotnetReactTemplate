# Configuration Guide

This guide explains all configuration options in Crystal and how to customize the application for different environments.

## Configuration Overview

Crystal uses ASP.NET Core's configuration system with multiple sources:

```
Priority (lowest to highest):
1. appsettings.json (base configuration)
2. appsettings.{Environment}.json
3. User Secrets (Development only)
4. Environment Variables
5. Command-line arguments
```

## Configuration Files

### `appsettings.json` (Base Configuration)

Located at: `src/Crystal.WebApi/appsettings.json`

This is the main configuration file committed to source control. Contains:
- Default settings
- Non-sensitive configuration
- Structure for all options

**Full example**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=app.db;Cache=Shared"
  },
  "Crystal": {
    "ClientApp": {
      "BaseUrl": "http://localhost:5173",
      "EmailConfirmationPath": "/confirm-email"
    },
    "JwtBearer": {
      "SigningKey": "base64-encoded-key-here",
      "Issuer": "Crystal.WebApi",
      "Audience": "Crystal.Client",
      "AccessTokenExpiresInMinutes": 5,
      "RefreshTokenExpiresInHours": 48
    },
    "Providers": {
      "GitHub": {
        "ClientId": "!---SECRET-KEY---!",
        "ClientSecret": "!---SECRET-KEY---!",
        "Scopes": [
          "user:email",
          "read:user"
        ]
      },
      "Discord": {
        "ClientId": "!---SECRET-KEY---!",
        "ClientSecret": "!---SECRET-KEY---!",
        "Scopes": [
          "email"
        ]
      }
    }
  },
  "FluentEmail": {
    "SaveEmailsOnDisk": "emails",
    "FromEmail": "noreply@crystal.com",
    "FromName": "Crystal App"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Warning",
      "Crystal": "Debug"
    }
  },
  "Serilog": {
    "Using": [
      "Serilog.Sinks.File"
    ],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/log.txt"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithThreadId"
    ],
    "Properties": {
      "Application": "WebApi"
    }
  }
}
```

### `appsettings.Development.json` (Development Overrides)

Contains development-specific settings (auto-loaded in Development environment):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=app.db;Cache=Shared"
  },
  "Crystal": {
    "JwtBearer": {
      "SigningKey": "development-signing-key"
    }
  },
  "FluentEmail": {
    "SaveEmailsOnDisk": "emails"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Crystal": "Debug"
    }
  }
}
```

### `appsettings.Production.json` (Production Overrides)

For production deployments:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=Crystal;User Id=sa;Password=***;"
  },
  "Crystal": {
    "ClientApp": {
      "BaseUrl": "https://yourdomain.com"
    }
  },
  "FluentEmail": {
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "SmtpUsername": "apikey",
    "SmtpPassword": "***",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "Your App"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Crystal": "Information"
    }
  }
}
```

## Configuration Sections

### Connection Strings

Database connection configuration.

**SQLite (Development)**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=app.db;Cache=Shared"
  }
}
```

**SQL Server (Production)**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Crystal;Integrated Security=true;"
  }
}
```

**PostgreSQL**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=crystal;Username=postgres;Password=***"
  }
}
```

**MySQL**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=crystal;User=root;Password=***;"
  }
}
```

### Crystal Configuration

Main application settings under the `Crystal` section.

#### ClientApp Settings

Frontend URL configuration for email links:

```json
{
  "Crystal": {
    "ClientApp": {
      "BaseUrl": "http://localhost:5173",
      "EmailConfirmationPath": "/confirm-email"
    }
  }
}
```

**Properties**:
- `BaseUrl` - Frontend application URL
- `EmailConfirmationPath` - Path for email confirmation page

**Environment-specific**:
- Development: `http://localhost:5173`
- Production: `https://yourdomain.com`

#### JWT Bearer Settings

JWT token configuration:

```json
{
  "Crystal": {
    "JwtBearer": {
      "SigningKey": "your-512-bit-base64-encoded-key",
      "Issuer": "Crystal.WebApi",
      "Audience": "Crystal.Client",
      "AccessTokenExpiresInMinutes": 5,
      "RefreshTokenExpiresInHours": 48
    }
  }
}
```

**Properties**:
- `SigningKey` - 512-bit key for signing tokens (see [Generating Keys](#generating-signing-keys))
- `Issuer` - Token issuer (usually API name)
- `Audience` - Token audience (usually client name)
- `AccessTokenExpiresInMinutes` - Access token lifetime (default: 5 minutes)
- `RefreshTokenExpiresInHours` - Refresh token lifetime (default: 48 hours)

**Security**:
- Use different keys for dev/prod
- Store production keys in environment variables or Key Vault
- Minimum key size: 512 bits (64 bytes)

#### OAuth Provider Settings

OAuth provider credentials:

```json
{
  "Crystal": {
    "Providers": {
      "GitHub": {
        "ClientId": "your-github-client-id",
        "ClientSecret": "your-github-client-secret",
        "Scopes": ["user:email", "read:user"]
      },
      "Discord": {
        "ClientId": "your-discord-client-id",
        "ClientSecret": "your-discord-client-secret",
        "Scopes": ["email"]
      }
    }
  }
}
```

**Setup GitHub OAuth**:
1. Go to GitHub Settings > Developer settings > OAuth Apps
2. Click "New OAuth App"
3. Fill in:
   - Application name: `Crystal (Dev)`
   - Homepage URL: `http://localhost:5173`
   - Callback URL: `https://localhost:7050/signin-github`
4. Copy Client ID and Client Secret to configuration

**Setup Discord OAuth**:
1. Go to [Discord Developer Portal](https://discord.com/developers/applications)
2. Click "New Application"
3. Go to OAuth2 settings
4. Add redirect URL: `https://localhost:7050/signin-discord`
5. Copy Client ID and Client Secret

### FluentEmail Configuration

Email service settings:

```json
{
  "FluentEmail": {
    "SaveEmailsOnDisk": "emails",
    "FromEmail": "noreply@crystal.com",
    "FromName": "Crystal App",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "Emails": {
      "Confirmation": {
        "Subject": "Confirm your email address",
        "TemplatePath": "Templates/CustomEmailConfirmation.html"
      },
      "PasswordReset": {
        "Subject": "Reset your password",
        "TemplatePath": "Templates/CustomPasswordReset.html"
      }
    }
  }
}
```

**Properties**:
- `SaveEmailsOnDisk` - Folder to save emails (dev mode, removes SMTP requirement)
- `FromEmail` - Sender email address
- `FromName` - Sender display name
- `SmtpHost` - SMTP server hostname
- `SmtpPort` - SMTP port (usually 587)
- `SmtpUsername` - SMTP username
- `SmtpPassword` - SMTP password
- `Emails` - Per-email-type settings (optional)

**Development Mode**:
When `SaveEmailsOnDisk` is set, emails are saved as HTML files instead of being sent via SMTP.

**Production Mode**:
Remove `SaveEmailsOnDisk` and configure SMTP settings.

### Logging Configuration

ASP.NET Core logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
      "Crystal": "Debug"
    }
  }
}
```

**Log Levels**:
- `Trace` - Most detailed
- `Debug` - Debugging information
- `Information` - General flow
- `Warning` - Unusual events
- `Error` - Errors and exceptions
- `Critical` - Critical failures
- `None` - Disable logging

### Serilog Configuration

Structured logging with Serilog:

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.File", "Serilog.Sinks.Console"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/log.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      },
      {
        "Name": "Console"
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"],
    "Properties": {
      "Application": "Crystal.WebApi"
    }
  }
}
```

**Sinks** (destinations):
- `File` - Write to file
- `Console` - Write to console
- `Seq` - Structured logging server
- `Elasticsearch` - Elasticsearch

**File Sink Options**:
- `path` - Log file path
- `rollingInterval` - Create new file daily/hourly
- `retainedFileCountLimit` - Keep only N files

## Environment-Specific Configuration

### Development Environment

**How to use**:
```bash
export ASPNETCORE_ENVIRONMENT=Development  # Linux/Mac
set ASPNETCORE_ENVIRONMENT=Development     # Windows
```

Or in `launchSettings.json`:
```json
{
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development"
  }
}
```

**Automatically loads**:
- `appsettings.json`
- `appsettings.Development.json`
- User Secrets

**Best for**:
- SQLite database
- Email save to disk
- Verbose logging
- Development signing keys

### Production Environment

**Set environment**:
```bash
export ASPNETCORE_ENVIRONMENT=Production
```

**Automatically loads**:
- `appsettings.json`
- `appsettings.Production.json`
- Environment variables

**Best for**:
- SQL Server / PostgreSQL
- SMTP email sending
- Minimal logging
- Secure signing keys from Key Vault

### Staging Environment

Create `appsettings.Staging.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=staging-db;..."
  }
}
```

Set environment:
```bash
export ASPNETCORE_ENVIRONMENT=Staging
```

## Sensitive Configuration

### User Secrets (Development)

Store sensitive data locally without committing to source control.

**Initialize**:
```bash
cd src/Crystal.WebApi
dotnet user-secrets init
```

**Set secrets**:
```bash
dotnet user-secrets set "Crystal:JwtBearer:SigningKey" "your-dev-key"
dotnet user-secrets set "Crystal:Providers:GitHub:ClientSecret" "your-secret"
dotnet user-secrets set "FluentEmail:SmtpPassword" "your-password"
```

**List secrets**:
```bash
dotnet user-secrets list
```

**Clear secrets**:
```bash
dotnet user-secrets clear
```

**Location**:
- Windows: `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`
- Linux/Mac: `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

### Environment Variables (Production)

**Set environment variables**:

**Linux/Mac**:
```bash
export ConnectionStrings__DefaultConnection="Server=prod;..."
export Crystal__JwtBearer__SigningKey="prod-key"
export FluentEmail__SmtpPassword="smtp-password"
```

**Windows**:
```cmd
set ConnectionStrings__DefaultConnection=Server=prod;...
set Crystal__JwtBearer__SigningKey=prod-key
```

**Docker**:
```dockerfile
ENV ConnectionStrings__DefaultConnection="Server=db;..."
ENV Crystal__JwtBearer__SigningKey="key"
```

**Note**: Use double underscores (`__`) to represent nested configuration.

### Azure Key Vault (Production)

Store secrets in Azure Key Vault.

**Install package**:
```bash
dotnet add package Azure.Identity
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
```

**Configure in Program.cs**:
```csharp
var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var keyVaultUri = new Uri("https://your-keyvault.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(
        keyVaultUri,
        new DefaultAzureCredential());
}
```

**Set secrets in Key Vault**:
```bash
az keyvault secret set --vault-name your-keyvault --name "Crystal--JwtBearer--SigningKey" --value "key"
```

## Configuration Helpers

### Generating Signing Keys

**Generate 512-bit key**:

```bash
# Using OpenSSL (Recommended)
openssl rand -base64 64

# Using PowerShell
[Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))

# Using .NET (C# Interactive)
Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
```

Copy the output to `Crystal:JwtBearer:SigningKey`.

### Testing Configuration

**View effective configuration**:

```csharp
// In Program.cs (temporary for debugging)
var config = builder.Configuration;
var connectionString = config.GetConnectionString("DefaultConnection");
var signingKey = config["Crystal:JwtBearer:SigningKey"];

Console.WriteLine($"Connection: {connectionString}");
Console.WriteLine($"SigningKey: {signingKey?.Substring(0, 10)}...");
```

**Access in code**:

```csharp
// Option 1: Direct access
public MyService(IConfiguration configuration)
{
    var value = configuration["Crystal:JwtBearer:SigningKey"];
}

// Option 2: Options pattern (Recommended)
public MyService(IOptions<CrystalJwtBearerOptions> options)
{
    var key = options.Value.SigningKey;
}
```

## Frontend Configuration

### Vite Configuration

Located at: `src/Client.React/vite.config.ts`

```typescript
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: 'https://localhost:7050',
        secure: false
      }
    }
  },
  build: {
    outDir: 'dist',
    sourcemap: false
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src')
    }
  }
})
```

**Properties**:
- `server.port` - Dev server port
- `server.proxy` - Proxy API requests
- `build.outDir` - Build output folder
- `resolve.alias` - Path aliases

### Environment Variables (.env)

Create `.env` file in `Client.React/`:

```bash
VITE_API_BASE_URL=https://localhost:7050
VITE_APP_NAME=Crystal
```

**Access in code**:
```typescript
const apiUrl = import.meta.env.VITE_API_BASE_URL;
```

**Environment-specific**:
- `.env` - All environments
- `.env.local` - Local overrides (not committed)
- `.env.development` - Development only
- `.env.production` - Production only

## Configuration Best Practices

### DO ✅

1. **Commit `appsettings.json`** with default/non-sensitive values
2. **Use User Secrets** for local development secrets
3. **Use Environment Variables** in production
4. **Use different keys** for dev/prod
5. **Document all settings** with comments or README
6. **Validate configuration** at startup
7. **Use Options pattern** to access configuration

### DON'T ❌

1. **Don't commit secrets** to source control
2. **Don't hardcode** values in code
3. **Don't use same keys** across environments
4. **Don't expose secrets** in logs or error messages
5. **Don't share production** credentials
6. **Don't forget to rotate** keys periodically

## Troubleshooting

### Configuration not loading

**Check**:
1. File name matches environment: `appsettings.Production.json`
2. File set to "Copy to Output Directory" in project file
3. Environment variable is set correctly: `ASPNETCORE_ENVIRONMENT`

### Secrets not found

**User Secrets**:
```bash
dotnet user-secrets list  # Verify secrets exist
```

**Environment Variables**:
```bash
printenv | grep Crystal  # Linux/Mac
set | findstr Crystal    # Windows
```

### JWT signing key invalid

**Verify key size**:
```bash
echo "your-key" | base64 -d | wc -c  # Should be 64 bytes
```

**Generate new key** (see [Generating Keys](#generating-signing-keys))

### Database connection fails

**SQLite**:
- Check file path is correct
- Ensure directory exists
- Check file permissions

**SQL Server**:
- Verify connection string format
- Test connection with SQL client
- Check firewall rules

## Summary

Crystal's configuration system:

- **Hierarchical**: Multiple sources with clear precedence
- **Environment-aware**: Different settings per environment
- **Secure**: Secrets kept out of source control
- **Flexible**: Easy to customize and extend
- **Type-safe**: Options pattern for compile-time checking

Follow best practices to keep your configuration secure and maintainable!
