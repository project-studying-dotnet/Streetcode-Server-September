using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Users.Logout;

public record LogoutCommand (int userId) : IRequest<Result<Unit>>;
