using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.DeleteHard
{
    public class DeleteHardStreetcodeHandler : IRequestHandler<DeleteHardStreetcodeCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public DeleteHardStreetcodeHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<Unit>> Handle(DeleteHardStreetcodeCommand request, CancellationToken cancellationToken)
        {
            // Видалення пов'язаних записів
            var relatedFigures = await _repositoryWrapper.RelatedFigureRepository
                .GetAllAsync(rf => rf.ObserverId == request.Id || rf.TargetId == request.Id);

            _repositoryWrapper.RelatedFigureRepository.DeleteRange(relatedFigures);

            // Видалення самого streetcode
            var streetcode = await _repositoryWrapper.StreetcodeRepository
                .GetFirstOrDefaultAsync(s => s.Id == request.Id);

            if (streetcode == null)
            {
                throw new CustomException($"No streetcode found with Id - {request.Id}", StatusCodes.Status204NoContent);
            }

            _repositoryWrapper.StreetcodeRepository.Delete(streetcode);
            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(Unit.Value);
            }
            else
            {
                throw new CustomException("Failed to delete streetcode", StatusCodes.Status400BadRequest);
            }
        }
    }
}
