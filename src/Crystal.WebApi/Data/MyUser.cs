using Crystal.Core;
using Crystal.Core.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace WebApi.Data;

public class MyUser : IdentityUser, ICrystalUser
{
    public string? AboutMe { get; set; }
    public string? MySiteUrl { get; set; }
}