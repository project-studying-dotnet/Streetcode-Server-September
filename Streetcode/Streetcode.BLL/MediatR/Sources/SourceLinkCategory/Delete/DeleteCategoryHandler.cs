using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Delete
{

    public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryQuery, Result<SourceLinkCategoryDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repository;
        private readonly ILoggerService _logger;

        public DeleteCategoryHandler(IRepositoryWrapper repository, IMapper mapper, ILoggerService logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<SourceLinkCategoryDto>> Handle(DeleteCategoryQuery request, CancellationToken cancellationToken)
        {
            var srcCategory = await _repository.SourceCategoryRepository.GetFirstOrDefaultAsync(p => p.Id == request.id);
           
            if (srcCategory == null)
            {
                const string errorMsg = "No category with such id";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            _repository.SourceCategoryRepository.Delete(srcCategory);
            var resultIsSuccess = await _repository.SaveChangesAsync() > 0;
            var srcCategoryDto = _mapper.Map<SourceLinkCategoryDto>(srcCategory);

            if (resultIsSuccess && srcCategoryDto != null)
            {
                return Result.Ok(srcCategoryDto);
            }
            else
            {
                const string errorMsg = "Failed to delete a related term";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }
    }
}
