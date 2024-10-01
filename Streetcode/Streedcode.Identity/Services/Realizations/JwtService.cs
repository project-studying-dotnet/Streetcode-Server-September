using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Streetcode.Identity.Models;
using Streetcode.Identity.Models.Additional;
using Streetcode.Identity.Repository;
using Streetcode.Identity.Services.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Streetcode.Identity.Services.Realizations
{
    public class JwtService : IJwtService
    {
        private readonly JwtVariables _jwtEnvironment;
        private readonly string _secret;
        private readonly int _expiration;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly RefreshVariables _refreshEnvironment;
        private readonly int _refreshExpiration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRefreshRepository _refreshRepository;
        private readonly ICacheService _cacheService;

        public JwtService(UserManager<ApplicationUser> userManager,
                           IRefreshRepository refreshRepository, 
                           IOptions<JwtVariables> jwtEnvironment,
                           IOptions<RefreshVariables> refreshEnvironment,
                           ICacheService cacheService)
        {
            _userManager = userManager;
            _jwtEnvironment = jwtEnvironment.Value;
            _secret = _jwtEnvironment.Secret;
            _expiration = _jwtEnvironment.ExpirationInMinutes;
            _issuer = _jwtEnvironment.Issuer;
            _audience = _jwtEnvironment.Audience;
            _refreshEnvironment = refreshEnvironment.Value;
            _refreshExpiration = _refreshEnvironment.ExpirationInDays;
            _refreshRepository = refreshRepository;
            _cacheService = cacheService;
        }

        public async Task<string> CreateJwtTokenAsync(ApplicationUser user)
        {
            string secretKey = _secret;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
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

        public async Task<RefreshToken> CreateRefreshTokenAsync(ApplicationUser user)
        {
            int userid = user.Id;
            string key = $"user-{userid}";

            var randomNumber = new byte[64];

            using (var numberGenerator = RandomNumberGenerator.Create())
            {
                numberGenerator.GetBytes(randomNumber);
            }

            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                UserId = user.Id,
                ExpiryDate = DateTime.Now.AddDays(_refreshExpiration),
            };

            var result = await _refreshRepository.CreateAsync(refreshToken);

            var resultIsSuccess = await _refreshRepository.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                await _cacheService.SetAsync(key, refreshToken);
                return result;
            }
            else
            {
                throw new InvalidOperationException("Failed to create Refresh token");
            }
        }

        public async Task<List<RefreshToken>> GetAllRefreshsTokenByUserIdAsync(int id, CancellationToken cancellationToken)
        {
            string key = $"user-{id}";

            var result = await _cacheService.GetAsync(
              key,
              async () => (await _refreshRepository.GetByUserIdAsync(id)),
              cancellationToken: cancellationToken);

            return result;
        }

        public async Task<RefreshToken> GetValidRefreshTokenByUserIdAsync(int id, CancellationToken cancellationToken)
        {
            string key = $"user-{id}";

            var result = await _cacheService.GetAsync(
             key,
             async () => (await _refreshRepository.GetValidByUserIdAsync(id)),
             cancellationToken: cancellationToken);

            return result;
        }
    }
}