using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Dto.Sources;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetAll
{
    public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesQuery, Result<IEnumerable<SourceLinkCategoryDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IBlobAzureService _blobAzureService;
        public GetAllCategoriesHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobAzureService blobAzureService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _blobAzureService = blobAzureService;
        }

        public async Task<Result<IEnumerable<SourceLinkCategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var allCategories = await _repositoryWrapper.SourceCategoryRepository.GetAllAsync(
                include: cat => cat.Include(img => img.Image) !);
            if (allCategories == null)
            {
                const string errorMsg = $"Categories is null";
                throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
            }

            var dtos = _mapper.Map<IEnumerable<SourceLinkCategoryDto>>(allCategories);

            var updatedDtos = dtos.Select(dto =>
            {
                dto.Image.Base64 = _blobAzureService.FindFileInStorageAsBase64(dto.Image.BlobName);
            }
=======
                dto.Image!.Base64 = _blobService.FindFileInStorageAsBase64(dto.Image.BlobName);
                return dto;
            });
>>>>>>> dev

            return Result.Ok(updatedDtos);
        }
    }
}
