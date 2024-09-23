using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Streetcode.BLL.Dto.Users;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Enums;

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
            User user = new User()
            {
                Id = 1,
                Email = "smth@gmail.com",
                Name = "Test",  
                Surname = "Testov",
                Login = "test_login",
                Password = "test_password",
                Role = UserRole.Administrator
            };

            if (user is null)
            {
                throw new CustomException("User not found",
                                    StatusCodes.Status404NotFound);
            }

            //var user = _userManager.FindByEmailAsync(loginDto.Email);
            //var loginResult = await _signInManager.PasswordSignInAsync(user.Result,
            //loginDto.Password, loginDto.RememberMe, true);
            //?? throw new CustomException("Mapping from User to UserDataDto failed",
            //                                StatusCodes.Status400BadRequest)

            var jwtToken = await _jwtService.Create(user);

            var result = new LoginResultDto()
            {
                Token = jwtToken,
                User = _mapper.Map<UserDataDto>(user)
                ?? throw new CustomException("Mapping from User to UserDataDto failed",
                                             StatusCodes.Status400BadRequest)
            };

            return Result.Ok(result);
        }
    }
}
