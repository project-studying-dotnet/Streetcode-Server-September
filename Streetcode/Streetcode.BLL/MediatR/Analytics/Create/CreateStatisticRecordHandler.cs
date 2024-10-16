using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Analytics;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Analytics.Create;

public class CreateStatisticRecordHandler: IRequestHandler<CreateStatisticRecordCommand, Result<StatisticRecordDto>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;

    public CreateStatisticRecordHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper) 
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
    }

    public async Task<Result<StatisticRecordDto>> Handle(CreateStatisticRecordCommand request, CancellationToken cancellationToken)
    {
        StatisticRecord statisticRecord = _mapper.Map<StatisticRecord>(request.newStreetcodeRecord);

        if (statisticRecord is null)
        {
            const string errorMsg = "Cannot convert null to a Statistic Record";
            throw new CustomException(errorMsg, StatusCodes.Status400BadRequest);
        }

        var qrIds = await _repositoryWrapper.StatisticRecordRepository.GetAllAsync(
            x => x.StreetcodeId == statisticRecord.StreetcodeCoordinate!.StreetcodeId);

        if (qrIds.Any(a => a.QrId == statisticRecord.QrId))
        {
            string errorMsg = $"A QR table number of {statisticRecord.QrId} already exists for this streetcode. " +
                               "Please choose a different number.";
            throw new CustomException(errorMsg, StatusCodes.Status400BadRequest);
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
