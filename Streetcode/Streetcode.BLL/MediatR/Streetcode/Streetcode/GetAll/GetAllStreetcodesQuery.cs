using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetAll;

public record GetAllStreetcodesQuery(GetAllStreetcodesRequestDto request)
    : IRequest<Result<GetAllStreetcodesResponseDto>>;