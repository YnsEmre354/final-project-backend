using BitirmeTezi.Entities;
using BitirmeTezi.ModelsDto.Student;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BitirmeTezi.Auth
{
    public class JwtService
    {
        private readonly JwtSettings _settings;

        public JwtService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public string GenerateToken(LoginResultDto student)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, student.UserId.ToString()),
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_settings.Key)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_settings.ExpireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
