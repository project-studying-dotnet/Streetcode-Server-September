using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;


namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Create
{
    public class CreateTimelineItemHandler : IRequestHandler<CreateTimelineItemCommand, Result<TimelineItemDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repository;
        private readonly ILoggerService _logger;

        public CreateTimelineItemHandler(IMapper mapper, IRepositoryWrapper repository, ILoggerService logger)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<TimelineItemDto>> Handle(CreateTimelineItemCommand request, CancellationToken cancellationToken)
        {
            var streetcodeExists = await _repository.StreetcodeRepository.GetFirstOrDefaultAsync(s => s.Id == request.timelineItemCreateDto.StreetcodeId);
            if (streetcodeExists == null)
            {
                const string errorMessage = "Streetcode does not exist.";
                _logger.LogError(request, errorMessage);
                return Result.Fail(errorMessage);
            }

            var newTimelineItem = _mapper.Map<TimelineItemEntity>(request.timelineItemCreateDto);
            if (newTimelineItem == null)
            {
                const string errorMessage = "Failed to create TimelineItem";
                _logger.LogError(request, errorMessage);
                return Result.Fail(new Error(errorMessage));
            }

            if (!request.timelineItemCreateDto.HistoricalContexts!.Any())
            {
                await _repository.TimelineRepository.CreateAsync(newTimelineItem);
                await _repository.SaveChangesAsync();
                return Result.Ok(_mapper.Map<TimelineItemDto>(newTimelineItem));
            }

            newTimelineItem.HistoricalContextTimelines.Clear();
            await _repository.TimelineRepository.CreateAsync(newTimelineItem);
            await _repository.SaveChangesAsync();

            var historicalContextIds = request.timelineItemCreateDto.HistoricalContexts!.Select(hc => hc.Id).ToList();

            var historicalContexts = await _repository.HistoricalContextRepository
                .GetAllAsync(hc => historicalContextIds.Contains(hc.Id));                  

            var historicalContextTimelines = historicalContexts.
                Select(hct => new HistoricalContextTimeline
                {
                    HistoricalContextId = hct.Id,
                    HistoricalContext = hct,
                    TimelineId = newTimelineItem.Id,
                    Timeline = newTimelineItem
                }).ToList();

            newTimelineItem.HistoricalContextTimelines.AddRange(historicalContextTimelines);
            await _repository.SaveChangesAsync();
            
            return Result.Ok(_mapper.Map<TimelineItemDto>(newTimelineItem));
        }
    }
}
