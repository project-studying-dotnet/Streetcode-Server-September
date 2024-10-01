﻿using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Localization;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;
using Streetcode.BLL.Extensions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Text.GetById;

public class GetTextByIdHandler : IRequestHandler<GetTextByIdQuery, Result<TextDto>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly IStringLocalizer _stringLocalizer;

    public GetTextByIdHandler(
        IRepositoryWrapper repositoryWrapper, 
        IMapper mapper, 
        ILoggerService logger, 
        IStringLocalizer<ErrorMessages> stringLocalizerFactory)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
        _stringLocalizer = stringLocalizerFactory;
    }

    public async Task<Result<TextDto>> Handle(GetTextByIdQuery request, CancellationToken cancellationToken)
    {
        var text = await _repositoryWrapper.TextRepository.GetFirstOrDefaultAsync(f => f.Id == request.Id);

        if (text is null)
        {
            string errorMsg = _stringLocalizer.GetErrorMessage(ErrorKeys.NotFoundError, nameof(Text), request.Id);
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        return Result.Ok(_mapper.Map<TextDto>(text));
    }
}