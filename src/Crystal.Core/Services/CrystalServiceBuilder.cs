using Crystal.Core.AuthSchemes;
using Crystal.Core.Endpoints;
using Crystal.Core.Endpoints.SignUp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crystal.Core.Services;

/// <summary>
/// Builder for Crystal services. <br/>
/// Can be used to configure the services and endpoints of Crystal.
/// </summary>
/// <typeparam name="TUser"></typeparam>
public class CrystalServiceBuilder<TUser, TKey>
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>
        , ICrystalUser<TKey>, new()
{
    public CrystalOptions CrystalOptions { get; set; }
    public IdentityBuilder IdentityBuilder { get; }
    public AuthenticationBuilder AuthenticationBuilder { get; }
    public IServiceCollection Services { get; private set; }
    public IConfiguration Configuration { get; private set; }

    internal CrystalServiceBuilder(
        IServiceCollection services,
        CrystalOptions crystalOptions,
        IdentityBuilder identityBuilder,
        AuthenticationBuilder authenticationBuilder,
        IConfiguration configuration)
    {
        Services = services;
        CrystalOptions = crystalOptions;
        IdentityBuilder = identityBuilder;
        Configuration = configuration;
        AuthenticationBuilder = authenticationBuilder;
    }

    public CrystalServiceBuilder<TUser, TKey> ConfigureIdentity(Action<IdentityBuilder> configure)
    {
        configure(IdentityBuilder);
        return this;
    }

    public CrystalServiceBuilder<TUser, TKey> ConfigureAuthentication(Action<AuthenticationBuilder, CrystalOptions> configure)
    {
        configure(AuthenticationBuilder, CrystalOptions);
        return this;
    }

    /// <summary>
    /// Adds default CORS policy for the client app.
    /// Uses the base url of the client app from the configuration: Crystal:ClientApp:BaseUrl
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public CrystalServiceBuilder<TUser, TKey> AddDefaultCorsPolicy()
    {
        var clientAppUrl = CrystalOptions.ClientApp.BaseUrl;
        if (string.IsNullOrWhiteSpace(clientAppUrl))
        {
            return this;
        }

        Services.AddCors(opts => opts.AddDefaultPolicy(
            policy =>
            {
                policy.WithOrigins(clientAppUrl)
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("X-Token-Expired");
            }));

        return this;
    }

    /// <summary>
    /// Registers the custom SignUpRequest model for the SignUpEndpoint.
    /// </summary>
    /// <typeparam name="TSignUpRequest"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public CrystalServiceBuilder<TUser, TKey> UseSignUpModel<TSignUpRequest>() where TSignUpRequest : SignUpRequest
    {
        var descriptor = Services.FirstOrDefault(
            d => d.ServiceType == typeof(IAuthEndpoint) &&
                 d.ImplementationType == typeof(SignUpEndpoint<TUser, TKey, SignUpRequest>));
        if (descriptor is null)
        {
            throw new(
                "Error while registering SignUpEndpoint with custom model. Default SignUpEndpoint is not registered");
        }

        Services.Remove(descriptor);
        Services.AddSingleton<IAuthEndpoint, SignUpEndpoint<TUser, TKey, TSignUpRequest>>();
        return this;
    }

    /// <summary>
    /// Registers the custom SignUpExternalRequest model for the SignUpExternalEndpoint.
    /// </summary>
    /// <typeparam name="TSignUpExternalRequest"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public CrystalServiceBuilder<TUser, TKey> UseExternalSignUpModel<TSignUpExternalRequest>()
        where TSignUpExternalRequest : class
    {
        if (CrystalOptions.EnableSignUp is false)
        {
            throw new("EnableSignUp is set to false in CrystalOptions. Can't register SignUpExternalEndpoint");
        }

        Services.AddSingleton<IAuthEndpoint, SignUpExternalEndpoint<TUser, TKey, TSignUpExternalRequest>>();
        CrystalOptions.Internal.CustomExternalSignUpFlow = true;
        return this;
    }

    public CrystalServiceBuilder<TUser, TKey> AddProvider(string provider, Action<AuthenticationBuilder, CrystalOptions> authBuilder)
    {
        AuthenticationBuilder.AddProviderIfConfigured(provider, CrystalOptions, b =>
        {
            authBuilder?.Invoke(b, CrystalOptions);
        });

        return this;
    }
}