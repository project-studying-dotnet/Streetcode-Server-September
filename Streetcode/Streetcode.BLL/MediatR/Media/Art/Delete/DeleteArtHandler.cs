using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Media.Art.Delete
{
    public class DeleteArtHandler: IRequestHandler<DeleteArtCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public DeleteArtHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger) 
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(DeleteArtCommand request, CancellationToken cancellationToken) 
        {
            var id = request.id;
            var art = await _repositoryWrapper.ArtRepository.GetFirstOrDefaultAsync(art => art.Id == id);

            if (art == null) 
            {
                string errorMsg = $"Cannot find an art by entered Id: {id}";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            if (art.Image != null) 
            {
                _repositoryWrapper.ImageRepository.Delete(art.Image);
            }

            _repositoryWrapper.ArtRepository.Delete(art);

            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(Unit.Value);
            }
            else
            {
                const string errorMsg = $"Failed to delete an art";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }
    }
}
