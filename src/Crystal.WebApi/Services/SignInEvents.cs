using Crystal.Core.Endpoints.SignIn;
using Microsoft.AspNetCore.Identity;
using WebApi.Data;
namespace WebApi.Services;

public class SignInEvents(
    UserManager<MyUser> userManager,
    ILogger<SignInEvents> logger) : ISignInEndpointEvents<MyUser>
{
    public Task SignInFailedAsync(SignInRequest request, HttpContext context, SignInResult result)
    {
        return Task.CompletedTask;
    }

    public async Task SignInSucceededAsync(MyUser user, HttpContext context)
    {
        // Update last login date
        user.LastLoginDate = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        logger.LogInformation("User {Email} last login date updated", user.Email);
    }

    public Task UserNotFound(SignInRequest request, HttpContext context)
    {
        return Task.CompletedTask;
    }
}