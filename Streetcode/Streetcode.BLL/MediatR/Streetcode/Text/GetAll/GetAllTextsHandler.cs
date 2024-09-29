using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Services.Cache;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Text.GetAll;

using Text = DAL.Entities.Streetcode.TextContent.Text;

public class GetAllTextsHandler : IRequestHandler<GetAllTextsQuery, Result<IEnumerable<TextDto>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly ICacheService _cacheService;

    public GetAllTextsHandler(
        IRepositoryWrapper repositoryWrapper, 
        IMapper mapper, 
        ILoggerService logger,
        ICacheService cacheService)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<Result<IEnumerable<TextDto>>> Handle(GetAllTextsQuery request, CancellationToken cancellationToken)
    {
        const string key = "text-all";

        var texts = await _cacheService.GetAsync<IEnumerable<Text>>(
            key,
            async () => (await _repositoryWrapper.TextRepository.GetAllAsync()).ToList(),
            cancellationToken);
            
        if (!texts.Any())
        {
            const string errorMsg = $"Cannot find any text";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        return Result.Ok(_mapper.Map<IEnumerable<TextDto>>(texts));
    }
}