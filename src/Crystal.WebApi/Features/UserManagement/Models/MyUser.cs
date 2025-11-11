using Crystal.Core.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace WebApi.Features.UserManagement.Models;

public class MyUser : IdentityUser, ICrystalUser
{
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
}