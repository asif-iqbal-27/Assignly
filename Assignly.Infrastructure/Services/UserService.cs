using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Assignly.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;

    public UserService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<ErrorOr<string>> GenerateTokenAsync(ApplicationUser user)
    {
        if (user.Role is null)
        {
            return Task.FromResult<ErrorOr<string>>(Error.Unauthorized(description: "User has no assigned role."));
        }

        var issuer = _configuration["Jwt:Issuer"] ?? "Assignly";
        var audience = _configuration["Jwt:Audience"] ?? "AssignlyClients";
        var keyValue = _configuration["Jwt:Key"] ?? "replace-with-a-strong-secret-key";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Role, user.Role.Value.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult<ErrorOr<string>>(tokenHandler.WriteToken(token));
    }
}
