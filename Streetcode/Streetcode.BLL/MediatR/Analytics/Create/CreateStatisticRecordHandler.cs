using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Analytics;
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
            const string errorMsg = "Cannot convert null to StatisticRecord";
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
        await _repositoryWrapper.SaveChangesAsync();

        return Result.Ok(_mapper.Map<StatisticRecordDto>(statisticRecord));
    }
}
