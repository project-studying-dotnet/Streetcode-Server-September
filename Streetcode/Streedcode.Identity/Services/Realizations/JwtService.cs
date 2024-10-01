using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Streetcode.Identity.Models;
using Streetcode.Identity.Models.Additional;
using Streetcode.Identity.Services.Interfaces;
using System.Security.Claims;
using System.Text;

namespace Streetcode.Identity.Services.Realizations
{
    public class JwtService : IJwtService
    {
        private readonly JwtVariables _environment;
        private readonly string _secret;
        private readonly int _expiration;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtService(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
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

        public async Task<string> Create(ApplicationUser user)
        {
            string secretKey = _secret;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Id.ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
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

             string token = handler.CreateToken(tokenDescriptor)??
                   throw new InvalidOperationException("Token generation failed");

             return token;
        }
    }
}