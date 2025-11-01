# Email Service Documentation

## Overview

The Crystal email service provides a flexible, abstraction-based email system for sending transactional emails like email confirmations and password resets. It uses [FluentEmail](https://github.com/lukencode/FluentEmail) with Liquid templates and supports multiple sending strategies for development and production environments.

## Architecture

### Core Components

1. **Email Sender Interfaces** (`Crystal.Core/Services/EmailSender/`)
   - `ICrystalEmailConfirmationEmailSender<TUser>` - Contract for email confirmation emails
   - `ICrystalPasswordResetEmailSender<TUser>` - Contract for password reset emails

2. **Email Sender Manager** (`Crystal.Core/Services/EmailSender/CrystalEmailSenderManager.cs`)
   - Orchestrates email sending
   - Handles validation (email already confirmed, email confirmation required, etc.)
   - Logs critical errors if email senders aren't configured
   - Implements `ICrystalEmailSenderManager<TUser>`

3. **FluentEmail Implementation** (`Crystal.FluentEmail/`)
   - `EmailConfirmationEmailSender<TUser>` - Sends email confirmation using FluentEmail
   - `PasswordResetEmailSender<TUser>` - Sends password reset using FluentEmail
   - `CrystalFluentEmailFactory` - Creates FluentEmail instances with Liquid templating
   - Built-in HTML templates with Liquid syntax

### Email Types

```csharp
public enum EmailType
{
    Confirmation = 0,
    PasswordReset = 1,
}
```

## How It Works

### 1. Registration Flow

In `Program.cs`:
```csharp
builder.Services.SetupCrystal(builder.Configuration);
```

In `WebApi/ServicesExtensions.cs`:
```csharp
services.AddCrystal<MyUser>(configuration)
    .AddFluentEmail();  // Registers email services
```

This registers:
- `ICrystalEmailSenderManager<TUser>` as **Scoped**
- `ICrystalEmailConfirmationEmailSender<TUser>` as **Transient** (FluentEmail implementation)
- `ICrystalPasswordResetEmailSender<TUser>` as **Transient** (FluentEmail implementation)
- `ISender` - Either `SaveToDiskSender` (dev) or `MailKitSender` (prod)

### 2. Dependency Injection

The manager uses **constructor injection** (not service locator):

```csharp
public class CrystalEmailSenderManager<TUser>(
    ICrystalEmailConfirmationEmailSender<TUser>? _confirmationSender,
    ICrystalPasswordResetEmailSender<TUser>? _passwordResetSender,
    IOptions<IdentityOptions> _identityOptions,
    ILogger<CrystalEmailSenderManager<TUser>> logger)
```

Email senders are **nullable** - if not registered, the manager logs critical errors instead of throwing exceptions.

### 3. Sending Flow

```
User Action (Sign Up)
    ↓
Endpoint → SignUpEndpoint
    ↓
CrystalEmailSenderManager.SendEmailConfirmationAsync()
    ↓
Checks: RequireConfirmedEmail? EmailConfirmed? Sender configured?
    ↓
EmailConfirmationEmailSender.SendAsync()
    ↓
CrystalFluentEmailFactory.CreateEmail()
    ↓
ISender (SaveToDiskSender or MailKitSender)
    ↓
Email saved to disk (dev) or sent via SMTP (prod)
```

## Configuration

### Development Setup (Save to Disk)

Perfect for local development - saves emails as HTML files instead of sending them.

**appsettings.json:**
```json
{
  "Crystal": {
    "ClientApp": {
      "BaseUrl": "http://localhost:5173",
      "EmailConfirmationPath": "/confirm-email"
    }
  },
  "FluentEmail": {
    "SaveEmailsOnDisk": "emails",
    "FromEmail": "noreply@crystal.com",
    "FromName": "Crystal App"
  }
}
```

**How it works:**
1. When `SaveEmailsOnDisk` is set, `SaveToDiskSender` is registered
2. Emails are saved to the specified folder (e.g., `emails/`)
3. Open the HTML files in a browser to preview emails
4. Perfect for testing email templates without SMTP setup

**Location:** `src/Crystal.WebApi/emails/`

### Production Setup (SMTP with MailKit)

For production environments, emails are sent via SMTP.

**appsettings.Production.json:**
```json
{
  "Crystal": {
    "ClientApp": {
      "BaseUrl": "https://yourdomain.com",
      "EmailConfirmationPath": "/confirm-email"
    }
  },
  "FluentEmail": {
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "Your App Name",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password"
  }
}
```

**SMTP Providers:**

#### Gmail
```json
{
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-email@gmail.com",
  "SmtpPassword": "your-app-password"
}
```
Note: Use [App Passwords](https://support.google.com/accounts/answer/185833) for Gmail, not your regular password.

#### SendGrid
```json
{
  "SmtpHost": "smtp.sendgrid.net",
  "SmtpPort": 587,
  "SmtpUsername": "apikey",
  "SmtpPassword": "your-sendgrid-api-key"
}
```

#### AWS SES
```json
{
  "SmtpHost": "email-smtp.us-east-1.amazonaws.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-smtp-username",
  "SmtpPassword": "your-smtp-password"
}
```

#### Mailgun
```json
{
  "SmtpHost": "smtp.mailgun.org",
  "SmtpPort": 587,
  "SmtpUsername": "postmaster@your-domain.mailgun.org",
  "SmtpPassword": "your-mailgun-password"
}
```

## Email Templates

### Built-in Templates

Crystal includes embedded Liquid templates:
- `Crystal.FluentEmail/Templates/EmailConfirmation.template.html`
- `Crystal.FluentEmail/Templates/PasswordReset.template.html`

**Template Variables:**

**Email Confirmation:**
```liquid
{{ ConfirmationLink }}
{{ Email }}
{{ UserName }}
```

**Password Reset:**
```liquid
{{ ResetLink }}
{{ Email }}
{{ UserName }}
```

### Custom Templates

You can override templates with custom ones:

**appsettings.json:**
```json
{
  "FluentEmail": {
    "FromEmail": "noreply@crystal.com",
    "FromName": "Crystal App",
    "Emails": {
      "Confirmation": {
        "Subject": "Welcome! Please confirm your email",
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

**Custom Template Example (`Templates/CustomEmailConfirmation.html`):**
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>Email Confirmation</title>
    <style>
        body { font-family: Arial, sans-serif; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .button {
            background-color: #4F46E5;
            color: white;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 6px;
            display: inline-block;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>Welcome to Crystal!</h1>
        <p>Hi {{ UserName }},</p>
        <p>Thanks for signing up! Please confirm your email address to get started.</p>
        <p>
            <a href="{{ ConfirmationLink }}" class="button">Confirm Email</a>
        </p>
        <p>Or copy this link: {{ ConfirmationLink }}</p>
        <p>If you didn't create an account, you can safely ignore this email.</p>
    </div>
</body>
</html>
```

### Extending Email Senders

You can create custom email senders by inheriting from the base classes:

```csharp
public class CustomEmailConfirmationSender<TUser> : EmailConfirmationEmailSender<TUser>
    where TUser : IdentityUser, ICrystalUser
{
    public CustomEmailConfirmationSender(
        IOptions<FluentEmailOptions> fluentEmailOptions,
        CrystalFluentEmailFactory emailFactory,
        ILogger<CustomEmailConfirmationSender<TUser>> logger)
        : base(fluentEmailOptions, emailFactory, logger)
    {
    }

    // Override to add custom model properties
    protected override object PrepareEmailConfirmationModel(IdentityUser user, string confirmationLink)
    {
        return new
        {
            ConfirmationLink = confirmationLink,
            Email = user.Email,
            UserName = user.UserName,
            // Add custom properties
            CompanyName = "Crystal Inc.",
            Year = DateTime.Now.Year,
            SupportEmail = "support@crystal.com"
        };
    }

    // Or completely override the send logic
    public override async Task SendAsync(TUser user, string confirmationLink)
    {
        // Custom logic here
        await base.SendAsync(user, confirmationLink);
    }
}
```

Register in `WebApi/ServicesExtensions.cs`:
```csharp
services.AddCrystal<MyUser>(configuration)
    .AddFluentEmail();

// Override with custom implementation
services.AddTransient<ICrystalEmailConfirmationEmailSender<MyUser>, CustomEmailConfirmationSender<MyUser>>();
```

## Advanced Scenarios

### Disabling MailKit Sender

If you want to use a different sender (e.g., SendGrid SDK):

```csharp
services.AddCrystal<MyUser>(configuration)
    .AddFluentEmail(registerMailKitSender: false);

// Register custom sender
services.AddScoped<ISender, CustomSendGridSender>();
```

### Testing Without Email

For integration tests, you can register null implementations:

```csharp
services.AddScoped<ICrystalEmailConfirmationEmailSender<MyUser>>(_ => null);
services.AddScoped<ICrystalPasswordResetEmailSender<MyUser>>(_ => null);
```

The manager will gracefully handle null senders and log warnings.

### Environment-Specific Configuration

Use environment variables to override settings:

```bash
export FluentEmail__SmtpHost="smtp.production.com"
export FluentEmail__SmtpPassword="production-password"
```

Or use User Secrets for development:
```bash
dotnet user-secrets set "FluentEmail:SmtpPassword" "dev-password"
```

## Troubleshooting

### Emails Not Sending

1. **Check logs** - Look for critical errors:
   ```
   Email client is not configured. Cannot send email confirmation for user@example.com
   ```

2. **Verify ISender registration** - Ensure either `SaveEmailsOnDisk` is set or SMTP is configured

3. **Check SMTP credentials** - Test with a tool like `telnet` or `openssl`:
   ```bash
   openssl s_client -connect smtp.gmail.com:587 -starttls smtp
   ```

4. **Firewall issues** - Ensure outbound port 587 (or 465) is open

### Email Confirmation Not Required

Check `IdentityOptions` in `ServicesExtensions.cs`:
```csharp
services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;  // Must be true
});
```

### Template Not Found

Ensure template path is relative to the WebApi project directory:
```json
{
  "Emails": {
    "Confirmation": {
      "TemplatePath": "Templates/EmailConfirmation.html"  // Relative to WebApi root
    }
  }
}
```

### Liquid Template Errors

Check template syntax - common issues:
- Mismatched variables: `{{ ConfirmationLink }}` (case-sensitive)
- Invalid Liquid syntax: Use `{% if %}...{% endif %}` not `@if`

## Security Considerations

1. **Never commit SMTP passwords** - Use environment variables, user secrets, or Azure Key Vault

2. **Use App Passwords** - For Gmail, don't use your main password

3. **TLS/SSL** - MailKit automatically uses STARTTLS on port 587

4. **Rate Limiting** - Consider rate limiting email confirmations to prevent abuse

5. **Token Expiration** - Default ASP.NET Core tokens expire after a configured time (check `DataProtectionTokenProviderOptions`)

## Summary

| Environment | Configuration | Sender | Use Case |
|-------------|---------------|--------|----------|
| **Development** | `SaveEmailsOnDisk: "emails"` | `SaveToDiskSender` | Local testing, template preview |
| **Production** | SMTP settings | `MailKitSender` | Real email sending |
| **Testing** | Null senders | None | Integration tests |

The email service is designed to be:
- **Flexible** - Easy to swap implementations
- **Testable** - Nullable dependencies, no hard coupling
- **Observable** - Extensive logging at all levels
- **Extensible** - Override senders, templates, and models
