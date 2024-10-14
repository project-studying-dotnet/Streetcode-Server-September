using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Streetcode.Identity.Models;
using Streetcode.Identity.Models.Dto;
using Streetcode.Identity.Services.Interfaces;
using Xunit;
using Streetcode.Identity.Models.Enums;

using AuthServiceClass = Streetcode.Identity.Services.Realizations.AuthService;
using Microsoft.Extensions.Configuration;
using Streetcode.Identity.Models.AzureBus;

namespace Streetcode.XUnitTest.IdentityMicroservice.AuthService;

public class RegisterAsyncTests
{
    private readonly AuthServiceClass _authService;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IUserClaimsPrincipalFactory<ApplicationUser>> _claimsPrincipalFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IConfigurationSection> _configurationSectionMock;
    private readonly Mock<IAzureBusService> _azureBusServiceMock;
    private readonly string _emailMessageTopic = "email-topic-test";
    public RegisterAsyncTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object,
            null, null, null, null, null, null, null, null
        );

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _claimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            _httpContextAccessorMock.Object,
            _claimsPrincipalFactoryMock.Object,
            null, null, null, null
        );

        _jwtServiceMock = new Mock<IJwtService>();
        _mapperMock = new Mock<IMapper>();
        _azureBusServiceMock = new Mock<IAzureBusService>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationSectionMock = new Mock<IConfigurationSection>();

        _configurationSectionMock.Setup(x => x.Value).Returns(_emailMessageTopic);
        _configurationMock.Setup(config => config.GetSection("ServiceBusSettings:EmailMessageTopic"))
                          .Returns(_configurationSectionMock.Object);

        _authService = new AuthServiceClass(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _jwtServiceMock.Object,
            _mapperMock.Object,
            _httpContextAccessorMock.Object,
            _configurationMock.Object,
            _azureBusServiceMock.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_UserCreationSuccess_ReturnsUserDto()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            Role = UserRole.User
        };

        var applicationUser = new ApplicationUser { UserName = "test@example.com", Email = "test@example.com" };
        var userDto = new UserDto { Email = "test@example.com", Role = UserRole.User };

        _mapperMock.Setup(m => m.Map<ApplicationUser>(registerDto)).Returns(applicationUser);
        _mapperMock.Setup(m => m.Map<UserDto>(applicationUser)).Returns(userDto);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userDto.Email, result.Email);
        Assert.Equal(userDto.Role, result.Role);

        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);

        _azureBusServiceMock.Verify(x => x.PublishMessage(
            It.Is<AzureRegisterMailDto>(m => m.To == userDto.Email),
            _emailMessageTopic
        ), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_UserCreationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            Role = UserRole.User
        };

        var applicationUser = new ApplicationUser { UserName = "test@example.com", Email = "test@example.com" };

        _mapperMock.Setup(m => m.Map<ApplicationUser>(registerDto)).Returns(applicationUser);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to create user" }));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _authService.RegisterAsync(registerDto));

        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_UserRoleAdditionFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            Role = UserRole.User
        };

        var applicationUser = new ApplicationUser { UserName = "test@example.com", Email = "test@example.com" };

        _mapperMock.Setup(m => m.Map<ApplicationUser>(registerDto)).Returns(applicationUser);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to add user to role" }));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _authService.RegisterAsync(registerDto));
    }


    [Fact]
    public async Task RegisterAsync_MappingFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            Role = UserRole.User
        };

        var applicationUser = new ApplicationUser { UserName = "test@example.com", Email = "test@example.com" };

        _mapperMock.Setup(m => m.Map<ApplicationUser>(registerDto)).Returns(applicationUser);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
             .ReturnsAsync(IdentityResult.Success);

        _mapperMock.Setup(m => m.Map<UserDto>(applicationUser)).Returns((UserDto)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _authService.RegisterAsync(registerDto));
        Assert.Equal("Failed to map", exception.Message);
    }
}
