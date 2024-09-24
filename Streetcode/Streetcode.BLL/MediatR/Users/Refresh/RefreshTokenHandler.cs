using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.DAL.Entities.Users;

namespace Streetcode.BLL.MediatR.Users.Refresh;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<string>>
{
    private readonly IJwtService _jwtService;
    private readonly UserManager<User> _userManager;

    public RefreshTokenHandler(IJwtService jwtService, UserManager<User> userManager)
    {
        _jwtService = jwtService;
        _userManager = userManager; 
    }

    public async Task<Result<string>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _jwtService.GetRefreshTokenByUserIdAsync(request.userId);

        if (token == null || token.IsRevoked || token.IsExpired)
        {
            throw new CustomException("Invalid or expired refresh token. Log in again",
                                      StatusCodes.Status401Unauthorized);
        }

        var user = await _userManager.FindByIdAsync(token.UserId.ToString());

        var newJwtToken = await _jwtService.CreateJwtTokenAsync(user);


        return newJwtToken;
    }
}
