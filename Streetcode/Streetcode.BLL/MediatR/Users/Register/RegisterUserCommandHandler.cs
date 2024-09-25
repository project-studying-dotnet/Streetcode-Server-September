using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(UserManager<User> userManager, RoleManager<Role> roleManager, ILogger<RegisterUserCommandHandler> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
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
                Role = userDto.Role.ToString()
            };

            _logger.LogInformation($"Attempting to create user: Name={user.Name}, Email={user.Email}, Role={user.Role}");
            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
            {
                _logger.LogError($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return Result.Fail(result.Errors.Select(e => e.Description).ToList());
            }

            var roleResult = await _userManager.AddToRoleAsync(user, user.Role);

            if (!roleResult.Succeeded)
            {
                _logger.LogError($"Failed to add user to role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                await _userManager.DeleteAsync(user);
                return Result.Fail(roleResult.Errors.Select(e => e.Description).ToList());
            }

            var createdUserDto = new UserDto
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                UserName = user.UserName,
                Role = Enum.Parse<UserRole>(user.Role)
            };

            return Result.Ok(createdUserDto);
        }
    }
}
