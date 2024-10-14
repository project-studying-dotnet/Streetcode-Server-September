using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;

namespace Streetcode.BLL.MediatR.Newss.GetById
{
    public class GetNewsByIdHandler : IRequestHandler<GetNewsByIdQuery, Result<NewsDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IBlobAzureService _blobAzureService;
        public GetNewsByIdHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, IBlobAzureService blobAzureService)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _blobAzureService = blobAzureService;
        }

        public async Task<Result<NewsDto>> Handle(GetNewsByIdQuery request, CancellationToken cancellationToken)
        {
            int id = request.id;
            var newsDto = _mapper.Map<NewsDto>(await _repositoryWrapper.NewsRepository.GetFirstOrDefaultAsync(
                predicate: sc => sc.Id == id,
                include: scl => scl
                    .Include(sc => sc.Image)));

            if(newsDto is null)
            {
                string errorMsg = $"No news by entered Id - {id}";
                throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
            }

            if (newsDto.Image is not null)
            {
                newsDto.Image.Base64 = _blobAzureService.FindFileInStorageAsBase64(newsDto.Image.BlobName);
            }

            return Result.Ok(newsDto);
        }
    }
}