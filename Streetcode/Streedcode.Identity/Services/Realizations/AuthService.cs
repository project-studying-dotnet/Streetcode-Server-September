using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Streetcode.Identity.Models;
using Streetcode.Identity.Models.Dto;
using Streetcode.Identity.Services.Interfaces;

namespace Streetcode.Identity.Services.Realizations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;

        public AuthService(UserManager<ApplicationUser> userManager, 
                        SignInManager<ApplicationUser> signInManager, 
                        IJwtService jwtService, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _mapper = mapper;
        }

        public async Task<LoginResultDto> LoginAsync(LoginDto loginDto)
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

            var refreshToken = await _jwtService.GetValidRefreshTokenByUserIdAsync(user.Id) ??
                                          await _jwtService.CreateRefreshTokenAsync(user);


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

        public async Task<string> RefreshJwtToken(int userId)
        {
            var token = await _jwtService.GetValidRefreshTokenByUserIdAsync(userId) 
                            ?? throw new KeyNotFoundException("Refresh token not found");

            var user = await _userManager.FindByIdAsync(token.UserId.ToString());

            var newJwtToken = await _jwtService.CreateJwtTokenAsync(user);

            return newJwtToken;
        }
    }
}
