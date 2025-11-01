using Crystal.Core.Abstractions;
using Crystal.Core.AuthSchemes;
using Crystal.Core.Endpoints;
using Crystal.Core.Options;
using Crystal.Core.Services;
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
    public static CrystalServiceBuilder<TUser> AddCrystal<TUser>(
        this IServiceCollection services, IConfiguration configuration, Action<CrystalOptions>? configureOptions = null)
        where TUser : IdentityUser, ICrystalUser, new()
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
                .AddScoped<ICrystalEmailSenderManager<TUser>, CrystalEmailSenderManager<TUser>>();

        if (opts.EnableEmailPasswordFlow)
        {
            services.AddSingleton<IAuthEndpoint, SignInEndpoint<TUser>>()
                    .AddSingleton<IAuthEndpoint, TokenRefreshEndpoint<TUser>>()
                    .AddSingleton<IAuthEndpoint, TokenEndpoint<TUser>>()
                    .AddSingleton<IAccountEndpoint, PasswordForgotEndpoint<TUser>>()
                    .AddSingleton<IAccountEndpoint, PasswordResetEndpoint<TUser>>()
                    .AddSingleton<IAccountEndpoint, PasswordChangeEndpoint<TUser>>()
                    .AddSingleton<IAccountEndpoint, EmailConfirmationResendEndpoint<TUser>>()
                    .AddSingleton<IAccountEndpoint, EmailConfirmEndpoint<TUser>>();

            if (opts.EnableSignUp)
            {
                services.AddSingleton<IAuthEndpoint, SignUpEndpoint<TUser, SignUpRequest>>();
            }
        }

        if (opts.EnableExternalProvidersFlow)
        {
            services.AddSingleton<IAuthEndpoint, ExternalChallengeEndpoint<TUser>>()
                    .AddSingleton<IAuthEndpoint, ExternalProvidersEndpoint<TUser>>()
                    .AddSingleton<IAuthEndpoint, SignInExternalEndpoint<TUser>>()
                    .AddSingleton<IAccountEndpoint, LinkExternalLoginEndpoint<TUser>>();
        }

        services.AddSingleton<IAuthEndpoint, SignInRefreshEndpoint<TUser>>()
                .AddSingleton<IAuthEndpoint, SignOutEndpoint<TUser>>()
                .AddSingleton<IAuthEndpoint, WhoAmIEndpoint<TUser>>()
                .AddSingleton<IAccountEndpoint, AccountInfoEndpoint<TUser>>();

        var identityBuilder = services
            .AddIdentityCore<TUser>()
            .AddSignInManager<CrystalSignInManager<TUser>>()
            .AddUserManager<CrystalUserManager<TUser>>()
            .AddRoles<IdentityRole>()
            .AddDefaultTokenProviders();

        services.AddScoped<ICrystalUserManager>(sp => sp.GetRequiredService<CrystalUserManager<TUser>>());

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
            .AddScheme<CrystalJwtBearerOptions, CrystalSignInJwtBearerHandler>(
                CrystalAuthSchemeDefaults.BearerSignInScheme, _ => { })
            .AddScheme<CrystalJwtBearerOptions, CrystalTokenJwtBearerHandler>(
                CrystalAuthSchemeDefaults.BearerTokenScheme, _ => { })
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
            .AddScheme<PolicySchemeOptions, CrystalPolicySignInExternalHandler>(
                CrystalAuthSchemeDefaults.SignInExternalPolicyScheme, _ => { })
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

        return new CrystalServiceBuilder<TUser>(services, opts, identityBuilder, authenticationBuilder, configuration);
    }
}