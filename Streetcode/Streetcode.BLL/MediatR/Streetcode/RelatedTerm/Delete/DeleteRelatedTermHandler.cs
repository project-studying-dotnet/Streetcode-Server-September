using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete;

public class DeleteRelatedTermHandler : IRequestHandler<DeleteRelatedTermCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repository;

    public DeleteRelatedTermHandler(IRepositoryWrapper repository)
    {
        _repository = repository;
    }

    public async Task<Result<Unit>> Handle(DeleteRelatedTermCommand request, CancellationToken cancellationToken)
    {
        var relatedTerm =
            await _repository.RelatedTermRepository.GetFirstOrDefaultAsync(
                rt => rt.Word.Equals(request.Word)
                && rt.TermId == request.TermId);

        if (relatedTerm is null)
        {
            throw new CustomException(
                $"Cannot find a related term: {request.Word}, termId = {request.TermId}", 
                StatusCodes.Status404NotFound);
        }

        _repository.RelatedTermRepository.Delete(relatedTerm);
        var resultIsSuccess = await _repository.SaveChangesAsync() > 0;
            
        if (!resultIsSuccess)
        {
            throw new CustomException("Failed to delete a related term", StatusCodes.Status500InternalServerError);
        }
            
        return Result.Ok(Unit.Value);
    }
}       