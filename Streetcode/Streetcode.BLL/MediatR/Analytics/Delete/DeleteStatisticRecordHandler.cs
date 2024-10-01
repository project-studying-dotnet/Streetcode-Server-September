using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Analytics.Delete;

public class DeleteStatisticRecordHandler : IRequestHandler<DeleteStatisticRecordCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public DeleteStatisticRecordHandler(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<Result<Unit>> Handle(DeleteStatisticRecordCommand request, CancellationToken cancellationToken)
    {
        var id = request.id;

        var statisticRecord = await _repositoryWrapper.StatisticRecordRepository.GetFirstOrDefaultAsync(
            sr => sr.Id == id);

        if (statisticRecord == null)
        {
            string errorMsg = $"Cannot find a Statistic Record by entered Id: {id}";
            throw new CustomException(errorMsg, StatusCodes.Status400BadRequest);
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
            throw new CustomException("Failed to delete a Statistic Record", StatusCodes.Status400BadRequest);
        }
    }
}
