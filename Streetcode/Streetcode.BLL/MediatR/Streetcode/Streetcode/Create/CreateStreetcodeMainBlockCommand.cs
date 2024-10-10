using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;

public record CreateStreetcodeMainBlockCommand(StreetcodeMainBlockCreateDto StreetcodeMainBlockCreateDto)
    : IRequest<Result<int>>;