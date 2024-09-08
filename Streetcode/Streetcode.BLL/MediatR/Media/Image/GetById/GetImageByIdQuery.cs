using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Images;

namespace Streetcode.BLL.MediatR.Media.Image.GetById;

public record GetImageByIdQuery(int Id) : IRequest<Result<ImageDto>>;
