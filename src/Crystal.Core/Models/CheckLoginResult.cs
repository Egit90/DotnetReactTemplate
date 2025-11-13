using Crystal.Core.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Crystal.Core.Models;

public record CheckLoginResult<TUser, TKey>
    where TKey : IEquatable<TKey>
    where TUser : IdentityUser<TKey>
    , ICrystalUser<TKey>, new()
{
    public required string ProviderKey { get; set; }
    public TUser? User { get; set; }

    public void Deconstruct(out string providerKey, out TUser? user)
    {
        providerKey = ProviderKey;
        user = User;
    }
}
