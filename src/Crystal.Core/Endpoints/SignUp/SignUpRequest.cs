using System.ComponentModel.DataAnnotations;

namespace Crystal.Core.Endpoints.SignUp;

public class SignUpRequest
{
    [Required, EmailAddress] public string? Email { get; set; }
    [Required] public string? Password { get; set; }
}
