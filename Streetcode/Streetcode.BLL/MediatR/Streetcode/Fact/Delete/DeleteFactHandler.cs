using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Util;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Delete
{
    public class DeleteFactHandler : IRequestHandler<DeleteFactCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public DeleteFactHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<Unit>> Handle(DeleteFactCommand request, CancellationToken cancellationToken)
        {
            int id = request.id;
            var fact = await _repositoryWrapper.FactRepository.GetFirstOrDefaultAsync(f => f.Id == id);

            if (fact == null)
                throw new CustomException($"No fact found by entered Id - {id}", StatusCodes.Status204NoContent);

            // Getting all the facts related to the same Streetcode
            var facts = await _repositoryWrapper.FactRepository
                .GetAllAsync(f => f.StreetcodeId == fact.StreetcodeId);

            if (facts == null || !facts.Any())
                throw new CustomException($"No facts found for StreetcodeId: {fact.StreetcodeId}", StatusCodes.Status204NoContent);

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
                throw new CustomException("Failed to delete fact", StatusCodes.Status400BadRequest);
            }
        }
    }
}
