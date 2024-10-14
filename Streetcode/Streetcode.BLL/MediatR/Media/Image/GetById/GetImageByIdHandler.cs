using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Media.Image.GetById;
public class GetImageByIdHandler : IRequestHandler<GetImageByIdQuery, Result<ImageDto>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobAzureService _blobAzureService;

    public GetImageByIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, 
        IBlobAzureService blobAzureService)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobAzureService = blobAzureService;
    }

    public async Task<Result<ImageDto>> Handle(GetImageByIdQuery request, CancellationToken cancellationToken)
    {
        var image = await _repositoryWrapper.ImageRepository.GetFirstOrDefaultAsync(
            f => f.Id == request.Id,
            include: q => q.Include(i => i.ImageDetails) !);

        if (image is null)
        {
            string errorMsg = $"Cannot find a image with corresponding id: {request.Id}";
            throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
        }

        var imageDto = _mapper.Map<ImageDto>(image);
        if(imageDto.BlobName != null)
        {
            imageDto.Base64 = _blobAzureService.FindFileInStorageAsBase64(image.BlobName);
        }

        return Result.Ok(imageDto);
    }
}