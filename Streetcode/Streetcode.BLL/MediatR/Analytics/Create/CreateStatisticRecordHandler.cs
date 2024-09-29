using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Analytics;
using Streetcode.BLL.Dto.Media.Art;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Analytics.Create;

public class CreateStatisticRecordHandler: IRequestHandler<CreateStatisticRecordCommand, Result<StatisticRecordDto>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public CreateStatisticRecordHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger) 
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<StatisticRecordDto>> Handle(CreateStatisticRecordCommand request, CancellationToken cancellationToken)
    {
        StatisticRecord statisticRecord = _mapper.Map<StatisticRecord>(request.newStreetcodeRecord);

        if (statisticRecord is null)
        {
            const string errorMsg = "Cannot convert null to a Statistic Record";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var qrIds = await _repositoryWrapper.StatisticRecordRepository.GetAllAsync(
            x => x.StreetcodeId == statisticRecord.StreetcodeCoordinate!.StreetcodeId);

        if (qrIds.Any(a => a.QrId == statisticRecord.QrId))
        {
            string errorMsg = $"A QR table number of {statisticRecord.QrId} already exists for this streetcode. " +
                               "Please choose a different number.";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        await _repositoryWrapper.StatisticRecordRepository.CreateAsync(statisticRecord);
        bool resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (resultIsSuccess)
        {
            return Result.Ok(_mapper.Map<StatisticRecordDto>(statisticRecord));
        }
        else
        {
            throw new CustomException("Failed to save a Statistic Record", StatusCodes.Status400BadRequest);
        }
    }
}
