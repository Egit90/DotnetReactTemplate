using Crystal.Core;
using Crystal.Core.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace WebApi.Data;

public class MyUser : IdentityUser, ICrystalUser
{
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
}