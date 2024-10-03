using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Streetcode.Identity.Exceptions;
using Streetcode.Identity.Models;
using Streetcode.Identity.Models.Dto;
using Streetcode.Identity.Services.Interfaces;
using System.Security.Claims;

namespace Streetcode.Identity.Services.Realizations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;

        public AuthService(UserManager<ApplicationUser> userManager, 
                        SignInManager<ApplicationUser> signInManager, 
                        IJwtService jwtService, IMapper mapper,
                        IHttpContextAccessor contextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _mapper = mapper;
            _httpContextAccessor = contextAccessor;
        }

        public async Task<LoginResultDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email) ??
                throw new ArgumentException("User not found");

            var signInResult = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, true);
            if (!signInResult.Succeeded)
            {
                throw new InvalidOperationException("Incorrect login data");
            }

            var jwtResult = await _jwtService.CreateJwtTokenAsync(user) ??
                throw new InvalidOperationException("Jwt token generation failed");

            var refreshToken = await _jwtService.CreateRefreshTokenAsync(user);


            var userData = _mapper.Map<UserDataDto>(user) ??
                throw new InvalidOperationException("Mapping from User to UserDataDto failed");

            if (refreshToken?.Token == null)
            {
                throw new InvalidOperationException("Failed to generate or retrieve a valid refresh token.");
            }

            return new LoginResultDto
            {
                RefreshToken = refreshToken.Token,
                Token = jwtResult,
                User = userData
            };
        }

        public async Task LogoutAsync(CancellationToken cancellationToken)
        {
            var currentUserIdClaim = _httpContextAccessor.HttpContext?.User?
                                            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (currentUserIdClaim == null)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }

            var currentUserId = int.Parse(currentUserIdClaim);

            var refreshToken = await _jwtService.GetValidRefreshTokenByUserIdAsync(currentUserId, cancellationToken);

            refreshToken.IsRevoked = true;
            await _jwtService.UpdateTokenAsync(refreshToken);
            await _signInManager.SignOutAsync();
        }

        public async Task<string> RefreshJwtToken(CancellationToken cancellationToken)
        {
            var currentUserIdClaim = _httpContextAccessor.HttpContext?.User?
                                           .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (currentUserIdClaim == null)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }

            var currentUserId = int.Parse(currentUserIdClaim);

            var token = await _jwtService.GetValidRefreshTokenByUserIdAsync(currentUserId, cancellationToken) 
                            ?? throw new KeyNotFoundException("Refresh token not found. Login again");

            var user = await _userManager.FindByIdAsync(token.UserId.ToString());

            var newJwtToken = await _jwtService.CreateJwtTokenAsync(user);

            return newJwtToken;
        }
    }
}
