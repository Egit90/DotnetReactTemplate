using System.Security.Claims;

namespace Crystal.Core.Abstractions;

public interface ICrystalUserManager
{
    Task<(bool result, string? error)> ShouldUseExternalSignUpFlow(ClaimsIdentity identity);
}
