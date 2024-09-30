using AutoMapper;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Streetcode.BLL.Dto.Users;
using Streetcode.BLL.MediatR.Users.Register;
using Streetcode.DAL.Entities.Role;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Enums;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Users.Register
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<RoleManager<Role>> _roleManagerMock;
        private readonly Mock<ILogger<RegisterUserCommandHandler>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly RegisterUserCommandHandler _handler;

        public RegisterUserCommandHandlerTests()
        {
            _userManagerMock = GetUserManagerMock();
            _roleManagerMock = GetRoleManagerMock();
            _loggerMock = new Mock<ILogger<RegisterUserCommandHandler>>();
            _mapperMock = new Mock<IMapper>();

            _handler = new RegisterUserCommandHandler(
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _loggerMock.Object,
                _mapperMock.Object);
        }
        private Mock<RoleManager<Role>> GetRoleManagerMock()
        {
            var store = new Mock<IRoleStore<Role>>();
            return new Mock<RoleManager<Role>>(store.Object, null, null, null, null);
        }

        private Mock<UserManager<User>> GetUserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserDto_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var registerUserDto = new RegisterUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Password123!",
                Role = UserRole.User
            };

            var user = new User
            {
                Name = registerUserDto.Name,
                Email = registerUserDto.Email,
                UserName = registerUserDto.Email,
                Role = registerUserDto.Role.ToString()
            };

            _mapperMock.Setup(m => m.Map<User>(registerUserDto)).Returns(user);

            _userManagerMock.Setup(um => um.CreateAsync(user, registerUserDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(um => um.AddToRoleAsync(user, user.Role))
                .ReturnsAsync(IdentityResult.Success);

            var expectedUserDto = new UserDto
            {
                Name = user.Name,
                Email = user.Email,
                Role = registerUserDto.Role
            };

            _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(expectedUserDto);

            var command = new RegisterUserCommand(registerUserDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedUserDto);

            _userManagerMock.Verify(um => um.CreateAsync(user, registerUserDto.Password), Times.Once);
            _userManagerMock.Verify(um => um.AddToRoleAsync(user, user.Role), Times.Once);
            _mapperMock.Verify(m => m.Map<User>(registerUserDto), Times.Once);
            _mapperMock.Verify(m => m.Map<UserDto>(user), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserCreationFails()
        {
            // Arrange
            var registerUserDto = new RegisterUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Password123!",
                Role = UserRole.User
            };

            var user = new User
            {
                Name = registerUserDto.Name,
                Email = registerUserDto.Email,
                UserName = registerUserDto.Email,
                Role = registerUserDto.Role.ToString()
            };

            _mapperMock.Setup(m => m.Map<User>(registerUserDto)).Returns(user);

            var identityErrors = new IdentityError[] { new IdentityError { Description = "User creation failed." } };
            _userManagerMock.Setup(um => um.CreateAsync(user, registerUserDto.Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            var command = new RegisterUserCommand(registerUserDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Message == "User creation failed.");

            _userManagerMock.Verify(um => um.CreateAsync(user, registerUserDto.Password), Times.Once);
            _userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _mapperMock.Verify(m => m.Map<User>(registerUserDto), Times.Once);
            _loggerMock.Verify(
                 x => x.Log(
                 LogLevel.Error,
                 It.IsAny<EventId>(),
                 It.Is<It.IsAnyType>((v, t) => true),
                 It.IsAny<Exception>(),
                 It.IsAny<Func<It.IsAnyType, Exception, string>>()),Times.Once);
        }


        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenAddingToRoleFails()
        {
            // Arrange
            var registerUserDto = new RegisterUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Password123!",
                Role = UserRole.User
            };

            var user = new User
            {
                Name = registerUserDto.Name,
                Email = registerUserDto.Email,
                UserName = registerUserDto.Email,
                Role = registerUserDto.Role.ToString()
            };

            _mapperMock.Setup(m => m.Map<User>(registerUserDto)).Returns(user);

            _userManagerMock.Setup(um => um.CreateAsync(user, registerUserDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            var identityErrors = new IdentityError[] { new IdentityError { Description = "Add to role failed." } };
            _userManagerMock.Setup(um => um.AddToRoleAsync(user, user.Role))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            _userManagerMock.Setup(um => um.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var command = new RegisterUserCommand(registerUserDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Message == "Add to role failed.");

            _userManagerMock.Verify(um => um.CreateAsync(user, registerUserDto.Password), Times.Once);
            _userManagerMock.Verify(um => um.AddToRoleAsync(user, user.Role), Times.Once);
            _userManagerMock.Verify(um => um.DeleteAsync(user), Times.Once);
            _mapperMock.Verify(m => m.Map<User>(registerUserDto), Times.Once);
            _loggerMock.Verify(
                  x => x.Log(
                  LogLevel.Error,
                  It.IsAny<EventId>(),
                  It.Is<It.IsAnyType>((v, t) => true),
                  It.IsAny<Exception>(),
                  It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
      
    }
}
