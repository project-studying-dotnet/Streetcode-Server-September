using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.DAL.Entities.Users;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;

namespace Streetcode.BLL.Services.JwtService
{
    public class JwtService : IJwtService
    {
        private readonly JwtVariables _environment;
        private readonly string _secret;
        private readonly int _expiration;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtService(IOptions<JwtVariables> environment)
        {
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
                   new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                   new Claim(JwtRegisteredClaimNames.Sid, user.Id.ToString()),
                   new Claim("role", user.Role.ToString())
            };

            //var roles = await _userManager.GetRolesAsync(user);
            //foreach (var role in roles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, role));
            //}

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), 
                Expires = DateTime.UtcNow.AddMinutes(_expiration),
                SigningCredentials = credentials,
                Issuer = _issuer,
                Audience = _audience    
            };

            var handler = new JsonWebTokenHandler();

            string token = handler.CreateToken(tokenDescriptor);
            
            if (token is null)
            {
                throw new CustomException("Failed to crate JWT Token", 
                                    StatusCodes.Status500InternalServerError);
            }

            return token;
        }

        //public string GenerateRefreshTokenString()
        //{
        //    var randomNumber = new byte[64];

        //    using (var numberGenerator = RandomNumberGenerator.Create()) { 
        //        numberGenerator.GetBytes(randomNumber); 
        //    }    

        //    return Convert.ToBase64String(randomNumber);    
        //}

        //public async Task<RefreshToken> CreateRefreshTokenAsync(User user)
        //{
        //    var refreshToken = new RefreshToken
        //    {
        //        Token = GenerateRefreshTokenString(),
        //        UserId = user.Id,
        //        ExpiryDate = DateTime.UtcNow.AddDays(1) 
        //    };

        //    await _repositoryWrapper.RefreshTokenRepository.CreateAsync(refreshToken);  

        //    return refreshToken;
        //}

        //public async Task<RefreshToken> GetRefreshToken(User user)
        //{
        //    var storedRefreshToken = await _repositoryWrapper.RefreshTokenRepository
        //        .GetFirstOrDefaultAsync(predicate: rt => rt.UserId == user.Id);

        //    return storedRefreshToken;
        //}
    }
}
