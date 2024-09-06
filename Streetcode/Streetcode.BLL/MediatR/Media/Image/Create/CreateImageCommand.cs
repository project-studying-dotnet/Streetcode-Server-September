using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Images;

namespace Streetcode.BLL.MediatR.Media.Image.Create;

public record CreateImageCommand(ImageFileBaseCreateDto Image) : IRequest<Result<ImageDto>>;
