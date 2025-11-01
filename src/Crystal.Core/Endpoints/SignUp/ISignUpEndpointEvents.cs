using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Crystal.Core.Endpoints.SignUp;

/// <summary>
/// Extension point for the SignUpEndpoint.
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TModel"></typeparam>
public interface ISignUpEndpointEvents<in TUser, in TModel> where TModel : SignUpRequest where TUser : ICrystalUser
{
    /// <summary>
    /// Called when a user is being created. <br/>
    /// Return a ProblemHttpResult if the user can't be created.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="httpRequest"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<ProblemHttpResult?> UserCreatingAsync(TModel model, HttpRequest httpRequest, TUser user)
    {
        return Task.FromResult<ProblemHttpResult?>(null);
    }

    /// <summary>
    /// Called when a user is created and saved to the database.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="httpRequest"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    Task UserCreatedAsync(TModel model, HttpRequest httpRequest, TUser user)
    {
        return Task.CompletedTask;
    }
}
