using Microsoft.AspNetCore.Authentication;

namespace Crystal.Core.Options;

/// <summary>
/// Options for Crystal
/// </summary>
public class CrystalOptions
{
    public const string SectionPath = "Crystal";

    public string AuthApiBasePath { get; set; } = "/api/auth";
    public string AccountApiBasePath { get; set; } = "/api/account";
    public string[] DefaultRoles { get; set; } = [];
    public bool AutoAccountLinking { get; set; } = true;

    public bool EnableEmailPasswordFlow { get; set; } = true;
    public bool EnableExternalProvidersFlow { get; set; } = true;
    public bool EnableSignUp { get; set; } = true;

    public ClientAppOptions ClientApp { get; set; } = new();
    public CrystalJwtBearerOptions JwtBearer { get; set; } = new();
    public CrystalProviders Providers { get; set; } = new();

    internal class Internal
    {
        public static bool CustomExternalSignUpFlow { get; set; }
    }
}

public class ClientAppOptions
{
    public string? BaseUrl { get; set; }
    public string EmailConfirmationPath { get; set; } = "/confirm-email";
    public string PasswordResetPath { get; set; } = "/reset-password";
}

public class CrystalJwtBearerOptions : AuthenticationSchemeOptions
{
    public const string SectionPath = "Crystal:JwtBearer";

    public string? SigningKey { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public int AccessTokenExpiresInMinutes { get; set; } = 30;
    public int RefreshTokenExpireInHours { get; set; } = 72;
}

public class CrystalProviders : Dictionary<string, CrystalProviderInfo>
{
}

public class CrystalProviderInfo
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string[]? Scopes { get; set; }
}