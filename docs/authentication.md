# Authentication System

Crystal includes a complete, production-ready authentication system built on ASP.NET Core Identity with JWT tokens. This guide explains how everything works.

## Table of Contents

- [Overview](#overview)
- [Authentication Methods](#authentication-methods)
- [Complete Flow Diagrams](#complete-flow-diagrams)
- [JWT Token System](#jwt-token-system)
- [Storage and State Management](#storage-and-state-management)
- [API Endpoints](#api-endpoints)
- [Frontend Integration](#frontend-integration)
- [Security Features](#security-features)

## Overview

### Technology Stack

- **Backend**: ASP.NET Core Identity + JWT Bearer Authentication
- **Frontend**: React with TypeScript + CrystalClient library
- **Storage**: SQLite (development) / SQL Server (production)
- **OAuth**: GitHub, Discord (extensible)

### Key Features

✅ Email/Password authentication
✅ Email confirmation
✅ Password reset via email
✅ OAuth login (GitHub, Discord)
✅ Account linking (multiple login methods)
✅ JWT access & refresh tokens
✅ Automatic token refresh
✅ Secure token storage
✅ Role-based authorization (extensible)

## Authentication Methods

### 1. Email/Password Authentication

**Sign Up Flow**:
```
User fills form → API creates account → Email sent → User confirms → Can sign in
```

**Sign In Flow**:
```
User provides credentials → API validates → JWT tokens returned → Stored in browser
```

### 2. OAuth Authentication

**Providers**: GitHub, Discord (can add Google, Microsoft, etc.)

**Flow**:
```
User clicks "Sign in with GitHub" → Redirected to GitHub → Approves →
Redirected back → Account created/linked → JWT tokens issued
```

## Complete Flow Diagrams

### Sign Up with Email/Password

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │ 1. POST /auth/signup
       │    { email, password, userName }
       ▼
┌─────────────────────────────────────────┐
│  SignUpEndpoint                         │
│  ────────────────                       │
│  • Validates input                      │
│  • Creates IdentityUser                 │
│  • Saves to database                    │
└──────┬──────────────────────────────────┘
       │ 2. Trigger email confirmation
       ▼
┌─────────────────────────────────────────┐
│  CrystalEmailSenderManager              │
│  ───────────────────────────            │
│  • Generates confirmation token         │
│  • Creates confirmation link            │
└──────┬──────────────────────────────────┘
       │ 3. Send email
       ▼
┌─────────────────────────────────────────┐
│  FluentEmail (EmailConfirmationSender)  │
│  ─────────────────────────────────────  │
│  • Renders Liquid template              │
│  • Saves to disk (dev) or sends (prod)  │
└──────┬──────────────────────────────────┘
       │ 4. Response
       ▼
┌─────────────────────────────────────────┐
│  Browser                                │
│  ───────                                │
│  • Shows "Check your email" message     │
│  • User clicks confirmation link        │
└──────┬──────────────────────────────────┘
       │ 5. GET /account/email/confirm?code=...&userId=...
       ▼
┌─────────────────────────────────────────┐
│  EmailConfirmEndpoint                   │
│  ────────────────────                   │
│  • Validates token                      │
│  • Marks email as confirmed             │
│  • User can now sign in                 │
└─────────────────────────────────────────┘
```

### Sign In with Email/Password

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │ 1. POST /auth/signin
       │    { email, password }
       ▼
┌─────────────────────────────────────────┐
│  SignInEndpoint                         │
│  ──────────────                         │
│  • Checks if email confirmed            │
│  • Validates password                   │
│  • Uses CrystalSignInManager            │
└──────┬──────────────────────────────────┘
       │ 2. Password valid
       ▼
┌─────────────────────────────────────────┐
│  JwtTokenService                        │
│  ───────────────                        │
│  • Generates access token (5 min)       │
│  • Generates refresh token (48 hours)   │
│  • Signs with secret key                │
└──────┬──────────────────────────────────┘
       │ 3. Return tokens
       ▼
┌─────────────────────────────────────────┐
│  SignInEndpoint                         │
│  ──────────────                         │
│  • Sets access token in cookie          │
│  • Sets refresh token in cookie         │
│  • Returns token response               │
└──────┬──────────────────────────────────┘
       │ 4. Store tokens
       ▼
┌─────────────────────────────────────────┐
│  CrystalClient (Frontend)               │
│  ────────────────────────               │
│  • Stores access token in localStorage  │
│  • Stores user info in localStorage     │
│  • Triggers onSignIn event              │
└──────┬──────────────────────────────────┘
       │ 5. Navigate to app
       ▼
┌─────────────────────────────────────────┐
│  React App                              │
│  ─────────                              │
│  • Updates auth state                   │
│  • Shows authenticated UI               │
│  • Redirects to home page               │
└─────────────────────────────────────────┘
```

### OAuth Sign In (GitHub Example)

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │ 1. Click "Sign in with GitHub"
       │    Calls: externalChallenge({ provider: 'GitHub', mode: 'SignIn' })
       ▼
┌─────────────────────────────────────────┐
│  CrystalClient                          │
│  ─────────────                          │
│  • Builds callback URL                  │
│  • Redirects to:                        │
│    /auth/external/challenge/GitHub      │
└──────┬──────────────────────────────────┘
       │ 2. Backend redirect
       ▼
┌─────────────────────────────────────────┐
│  ExternalChallengeEndpoint              │
│  ─────────────────────────              │
│  • Configures OAuth                     │
│  • Redirects to GitHub                  │
└──────┬──────────────────────────────────┘
       │ 3. User approves on GitHub
       ▼
┌─────────────────────────────────────────┐
│  GitHub OAuth                           │
│  ────────────                           │
│  • User logs in to GitHub               │
│  • Approves app permissions             │
│  • Redirects back with code             │
└──────┬──────────────────────────────────┘
       │ 4. Callback: /auth/signin-github
       ▼
┌─────────────────────────────────────────┐
│  ASP.NET Core OAuth Handler             │
│  ──────────────────────────             │
│  • Exchanges code for token             │
│  • Gets user info from GitHub           │
│  • Stores in auth cookie                │
└──────┬──────────────────────────────────┘
       │ 5. Redirect to frontend callback
       ▼
┌─────────────────────────────────────────┐
│  Frontend: /external-challenge-callback │
│  ──────────────────────────────────────│
│  • Calls signInExternal()               │
└──────┬──────────────────────────────────┘
       │ 6. POST /auth/signin/external
       ▼
┌─────────────────────────────────────────┐
│  SignInExternalEndpoint                 │
│  ──────────────────────                 │
│  • Validates external login             │
│  • Creates or links account             │
│  • Generates JWT tokens                 │
└──────┬──────────────────────────────────┘
       │ 7. Return tokens
       ▼
┌─────────────────────────────────────────┐
│  CrystalClient                          │
│  ─────────────                          │
│  • Stores tokens                        │
│  • Gets user info (/auth/whoami)        │
│  • Triggers onSignIn event              │
└──────┬──────────────────────────────────┘
       │ 8. Redirect to app
       ▼
┌─────────────────────────────────────────┐
│  React App (Authenticated)              │
└─────────────────────────────────────────┘
```

### Automatic Token Refresh

```
┌─────────────┐
│  React App  │
└──────┬──────┘
       │ 1. API request with expired token
       │    Authorization: Bearer <expired-token>
       ▼
┌─────────────────────────────────────────┐
│  Backend API                            │
│  ───────────                            │
│  • JWT middleware validates token       │
│  • Token is expired                     │
│  • Returns 401 with x-token-expired     │
└──────┬──────────────────────────────────┘
       │ 2. Response: 401 + x-token-expired
       ▼
┌─────────────────────────────────────────┐
│  Axios Response Interceptor             │
│  ──────────────────────────             │
│  • Detects 401 + x-token-expired        │
│  • Calls refreshToken()                 │
└──────┬──────────────────────────────────┘
       │ 3. POST /auth/signin/refresh
       │    (with refresh token cookie)
       ▼
┌─────────────────────────────────────────┐
│  SignInRefreshEndpoint                  │
│  ─────────────────────                  │
│  • Validates refresh token              │
│  • Generates new access token           │
│  • Returns new token                    │
└──────┬──────────────────────────────────┘
       │ 4. Store new token
       ▼
┌─────────────────────────────────────────┐
│  CrystalClient                          │
│  ─────────────                          │
│  • Updates access token                 │
│  • Retries original request             │
└──────┬──────────────────────────────────┘
       │ 5. Original request succeeds
       ▼
┌─────────────────────────────────────────┐
│  React App                              │
│  ─────────                              │
│  • User never knows token was refreshed │
│  • Seamless experience                  │
└─────────────────────────────────────────┘
```

## JWT Token System

### Token Types

Crystal uses **two token types**:

#### 1. Access Token
- **Purpose**: Authorize API requests
- **Lifetime**: 5 minutes (configurable)
- **Storage**: localStorage
- **Usage**: Sent in `Authorization: Bearer <token>` header

#### 2. Refresh Token
- **Purpose**: Get new access tokens
- **Lifetime**: 48 hours (configurable)
- **Storage**: HTTP-only cookie
- **Usage**: Automatically sent with refresh requests

### Token Structure

**Access Token Payload**:
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "jti": "unique-token-id",
  "exp": 1234567890,
  "iss": "Crystal.WebApi",
  "aud": "Crystal.Client"
}
```

**Refresh Token Payload**:
```json
{
  "sub": "user-id",
  "tokenId": "refresh-token-id",
  "exp": 1234567890,
  "iss": "Crystal.WebApi",
  "aud": "Crystal.Client"
}
```

### Configuration

In `appsettings.json`:
```json
{
  "Crystal": {
    "JwtBearer": {
      "SigningKey": "base64-encoded-512-bit-key",
      "Issuer": "Crystal.WebApi",
      "Audience": "Crystal.Client",
      "AccessTokenExpiresInMinutes": 5,
      "RefreshTokenExpiresInHours": 48
    }
  }
}
```

### Token Generation

Located in `Crystal.Core/Services/JwtTokenService.cs`:

```csharp
public async Task<TokenResponse> GenerateTokenAsync(TUser user)
{
    var accessToken = CreateAccessToken(user);
    var refreshToken = await CreateRefreshTokenAsync(user);

    return new TokenResponse
    {
        access_token = accessToken,
        refresh_token = refreshToken,
        expires_in = _options.AccessTokenExpiresInMinutes * 60
    };
}
```

## Storage and State Management

### Backend Storage

**Database Tables**:
- `AspNetUsers` - User accounts
- `AspNetUserLogins` - OAuth provider links
- `AspNetUserTokens` - Password reset tokens, email confirmation tokens
- `RefreshTokens` - Active refresh tokens

### Frontend Storage

**localStorage** (via CrystalStorage):
```javascript
{
  "crystal_auth_token": "eyJhbGc...",  // Access token
  "crystal_auth_user": "{...}"          // User info JSON
}
```

**Why localStorage?**
- Persists across browser sessions
- Accessible to JavaScript (needed for API calls)
- Can be cleared easily on sign out

**Security Note**: Access tokens are short-lived (5 min) to minimize risk.

### CrystalStorage Interface

```typescript
export interface CrystalStorage {
    getToken(): string | null;
    setToken(token: string): void;
    clearToken(): void;

    getUser(): AuthUser | null;
    setUser(user: AuthUser): void;
    clearUser(): void;
}
```

Abstraction allows swapping storage backends (localStorage, sessionStorage, memory, etc.).

## API Endpoints

### Authentication Endpoints (`/auth`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/auth/signup` | Register new account |
| POST | `/auth/signin` | Login with email/password |
| POST | `/auth/signout` | Logout (invalidate refresh token) |
| GET | `/auth/external/challenge/{provider}` | Start OAuth flow |
| POST | `/auth/signin/external` | Complete OAuth sign-in |
| POST | `/auth/signup/external` | Create account from OAuth |
| POST | `/auth/signin/refresh` | Refresh access token |
| GET | `/auth/whoami` | Get current user info |

### Account Endpoints (`/account`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/account/info` | Get account details |
| GET | `/account/email/confirm` | Confirm email address |
| POST | `/account/email/confirm/resend` | Resend confirmation email |
| POST | `/account/password/forgot` | Request password reset |
| POST | `/account/password/reset` | Reset password with token |
| POST | `/account/password/change` | Change password (authenticated) |
| POST | `/account/link/external` | Link OAuth account |

## Frontend Integration

### CrystalClient Library

Located in `src/CrystalClient/src/crystal-client.ts`:

```typescript
const client = new CrystalClient({
    apiBaseUrl: 'https://localhost:7050',
    axiosInstance: axios.create()
});

// Initialize interceptors for automatic token handling
client.initAxiosInterceptors(() => {
    // Called on sign out
    navigate('/signin');
});

// Sign up
await client.signUp({
    email: 'user@example.com',
    password: 'Password123!',
    userName: 'user'
});

// Sign in
const user = await client.signIn({
    email: 'user@example.com',
    password: 'Password123!'
});

// OAuth sign in
client.externalChallenge({
    provider: 'GitHub',
    mode: 'SignIn'
});

// Get current user
const userInfo = await client.whoAmI();

// Sign out
await client.signOut();
```

### React Integration

**AuthProvider** (`src/Client.React/src/providers/AuthProvider.tsx`):

```typescript
const { authClient, user, isAuthenticated } = useAuth();

// In components:
if (isAuthenticated) {
    return <Dashboard />;
} else {
    return <SignIn />;
}
```

### Protected Routes

```typescript
<Route
    path="/dashboard"
    element={
        <ProtectedRoute>
            <Dashboard />
        </ProtectedRoute>
    }
/>
```

## Security Features

### Password Requirements

Configured in `ServicesExtensions.cs`:

```csharp
services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
});
```

### Email Confirmation

**Enforced** by default:

```csharp
options.SignIn.RequireConfirmedEmail = true;
```

Users cannot sign in until email is confirmed.

### Token Security

- **Signing Algorithm**: HMAC-SHA256
- **Key Size**: 512 bits (64 bytes)
- **Token Expiry**: Short-lived access tokens
- **Refresh Token Rotation**: One-time use refresh tokens

### HTTPS Only

Cookies are marked as:
```csharp
o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
o.Cookie.HttpOnly = true;
o.Cookie.SameSite = SameSiteMode.None;
```

### SQL Injection Protection

- Entity Framework Core uses parameterized queries
- No raw SQL in authentication code

### CSRF Protection

- Refresh tokens in HTTP-only cookies
- CORS configured appropriately

## Common Flows

### Forgot Password

```
1. User clicks "Forgot Password"
2. Frontend calls: POST /account/password/forgot { email }
3. Backend sends reset email (check /emails folder in dev)
4. User clicks reset link
5. Frontend shows reset form
6. User submits new password
7. Frontend calls: POST /account/password/reset { code, userId, newPassword }
8. User can now sign in with new password
```

### Link OAuth Account

```
1. User signs in with email/password
2. Goes to My Account page
3. Clicks "Link GitHub Account"
4. Redirected to GitHub
5. Approves
6. Returns to app
7. GitHub account is now linked
8. User can sign in with either method
```

### Change Password

```
1. User goes to My Account
2. Fills change password form
3. Frontend calls: POST /account/password/change {
     currentPassword,
     newPassword
   }
4. Password updated
5. User continues using app (no re-login needed)
```

## Configuration

### Enable/Disable Features

In `appsettings.json`:

```json
{
  "Crystal": {
    "EnableEmailPasswordFlow": true,
    "EnableSignUp": true,
    "EnableExternalProvidersFlow": true
  }
}
```

### Configure OAuth Providers

```json
{
  "Crystal": {
    "Providers": {
      "GitHub": {
        "ClientId": "your-client-id",
        "ClientSecret": "your-client-secret",
        "Scopes": ["user:email", "read:user"]
      },
      "Discord": {
        "ClientId": "your-client-id",
        "ClientSecret": "your-client-secret",
        "Scopes": ["email"]
      }
    }
  }
}
```

## Troubleshooting

### "Email not confirmed" error

Check:
1. Email confirmation is sent (check `/emails` folder in dev)
2. Confirmation link was clicked
3. `RequireConfirmedEmail` is set correctly

### Tokens expire too quickly

Adjust in `appsettings.json`:
```json
{
  "AccessTokenExpiresInMinutes": 15,
  "RefreshTokenExpiresInHours": 168
}
```

### OAuth redirect errors

Ensure callback URLs match in:
1. OAuth provider settings
2. Application configuration

### CORS errors

Check `ServicesExtensions.cs`:
```csharp
.AddDefaultCorsPolicy()
```

Allows frontend origin in development.

## Summary

Crystal's authentication system provides:

- 🔐 Secure JWT-based authentication
- 📧 Email confirmation workflow
- 🔄 Automatic token refresh
- 🔗 OAuth provider integration
- 👤 Account linking
- 🛡️ Industry-standard security practices

All managed with minimal configuration required!
