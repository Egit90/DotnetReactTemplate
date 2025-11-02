using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Crystal.Core.Services;

/// <summary>
/// Default implementation of <see cref="IJwtTokenService"/> 
/// </summary>
/// <param name="options"></param>
public class JwtTokenService(IOptions<CrystalOptions> options) : IJwtTokenService
{
    public (string token, DateTime expiresAt) CreateAccessToken(ClaimsPrincipal user)
    {
        if (options.Value.JwtBearer.SigningKey is null)
        {
            throw new ArgumentNullException(nameof(options.Value.JwtBearer.SigningKey));
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(options.Value.JwtBearer.AccessTokenExpiresInMinutes);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = options.Value.JwtBearer.Issuer,
            Audience = options.Value.JwtBearer.Audience,
            IssuedAt = DateTime.UtcNow,
            Subject = new ClaimsIdentity(user.Claims),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.Value.JwtBearer.SigningKey)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return (handler.WriteToken(handler.CreateToken(descriptor)), expiresAt);
    }

    public (string token, DateTime expiresAt) CreateBearerRefreshToken(CrystalRefreshToken token)
    {
        if (options.Value.JwtBearer.SigningKey is null)
        {
            throw new ArgumentNullException(nameof(options.Value.JwtBearer.SigningKey));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = options.Value.JwtBearer.Issuer,
            Audience = options.Value.JwtBearer.Audience,
            IssuedAt = DateTime.UtcNow,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, token.UserId),
                new Claim(CrystalClaimTypes.RefreshToken, token.RefreshToken)
            }),
            Expires = token.ExpiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.Value.JwtBearer.SigningKey)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return (handler.WriteToken(handler.CreateToken(descriptor)), token.ExpiresAt);
    }
}


