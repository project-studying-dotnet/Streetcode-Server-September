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
using System.Security.Cryptography;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.Services.JwtService;

public class JwtService : IJwtService
{
    private readonly JwtVariables _jwtEnvironment;
    private readonly string _secret;
    private readonly int _jwtExpiration;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly RefreshVariables _refreshEnvironment;
    private readonly int _refreshExpiration;
    private readonly UserManager<User> _userManager;
    private readonly IRepositoryWrapper _repositoryWrapper;   

    public JwtService(UserManager<User> userManager,
                       IOptions<JwtVariables> jwtEnvironment,
                       IOptions<RefreshVariables> refreshEnvironment,
                       IRepositoryWrapper repositoryWrapper)
    {
        _userManager = userManager;
        _jwtEnvironment = jwtEnvironment.Value;
        _secret = _jwtEnvironment.Secret;
        _jwtExpiration = _jwtEnvironment.ExpirationInMinutes;
        _issuer = _jwtEnvironment.Issuer;
        _audience = _jwtEnvironment.Audience;
        _refreshEnvironment = refreshEnvironment.Value;
        _refreshExpiration = _refreshEnvironment.ExpirationInDays;   
        _repositoryWrapper = repositoryWrapper; 
    }

    public async Task<string> CreateJwtTokenAsync(User user)
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
            Expires = DateTime.UtcNow.AddMinutes(_jwtExpiration),
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

    public async Task<RefreshToken> CreateRefreshTokenAsync(User user)
    {
        var randomNumber = new byte[64];

        using (var numberGenerator = RandomNumberGenerator.Create()) { 
            numberGenerator.GetBytes(randomNumber); 
        }

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            UserId = user.Id,
            ExpiryDate = DateTime.Now.AddDays(_refreshExpiration),
        };

        var result = await _repositoryWrapper.RefreshTokenRepository.CreateAsync(refreshToken);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (resultIsSuccess)
        {
            return result;
        }
        else
        {
            throw new CustomException("Failed to create Refresh token",
                                StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<RefreshToken> GetRefreshTokenByUserIdAsync(int id)
    {
        var result = await _repositoryWrapper.RefreshTokenRepository
                                 .GetFirstOrDefaultAsync(rt => rt.UserId == id);

        return result;
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        var existingToken = await _repositoryWrapper.RefreshTokenRepository
            .GetFirstOrDefaultAsync((rt => rt.Id == refreshToken.Id)) ??
            throw new CustomException("Refresh token not found", StatusCodes.Status404NotFound);

        existingToken.Token = refreshToken.Token; 
        existingToken.ExpiryDate = refreshToken.ExpiryDate; 
        existingToken.IsRevoked = refreshToken.IsRevoked; 

        _repositoryWrapper.RefreshTokenRepository.Update(existingToken);
        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if(!resultIsSuccess)
        {
            throw new CustomException("Failed to update Refresh token",
                                StatusCodes.Status500InternalServerError);
        }
    }

    public async Task DeleteRefreshTokenAsync(int id)
    {
        var existingToken = await _repositoryWrapper.RefreshTokenRepository
            .GetFirstOrDefaultAsync((rt => rt.Id == id)) ??
            throw new CustomException("Refresh token not found", StatusCodes.Status404NotFound);

        _repositoryWrapper.RefreshTokenRepository.Delete(existingToken);
        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (!resultIsSuccess)
        {
            throw new CustomException("Failed to delete Refresh token",
                                StatusCodes.Status500InternalServerError);
        }
    }
}
