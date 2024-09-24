using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.DAL.Entities.Users;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Microsoft.AspNetCore.Identity;

namespace Streetcode.BLL.Services.JwtService
{
    public class JwtService : IJwtService
    {
        private readonly JwtVariables _environment;
        private readonly string _secret;
        private readonly int _expiration;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public JwtService(UserManager<User> userManager,
                           SignInManager<User> signInManager, 
                           IOptions<JwtVariables> environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment.Value;
            _secret = _environment.Secret;
            _expiration = _environment.ExpirationInMinutes;
            _issuer = _environment.Issuer;
            _audience = _environment.Audience;
        }

        public async Task<string> Create(User user)
        {
            string secretKey = _secret;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim> {
                   new Claim(JwtRegisteredClaimNames.Sid, user.Id.ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim("Roles", role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), 
                Expires = DateTime.UtcNow.AddMinutes(_expiration),
                SigningCredentials = credentials,
                Issuer = _issuer,
                Audience = _audience    
            };

            var handler = new JsonWebTokenHandler();

            string token = handler.CreateToken(tokenDescriptor) ??
                throw new CustomException("Failed to create JWT Token", 
                                    StatusCodes.Status500InternalServerError);

            return token;
        }
    }
}
