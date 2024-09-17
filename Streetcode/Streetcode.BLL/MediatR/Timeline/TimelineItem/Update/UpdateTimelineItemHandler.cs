using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Update
{
    public class UpdateTimelineItemHandler : IRequestHandler<CreateTimelineItemCommand, Result<TimelineItemDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repository;
        private readonly ILoggerService _logger;

        public UpdateTimelineItemHandler(IMapper mapper, IRepositoryWrapper repository, ILoggerService logger)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<TimelineItemDto>> Handle(CreateTimelineItemCommand request, CancellationToken cancellationToken)
        {
            var existingItem = await _repository.TimelineRepository
                .GetFirstOrDefaultAsync(x => x.Id == request.timelineItemUpdateDto.Id);

            var logMsg = string.Empty;

            if (existingItem == null)
            {
                logMsg = $"TimelineItem with ID {request.timelineItemUpdateDto.Id} not found.";
                _logger.LogError(request, logMsg);
                return Result.Fail(new Error(logMsg));
            }

            _mapper.Map(request.timelineItemUpdateDto, existingItem);

            _repository.TimelineRepository.Update(existingItem);

            var isSaveSuccessful = await _repository.SaveChangesAsync() > 0;

            if (!isSaveSuccessful) 
            {
                logMsg = "An error occurred while updating the TimelineItem";
                _logger.LogError(request, logMsg);
                return Result.Fail(new Error(logMsg));

            }

            var updatedItemDto = _mapper.Map<TimelineItemDto>(existingItem);

            _logger.LogInformation($"TimelineItem with ID {request.timelineItemUpdateDto.Id} updated successfully.");
            return Result.Ok(updatedItemDto);
        }
    }
}
