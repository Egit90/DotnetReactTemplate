using System.ComponentModel.DataAnnotations;

namespace Crystal.Core.Endpoints.Token;

public class TokenRequest
{
    [Required, EmailAddress] public string? Email { get; set; }
    [Required] public string? Password { get; set; }
}