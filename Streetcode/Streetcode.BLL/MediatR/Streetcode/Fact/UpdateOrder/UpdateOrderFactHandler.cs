using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Util;

using FactEntety = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;

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
                //  Get a list of all facts related to the same Streetcode
                var requestedStreetCodeId = request.Fact.StreetcodeId;

                var facts = await _repositoryWrapper.FactRepository
                    .GetAllAsync(f => f.StreetcodeId == requestedStreetCodeId);

                if (facts == null || !facts.Any())
                    throw new CustomException($"No facts found for StreetcodeId: {requestedStreetCodeId}", StatusCodes.Status204NoContent);

                // Use the function to update the order of facts
                FactOrderHelper.UpdateFactOrder(facts.ToList(), request.Fact.FactId, request.Fact.NewOrder);

                // Saving changes
                _repositoryWrapper.FactRepository.UpdateRange(facts);
                await _repositoryWrapper.SaveChangesAsync();

                // Return updated facts
                var factDtos = _mapper.Map<IEnumerable<FactDto>>(facts);
                factDtos = factDtos.OrderBy(f => f.SortOrder);

                return Result.Ok(factDtos);
            }
            catch (Exception ex)
            {
                throw new CustomException($"Error occurred while updating fact order: {ex.Message}", StatusCodes.Status400BadRequest);
            }
        }
    }
}
