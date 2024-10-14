using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Microsoft.EntityFrameworkCore;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;

namespace Streetcode.BLL.MediatR.Newss.SortedByDateTime
{
    public class SortedByDateTimeHandler : IRequestHandler<SortedByDateTimeQuery, Result<List<NewsDto>>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IBlobAzureService _blobAzureService;

        public SortedByDateTimeHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobAzureService blobAzureService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _blobAzureService = blobAzureService;
        }

        public async Task<Result<List<NewsDto>>> Handle(SortedByDateTimeQuery request, CancellationToken cancellationToken)
        {
            var news = await _repositoryWrapper.NewsRepository.GetAllAsync(
                include: cat => cat.Include(img => img.Image));
            if (news == null)
            {
                const string errorMsg = "There are no news in the database";
                throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
            }

            var newsDtos = _mapper.Map<IEnumerable<NewsDto>>(news).OrderByDescending(x => x.CreationDate).ToList();

            newsDtos
                .Where(dto => dto.Image is not null)
                .ToList()
                .ForEach(dto => dto.Image!.Base64 = _blobAzureService
                        .FindFileInStorageAsBase64(dto.Image.BlobName));

            return Result.Ok(newsDtos);
        }
    }
}
