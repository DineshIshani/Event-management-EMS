using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventManagement.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace EventManagement.Api.Services;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "EventManagement";

    public string Audience { get; set; } = "EventManagement";

    public int ExpiryMinutes { get; set; } = 480;
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}

public class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(JwtOptions options)
    {
        _options = options;
    }

    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
