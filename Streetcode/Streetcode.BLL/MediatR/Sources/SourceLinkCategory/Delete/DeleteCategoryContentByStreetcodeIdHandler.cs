using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Delete
{

    public class DeleteCategoryContentByStreetcodeIdHandler : IRequestHandler<DeleteCategoryContentByStreetcodeIdQuery, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repository;
        private readonly ILoggerService _logger;

        public DeleteCategoryContentByStreetcodeIdHandler(IRepositoryWrapper repository, ILoggerService logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(DeleteCategoryContentByStreetcodeIdQuery request, CancellationToken cancellationToken)
        {
            var srcCategoryContent = await _repository.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(p => p.SourceLinkCategoryId == request.categoryId && 
                 p.StreetcodeId == request.streetcodeId);

            if (srcCategoryContent == null)
            {
                const string errorMsg = "No item with such ids";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            _repository.StreetcodeCategoryContentRepository.Delete(srcCategoryContent);

            var resultIsSuccess = await _repository.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(Unit.Value);
            }
            else
            {
                const string errorMsg = "Failed to delete item";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }
    }
}
