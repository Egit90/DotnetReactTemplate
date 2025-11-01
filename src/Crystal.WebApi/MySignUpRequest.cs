using Crystal.Core.Endpoints;
using Crystal.Core.Endpoints.SignUp;

namespace WebApi;

public interface IMySignUpRequest
{
    string? AboutMe { get; set; }
    string? MySiteUrl { get; set; }
}

public class MySignUpRequest : SignUpRequest, IMySignUpRequest
{
    public string? AboutMe { get; set; }
    public string? MySiteUrl { get; set; }
}

public class MySignUpExternalRequest : SignUpExternalRequest, IMySignUpRequest
{
    public string? AboutMe { get; set; }
    public string? MySiteUrl { get; set; }
}