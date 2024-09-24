using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Streetcode.BLL.Dto.Users;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.DAL.Entities.Users;

namespace Streetcode.BLL.MediatR.Users.Login
{
    public class UserLoginHandler : IRequestHandler<LoginCommand, Result<LoginResultDto>>
    {
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public UserLoginHandler(UserManager<User> userManager,
                                 SignInManager<User> signInManager,
                                 IJwtService jwtService,
                                 IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _mapper = mapper;
        }

        public async Task<Result<LoginResultDto>> Handle(LoginCommand command,
                                                         CancellationToken cancellationToken)
        {
            var loginInfo = command.LoginDto;
            var user = await _userManager.FindByEmailAsync(loginInfo.Email) ??
                throw new CustomException("User not found",
                                    StatusCodes.Status404NotFound);

            var signInResult = await _signInManager.PasswordSignInAsync(user,
            loginInfo.Password, false, true);

            if (!signInResult.Succeeded)
            {
                throw new CustomException("Incorrect log in data",
                                         StatusCodes.Status400BadRequest);
            }

            var jwtToken = await _jwtService.CreateJwtTokenAsync(user);

            var refreshToken = await _jwtService.GetRefreshTokenByUserIdAsync(user.Id)
                       ?? await _jwtService.CreateRefreshTokenAsync(user);

            if (refreshToken.IsExpired || refreshToken.IsRevoked) //construct until expired/revoked refTokens
                                                                  //auto-delete implemented
            {
                await _jwtService.DeleteRefreshTokenAsync(refreshToken.Id);
                refreshToken = await _jwtService.CreateRefreshTokenAsync(user);
            }

            var result = new LoginResultDto()
            {
                JwtToken = jwtToken,
                RefreshToken = refreshToken.Token,
                User = _mapper.Map<UserDataDto>(user)
                ?? throw new CustomException("Mapping from User to UserDataDto failed",
                                             StatusCodes.Status400BadRequest)
            };

            return Result.Ok(result);
        }
    }
}
