using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Realizations.Base;
using HistoricalContextEntity = Streetcode.DAL.Entities.Timeline.HistoricalContext;
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
        
            var createdTimeline = await _repository.TimelineRepository.CreateAsync(newTimelineItem);

            bool isSaveSuccessful = await _repository.SaveChangesAsync() > 0;

            if (!isSaveSuccessful)
            {
                const string errorMessage = "Failed to create TimelineItem";
                _logger.LogError(request, errorMessage);
                return Result.Fail(new Error(errorMessage));
            }

            return Result.Ok(_mapper.Map<TimelineItemDto>(createdTimeline));
        }
    }
}
