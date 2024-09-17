using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update
{
    public class UpdateStreetcodeCategoryContentHandler : IRequestHandler<UpdateStreetcodeCategoryContentCommand, Result<SourceLinkCategoryContentUpdateDto>>
    {
        private readonly IRepositoryWrapper _repository;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public UpdateStreetcodeCategoryContentHandler(
            IRepositoryWrapper repository,
            IMapper mapper,
            ILoggerService logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<SourceLinkCategoryContentUpdateDto>> Handle(UpdateStreetcodeCategoryContentCommand request, CancellationToken cancellationToken)
        {
            var streetcodeCategoryContentToUpdate = await _repository.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(c => c.StreetcodeId == request.streetcodeId
                                          && c.SourceLinkCategoryId == request.categoryId);

            if (streetcodeCategoryContentToUpdate == null)
            {
                const string errorMsg = "Cannot convert null to category content";
                _logger.LogError(request, errorMsg);    
                return Result.Fail(errorMsg);
            }

            streetcodeCategoryContentToUpdate.Text = request.CategoryContentUpdateDto.Text;
            streetcodeCategoryContentToUpdate.SourceLinkCategoryId = request.CategoryContentUpdateDto.SourceLinkCategoryId;

            _repository.StreetcodeCategoryContentRepository.Update(streetcodeCategoryContentToUpdate);
            var resultIsSuccess = await _repository.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<SourceLinkCategoryContentUpdateDto>(streetcodeCategoryContentToUpdate)); 
            }
            else
            {
                const string errorMsg = "Failed to update category content";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }
    }
}
