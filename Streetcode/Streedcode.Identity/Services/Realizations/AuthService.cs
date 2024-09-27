using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Streetcode.Identity.Data;
using Streetcode.Identity.Models;
using Streetcode.Identity.Models.Dto;
using Streetcode.Identity.Services.Interfaces;

namespace Streetcode.Identity.Services.Realizations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;

        public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, 
                        SignInManager<ApplicationUser> signInManager, IJwtService jwtService, IMapper mapper)
        {
            _db = db;
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

            var jwtResult = await _jwtService.Create(user) ??
                throw new InvalidOperationException("Token generation failed");

            var userData = _mapper.Map<UserDataDto>(user) ??
                throw new InvalidOperationException("Mapping from User to UserDataDto failed");

            return new LoginResultDto
            {
                Token = jwtResult,
                User = userData
            };
        }
    }
}
