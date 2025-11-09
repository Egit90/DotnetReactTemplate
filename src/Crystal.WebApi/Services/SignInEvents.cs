using Crystal.Core.Endpoints.SignIn;
using Microsoft.AspNetCore.Identity;
using WebApi.Data;
namespace WebApi.Services;

public class SignInEvents(UserManager<MyUser> userManager) : ISignInEndpointEvents<MyUser>
{
    public Task SignInFailedAsync(SignInRequest request, HttpContext context, SignInResult result)
    {
        return Task.CompletedTask;
    }

    public async Task SignInSucceededAsync(MyUser user, HttpContext context)
    {
        user.LastLoginDate = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
    }

    public Task UserNotFound(SignInRequest request, HttpContext context)
    {
        return Task.CompletedTask;
    }
}