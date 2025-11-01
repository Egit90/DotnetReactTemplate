using System.ComponentModel.DataAnnotations;

namespace Crystal.Core.Endpoints.Password;

public class PasswordForgotRequest
{
    [Required, EmailAddress] public string? Email { get; set; }
}