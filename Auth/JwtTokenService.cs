using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;                          //To encode the JWT secret key into bytes.
using Microsoft.IdentityModel.Tokens;
using Dealership.Api.Domain;

namespace Dealership.Api.Auth;

public class JwtTokenService(IConfiguration cfg)                //Service responsible for creating JWTs.
{
    private readonly string _issuer = cfg["Jwt:Issuer"]!;
    private readonly string _audience = cfg["Jwt:Audience"]!;
    private readonly string _key = cfg["Jwt:Key"]!;

    public string Create(User u, TimeSpan? lifetime = null)
    {
        var now = DateTime.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, u.Id),
            new Claim(JwtRegisteredClaimNames.Email, u.Email),
            new Claim(ClaimTypes.Role, u.Role)
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(lifetime ?? TimeSpan.FromMinutes(30)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
