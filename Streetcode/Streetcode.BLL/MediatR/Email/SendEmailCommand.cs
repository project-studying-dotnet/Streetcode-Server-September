using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Email;

namespace Streetcode.BLL.MediatR.Email;
public record SendEmailCommand(EmailDto Email) : IRequest<Result<Unit>>;
