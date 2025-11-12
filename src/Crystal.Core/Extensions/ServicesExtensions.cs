using Crystal.Core.AuthSchemes;
using Crystal.Core.Endpoints;
using Crystal.Core.Endpoints.Account;
using Crystal.Core.Endpoints.Email;
using Crystal.Core.Endpoints.External;
using Crystal.Core.Endpoints.Password;
using Crystal.Core.Endpoints.SignIn;
using Crystal.Core.Endpoints.SignOut;
using Crystal.Core.Endpoints.SignUp;
using Crystal.Core.Endpoints.Token;
using Crystal.Core.Services.EmailSender;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crystal.Core.Extensions;

/// <summary>
///
/// </summary>
public static class ServicesExtensions
{
    /// <summary>
    /// Registers Crystal services and endpoints
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configureOptions"></param>
    /// <typeparam name="TUser"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static CrystalServiceBuilder<TUser, TKey> AddCrystal<TUser, TKey>(
        this IServiceCollection services, IConfiguration configuration, Action<CrystalOptions>? configureOptions = null)
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>
        , ICrystalUser<TKey>, new()
    {
        var opts = configuration.GetSection(CrystalOptions.SectionPath).Get<CrystalOptions>()
                   ?? throw new("CrystalOptions is not configured in appsettings.json");
        configureOptions?.Invoke(opts);

        services.AddOptions<CrystalOptions>().BindConfiguration(CrystalOptions.SectionPath).Configure(o => configureOptions?.Invoke(o));
        services.AddOptions<CrystalJwtBearerOptions>().BindConfiguration(CrystalJwtBearerOptions.SectionPath).Configure(o =>
        {
            o.Issuer = opts.JwtBearer.Issuer;
            o.Audience = opts.JwtBearer.Audience;
            o.SigningKey = opts.JwtBearer.SigningKey;
        });

        services.AddScoped<IRefreshTokenManager, RefreshTokenManager>()
                .AddScoped<IJwtTokenService, JwtTokenService>()
                .AddScoped<ICrystalEmailSenderManager<TUser, TKey>, CrystalEmailSenderManager<TUser, TKey>>();

        if (opts.EnableEmailPasswordFlow)
        {
            services.AddSingleton<IAuthEndpoint, SignInEndpoint<TUser, TKey>>()
                    .AddSingleton<IAuthEndpoint, TokenRefreshEndpoint<TUser, TKey>>()
                    .AddSingleton<IAuthEndpoint, TokenEndpoint<TUser, TKey>>()
                    .AddSingleton<IAccountEndpoint, PasswordForgotEndpoint<TUser, TKey>>()
                    .AddSingleton<IAccountEndpoint, PasswordResetEndpoint<TUser, TKey>>()
                    .AddSingleton<IAccountEndpoint, PasswordChangeEndpoint<TUser, TKey>>()
                    .AddSingleton<IAccountEndpoint, EmailConfirmationResendEndpoint<TUser, TKey>>()
                    .AddSingleton<IAccountEndpoint, EmailConfirmEndpoint<TUser, TKey>>();

            if (opts.EnableSignUp)
            {
                services.AddSingleton<IAuthEndpoint, SignUpEndpoint<TUser, TKey, SignUpRequest>>();
            }
        }

        if (opts.EnableExternalProvidersFlow)
        {
            services.AddSingleton<IAuthEndpoint, ExternalChallengeEndpoint<TUser, TKey>>()
                    .AddSingleton<IAuthEndpoint, ExternalProvidersEndpoint<TUser, TKey>>()
                    .AddSingleton<IAuthEndpoint, SignInExternalEndpoint<TUser, TKey>>()
                    .AddSingleton<IAccountEndpoint, LinkExternalLoginEndpoint<TUser, TKey>>();
        }

        services.AddSingleton<IAuthEndpoint, SignInRefreshEndpoint<TUser, TKey>>()
                .AddSingleton<IAuthEndpoint, SignOutEndpoint<TUser, TKey>>()
                .AddSingleton<IAuthEndpoint, WhoAmIEndpoint<TUser, TKey>>()
                .AddSingleton<IAccountEndpoint, AccountInfoEndpoint<TUser, TKey>>();

        var identityBuilder = services
            .AddIdentityCore<TUser>()
            .AddSignInManager<CrystalSignInManager<TUser, TKey>>()
            .AddUserManager<CrystalUserManager<TUser, TKey>>()
            .AddRoles<IdentityRole<TKey>>()
            .AddDefaultTokenProviders();

        services.AddScoped<ICrystalUserManager>(sp => sp.GetRequiredService<CrystalUserManager<TUser, TKey>>());

        services.Configure<IdentityOptions>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = true;
        });

        var authenticationBuilder = services
            .AddAuthorization()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                o =>
                {
                    o.Events ??= new JwtBearerEvents();
                    o.Events.OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(CrystalAuthSchemeDefaults.AccessTokenCookieName,
                                out var token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    };
                    o.ConfigureBearerAuth(opts.JwtBearer);
                })
            .AddScheme<CrystalJwtBearerOptions, CrystalSignInJwtBearerHandler>(CrystalAuthSchemeDefaults.BearerSignInScheme, _ => { })
            .AddScheme<CrystalJwtBearerOptions, CrystalTokenJwtBearerHandler>(CrystalAuthSchemeDefaults.BearerTokenScheme, _ => { })
            .AddJwtBearer(CrystalAuthSchemeDefaults.RefreshTokenScheme, o =>
            {
                o.Events ??= new JwtBearerEvents();
                o.Events.OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue(CrystalAuthSchemeDefaults.RefreshTokenCookieName,
                            out var token))
                    {
                        context.Token = token;
                    }

                    return Task.CompletedTask;
                };
                o.ConfigureBearerAuth(opts.JwtBearer);
            })
            .AddScheme<PolicySchemeOptions, CrystalPolicySignInExternalHandler>(CrystalAuthSchemeDefaults.SignInExternalPolicyScheme, _ => { })
            .AddCookie(CrystalAuthSchemeDefaults.SignInExternalScheme, o =>
            {
                o.Cookie.SameSite = SameSiteMode.None;
                o.Cookie.HttpOnly = true;
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.ExpireTimeSpan = TimeSpan.FromSeconds(60);
                o.Cookie.Name = CrystalAuthSchemeDefaults.SignInExternalScheme;
            })
            .AddCookie(CrystalAuthSchemeDefaults.SignUpExternalScheme, o =>
            {
                o.Cookie.SameSite = SameSiteMode.None;
                o.Cookie.HttpOnly = true;
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.ExpireTimeSpan = TimeSpan.FromSeconds(360);
                o.Cookie.Path = opts.AuthApiBasePath + "/signup/external";
                o.Cookie.Name = CrystalAuthSchemeDefaults.SignUpExternalScheme;
            });

        return new CrystalServiceBuilder<TUser, TKey>(services, opts, identityBuilder, authenticationBuilder, configuration);
    }
}