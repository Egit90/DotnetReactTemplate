using System.ComponentModel.DataAnnotations;

namespace Crystal.Core.Endpoints.Password;

public class PasswordResetRequest
{
    [Required] public string? Code { get; set; }
    [Required, EmailAddress] public string? Email { get; set; }
    [Required] public string? Password { get; set; }
}