using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Streetcode.BLL.Dto.Users;
using Streetcode.DAL.Entities.Role;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Enums;


namespace Streetcode.BLL.MediatR.Users.Register
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public RegisterUserCommandHandler(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var userDto = request.UserDto;

            var user = new User
            {
                Name = userDto.Name,
                Surname = userDto.Surname,
                Email = userDto.Email,
                UserName = userDto.UserName,
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
            {
                return Result.Fail(result.Errors.Select(e => e.Description).ToList());
            }

            if (!await _roleManager.RoleExistsAsync(userDto.Role))
            {
                var roleResult = await _roleManager.CreateAsync(new Role { Name = userDto.Role });
                if (!roleResult.Succeeded)
                {
                    return Result.Fail(roleResult.Errors.Select(e => e.Description).ToList());
                }
            }

            await _userManager.AddToRoleAsync(user, "User");

            return Result.Ok();
        }
    }
}
