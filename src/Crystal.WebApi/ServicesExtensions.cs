using AspNet.Security.OAuth.Discord;
using AspNet.Security.OAuth.GitHub;
using Crystal.Core.AuthSchemes;
using Crystal.Core.Endpoints.SignUp;
using Crystal.Core.Extensions;
using Crystal.Core.Services;
using Crystal.EntityFrameworkCore;
using Crystal.FluentEmail;
using WebApi.Data;

namespace WebApi;

public static class ServicesExtensions
{
    public static IServiceCollection SetupCrystal(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCrystal<MyUser>(configuration)
            .AddProvider(GitHubAuthenticationDefaults.AuthenticationScheme, (auth, options) =>
            {
                auth.AddGitHub(o => o.Configure(GitHubAuthenticationDefaults.AuthenticationScheme, options));
            })
            .AddProvider(DiscordAuthenticationDefaults.AuthenticationScheme, (auth, options) =>
            {
                auth.AddDiscord(o => o.Configure(DiscordAuthenticationDefaults.AuthenticationScheme, options));
            })
            .AddDefaultCorsPolicy()
            .AddEntityFrameworkStore<ApplicationDbContext, MyUser>()
            .AddFluentEmail();
        // Uncomment the line below to use custom signup models
        // .UseCrystalCustomSignup();

        return services;
    }
}


public static class CrystalBuilderExtensions
{
    /// <summary>
    /// Use this method to add custom signup models
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static CrystalServiceBuilder<MyUser> UseCrystalCustomSignup(this CrystalServiceBuilder<MyUser> builder)
    {
        builder
            .UseSignUpModel<MySignUpRequest>()
            .UseExternalSignUpModel<MySignUpExternalRequest>();


        builder.Services.AddScoped<ISignUpExternalEndpointEvents<MyUser, MySignUpExternalRequest>, SignUpExternalExtension>();
        builder.Services.AddScoped<ISignUpEndpointEvents<MyUser, MySignUpRequest>, SignUpExtension>();

        return builder;
    }
}