using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Toponyms;

namespace Streetcode.BLL.MediatR.Toponyms.GetById;

public record GetToponymByIdQuery(int Id) : IRequest<Result<ToponymDto>>;
