using MediatR;
using Serilog;
using Streetcode.BLL.Interfaces.Logging;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create;

using AutoMapper;
using FluentResults;
using DAL.Repositories.Interfaces.Base;
using SourceLinkCategory = DAL.Entities.Sources.SourceLinkCategory;

public class CreateSourceLinkCategoryHandler: IRequestHandler<CreateSourceLinkCategoryCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public CreateSourceLinkCategoryHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<Result<Unit>> Handle(CreateSourceLinkCategoryCommand request, CancellationToken cancellationToken)
    {
        SourceLinkCategory sourceLinkCategory = _mapper.Map<SourceLinkCategory>(request.SrcLinkCategoryCreateDto);
        
        if (sourceLinkCategory is null)
        {
            const string errorMessage = "Cannot convert null to SourceLinkCategory";
            _logger.LogError(request, errorMessage);
            return Result.Fail(new Error(errorMessage));
        }

        await _repositoryWrapper.SourceCategoryRepository.CreateAsync(sourceLinkCategory);
        bool isResultSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (!isResultSuccess)
        {
            const string errorMessage = "Failed to create SourceLinkCategory";
            _logger.LogError(request, errorMessage);
            return Result.Fail(new Error(errorMessage));
        }
        
        return Result.Ok(Unit.Value);
    }
}