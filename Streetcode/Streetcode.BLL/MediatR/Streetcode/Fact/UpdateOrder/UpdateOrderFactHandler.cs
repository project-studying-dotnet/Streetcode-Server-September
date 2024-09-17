using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Util;

using FactEntety = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.UpdateOrder
{
    public class UpdateOrderFactHandler : IRequestHandler<UpdateOrderFactCommand, Result<IEnumerable<FactDto>>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;
        public UpdateOrderFactHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<FactDto>>> Handle(UpdateOrderFactCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var requestedStreetCodeId = request.Fact.StreetcodeId;

                var facts = await _repositoryWrapper.FactRepository
                    .GetAllAsync(f => f.StreetcodeId == requestedStreetCodeId);

                if (facts == null || !facts.Any())
                {
                    _logger.LogError(request, $"No facts found for StreetcodeId: {requestedStreetCodeId}");
                    return Result.Fail<IEnumerable<FactDto>>("Facts not found");
                }

                var newOrder = request.Fact.NewOrder;
                var maxOrderInFacts = facts.Select(f => f.SortOrder).Max();

                if (newOrder > maxOrderInFacts) newOrder = maxOrderInFacts;

                if (newOrder < 1) newOrder = 1;

                FactOrderHelper.UpdateFactOrder(facts.ToList(), request.Fact.FactId, newOrder);

                _repositoryWrapper.FactRepository.UpdateRange(facts);
                await _repositoryWrapper.SaveChangesAsync();

                var factDtos = _mapper.Map<IEnumerable<FactDto>>(facts);
                factDtos = factDtos.OrderBy(f => f.SortOrder);

                return Result.Ok(factDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(request, $"Error occurred while updating fact order: {ex.Message}");
                return Result.Fail<IEnumerable<FactDto>>("Error occurred while updating fact order");
            }
        }
    }
}
