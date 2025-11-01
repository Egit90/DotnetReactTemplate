using System.ComponentModel.DataAnnotations;

namespace Crystal.Core.Endpoints.Email;

public class EmailConfirmationResendRequest
{
    [Required, EmailAddress]
    public string? Email { get; set; }
}
