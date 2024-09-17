using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Util;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Delete
{
    public class DeleteFactHandler : IRequestHandler<DeleteFactCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;
        public DeleteFactHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(DeleteFactCommand request, CancellationToken cancellationToken)
        {
            int id = request.id;
            var fact = await _repositoryWrapper.FactRepository.GetFirstOrDefaultAsync(f => f.Id == id);

            if (fact == null)
            {
                string errorMsg = $"No fact found by entered Id - {id}";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            // Getting all the facts related to the same Streetcode
            var facts = await _repositoryWrapper.FactRepository
                .GetAllAsync(f => f.StreetcodeId == fact.StreetcodeId);

            if (facts == null || !facts.Any())
            {
                string errorMsg = $"No facts found for StreetcodeId: {fact.StreetcodeId}";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _repositoryWrapper.FactRepository.Delete(fact);

            // Use the function to update the order of facts
            FactOrderHelper.UpdateFactOrder(facts.ToList(), fact.Id);

            // Saving changes
            _repositoryWrapper.FactRepository.UpdateRange(facts);

            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
            if (resultIsSuccess)
            {
                return Result.Ok(Unit.Value);
            }
            else
            {
                string errorMsg = "Failed to delete fact";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }
    }
}
