using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;

namespace Streetcode.BLL.MediatR.Newss.GetAll
{
    public class GetAllNewsHandler : IRequestHandler<GetAllNewsQuery, Result<IEnumerable<NewsDto>>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IBlobAzureService _blobAzureService;

        public GetAllNewsHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobAzureService blobAzureService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _blobAzureService = blobAzureService;
        }

        public async Task<Result<IEnumerable<NewsDto>>> Handle(GetAllNewsQuery request, CancellationToken cancellationToken)
        {
            var news = await _repositoryWrapper.NewsRepository.GetAllAsync(
                include: cat => cat.Include(img => img.Image));

            if (news == null)
            {
                const string errorMsg = "There are no news in the database";
                throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
            }

            var newsDtos = _mapper.Map<IEnumerable<NewsDto>>(news);

            newsDtos
                .Where(dto => dto.Image is not null)
                .ToList()
                .ForEach(dto => dto.Image!.Base64 =
                _blobAzureService.FindFileInStorageAsBase64(dto.Image.BlobName));

            return Result.Ok(newsDtos);
        }
    }
}
