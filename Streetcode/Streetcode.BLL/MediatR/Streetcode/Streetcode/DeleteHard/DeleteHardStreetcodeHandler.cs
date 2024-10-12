using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;


namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.DeleteHard
{
    public class DeleteHardStreetcodeHandler:IRequestHandler<DeleteHardStreetcodeCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public DeleteHardStreetcodeHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<Unit>> Handle(DeleteHardStreetcodeCommand request, CancellationToken cancellationToken)
        {
            // Спочатку видаляємо пов'язані записи
            await _repositoryWrapper.RelatedFigureRepository
                .Where(rf => rf.ObserverId == request.Id || rf.TargetId == request.Id)
                .ExecuteDeleteAsync(cancellationToken);

            // Тепер видаляємо сам streetcode
            var streetcode = await _repositoryWrapper.StreetcodeRepository.FindAsync(request.Id);
            if (streetcode == null)
            {
                throw new CustomException($"No streetcode found with Id - {request.Id}", StatusCodes.Status204NoContent);
            }

            _context.Streetcodes.Remove(streetcode);
            var resultIsSuccess = await _context.SaveChangesAsync(cancellationToken) > 0;

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
