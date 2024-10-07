using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.ExistWithUrl;

public class ExistWithUrlHandler: IRequestHandler<ExistWithUrlQuery, Result<bool>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public ExistWithUrlHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ExistWithUrlQuery request, CancellationToken cancellationToken)
    {
        var url = request.TransliterationUrl;
        var streetcode = await _repositoryWrapper.StreetcodeRepository.GetFirstOrDefaultAsync(
            predicate: st => st.TransliterationUrl == url);

        if (streetcode != null)
        {
            return Result.Ok(true);
        }

        string errorMsg = $"No streetcode with url: {url}";
        _logger.LogError(request, errorMsg);
        return Result.Fail(errorMsg);
    }
}