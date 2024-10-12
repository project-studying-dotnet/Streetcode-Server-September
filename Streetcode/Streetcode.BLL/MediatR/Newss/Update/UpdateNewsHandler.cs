using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Newss.Update;

public class UpdateNewsHandler : IRequestHandler<UpdateNewsCommand, Result<NewsDto>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly IBlobAzureService _blobAzureService;
    public UpdateNewsHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobAzureService blobAzureService)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobAzureService = blobAzureService;
    }

    public async Task<Result<NewsDto>> Handle(UpdateNewsCommand request, CancellationToken cancellationToken)
    {
        var news = _mapper.Map<News>(request.news);
        if (news is null)
        {
            const string errorMsg = $"Cannot convert null to news";
            throw new CustomException(errorMsg, StatusCodes.Status500InternalServerError);
        }

        var response = _mapper.Map<NewsDto>(news);

        if (news.Image is not null)
        {
            response.Image!.Base64 = _blobAzureService.FindFileInStorageAsBase64(response.Image.BlobName);
        }
        else
        {
            var img = await _repositoryWrapper.ImageRepository.GetFirstOrDefaultAsync(x => x.Id == response.ImageId);
            if (img != null)
            {
                _repositoryWrapper.ImageRepository.Delete(img);
            }
        }

        _repositoryWrapper.NewsRepository.Update(news);
        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if(resultIsSuccess)
        {
            return Result.Ok(response);
        }
        else
        {
            const string errorMsg = $"Failed to update news";
            throw new CustomException(errorMsg, StatusCodes.Status500InternalServerError);
        }
    }
}
