using Chat_App.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Chat_App.Code
{
    public class AuthHandler
    {
        private readonly string _secretKey;

        public AuthHandler(IConfiguration configuration)
        {
            _secretKey = configuration.GetValue<string>("ApiResponse:SecretKey");
        }
        public string GetJWT(User user) 
        {
            try
            {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("fullName", user.Name), 
                    new Claim("id", user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            // Create the token
            return tokenHandler.WriteToken(token);
            }
            catch(Exception ex)
            {
                return "";
            }
        }
    }
}
