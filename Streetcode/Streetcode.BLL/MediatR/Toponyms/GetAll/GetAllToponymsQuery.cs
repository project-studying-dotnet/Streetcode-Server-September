using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Toponyms;

namespace Streetcode.BLL.MediatR.Toponyms.GetAll;

public record GetAllToponymsQuery(GetAllToponymsRequestDto request)
    : IRequest<Result<GetAllToponymsResponseDto>>;