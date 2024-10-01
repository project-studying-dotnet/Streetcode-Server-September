using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Analytics.Delete;

public class DeleteStatisticRecordHandler : IRequestHandler<DeleteStatisticRecordCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public DeleteStatisticRecordHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteStatisticRecordCommand request, CancellationToken cancellationToken)
    {
        var id = request.id;

        var statisticRecord = await _repositoryWrapper.StatisticRecordRepository.GetFirstOrDefaultAsync(
            sr => sr.Id == id);

        if (statisticRecord == null)
        {
            string errorMsg = $"Cannot find  a Statistic Record by entered Id: {id}";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var streetcodeCoordinate = await _repositoryWrapper.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(
            sci => sci.Id == statisticRecord.StreetcodeCoordinateId);

        if (streetcodeCoordinate != null) 
        {
            _repositoryWrapper.StreetcodeCoordinateRepository.Delete(streetcodeCoordinate!);
        }

        _repositoryWrapper.StatisticRecordRepository.Delete(statisticRecord);
        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (resultIsSuccess)
        {
            return Result.Ok(Unit.Value);
        }
        else
        {
            const string errorMsg = $"Failed to delete a Statistic Record";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
