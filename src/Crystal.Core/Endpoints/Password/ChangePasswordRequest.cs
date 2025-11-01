using System.ComponentModel.DataAnnotations;

namespace Crystal.Core.Endpoints.Password;

public class ChangePasswordRequest
{
    [Required] public string? Password { get; set; }
    [Required] public string? NewPassword { get; set; }
}