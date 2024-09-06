using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Images;

namespace Streetcode.BLL.MediatR.Media.Image.GetByStreetcodeId;

public record GetImageByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<IEnumerable<ImageDto>>>;