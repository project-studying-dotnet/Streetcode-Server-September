using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Users.Refresh;

public record RefreshTokenCommand(int userId) : IRequest<Result<string>>;
