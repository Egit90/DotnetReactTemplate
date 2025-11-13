using Microsoft.AspNetCore.Http;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Crystal.Core.Endpoints.SignIn;

/// <summary>
/// Extension point for the SignInEndpoint.
/// </summary>
/// <typeparam name="TUser"></typeparam>
public interface ISignInEndpointEvents<TKey, in TUser>
        where TKey : IEquatable<TKey>
        where TUser : ICrystalUser<TKey>
{
    Task SignInSucceededAsync(TUser user, HttpContext context);

    /// <summary>
    /// Called when a user fails to sign in.
    /// </summary>
    Task SignInFailedAsync(SignInRequest request, HttpContext context, SignInResult result);

    /// <summary>
    /// User not found.
    /// </summary>
    Task UserNotFound(SignInRequest request, HttpContext context);
}