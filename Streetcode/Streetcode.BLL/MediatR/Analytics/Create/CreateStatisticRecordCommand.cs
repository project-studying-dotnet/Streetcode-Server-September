using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Analytics;


namespace Streetcode.BLL.MediatR.Analytics.Create;

public record CreateStatisticRecordCommand(StatisticRecordCreateDto newStreetcodeRecord): IRequest<Result<StatisticRecordDto>>;