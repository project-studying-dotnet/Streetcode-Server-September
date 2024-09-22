﻿using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.GetById;

public class GetFactByIdHandler : IRequestHandler<GetFactByIdQuery, Result<FactDto>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetFactByIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<FactDto>> Handle(GetFactByIdQuery request, CancellationToken cancellationToken)
    {
        var facts = await _repositoryWrapper.FactRepository.GetFirstOrDefaultAsync(f => f.Id == request.Id);

        if (facts is null)
            throw new CustomException($"Cannot find any fact with corresponding id: {request.Id}", StatusCodes.Status204NoContent);

        return Result.Ok(_mapper.Map<FactDto>(facts));
    }
}