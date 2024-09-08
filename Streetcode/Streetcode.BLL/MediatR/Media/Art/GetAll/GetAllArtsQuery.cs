using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Art;

namespace Streetcode.BLL.MediatR.Media.Art.GetAll;

public record GetAllArtsQuery : IRequest<Result<IEnumerable<ArtDto>>>;