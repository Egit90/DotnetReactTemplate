using AspNet.Security.OAuth.Discord;
using AspNet.Security.OAuth.GitHub;
using Crystal.Core.AuthSchemes;
using Crystal.Core.Extensions;
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
