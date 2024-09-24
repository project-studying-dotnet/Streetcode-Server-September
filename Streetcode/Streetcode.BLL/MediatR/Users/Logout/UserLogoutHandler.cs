using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.BLL.Services.JwtService;
using Streetcode.DAL.Entities.Users;

namespace Streetcode.BLL.MediatR.Users.Logout
{
    public class UserLogoutHandler : IRequestHandler<LogoutCommand, Result<Unit>>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;

        public UserLogoutHandler(UserManager<User> userManager, 
                                 SignInManager<User> signInManager,
                                 IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
    }
        public async Task<Result<Unit>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            //rewoke refresh token
            var user = await _userManager.FindByIdAsync(request.userId.ToString());
            if (user == null)
            {
                throw new CustomException("User not found", StatusCodes.Status404NotFound);
            }

            var refreshToken = await _jwtService.GetRefreshTokenByUserIdAsync(user.Id);

            if (refreshToken != null && !refreshToken.IsRevoked)
            {
                refreshToken.IsRevoked = true; 
                await _jwtService.UpdateRefreshTokenAsync(refreshToken); 
            }

            await _signInManager.SignOutAsync();

            return Result.Ok(Unit.Value);
        }
    }
}
