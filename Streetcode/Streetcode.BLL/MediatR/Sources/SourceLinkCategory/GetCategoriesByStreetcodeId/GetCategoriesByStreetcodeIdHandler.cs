using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Dto.AdditionalContent.Subtitles;
using Streetcode.BLL.Dto.Sources;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.SourceLink.GetCategoriesByStreetcodeId;

public class GetCategoriesByStreetcodeIdHandler : IRequestHandler<GetCategoriesByStreetcodeIdQuery, Result<IEnumerable<SourceLinkCategoryDto>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobAzureService _blobAzureService;

    public GetCategoriesByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobAzureService blobAzureService)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobAzureService = blobAzureService;
    }

    public async Task<Result<IEnumerable<SourceLinkCategoryDto>>> Handle(GetCategoriesByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var srcCategories = await _repositoryWrapper
            .SourceCategoryRepository
            .GetAllAsync(
                predicate: sc => sc.Streetcodes.Any(s => s.Id == request.StreetcodeId),
                include: scl => scl.Include(sc => sc.Image) !);

        if (srcCategories is null)
        {
            string errorMsg = $"Cant find any source category with the streetcode id {request.StreetcodeId}";
            throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
        }

        var mappedSrcCategories = _mapper.Map<IEnumerable<SourceLinkCategoryDto>>(srcCategories);

        var updatedCategories = mappedSrcCategories.Select(srcCategory =>
        {
            srcCategory.Image!.Base64 = _blobService.FindFileInStorageAsBase64(srcCategory.Image.BlobName);
            return srcCategory;
        });

        return Result.Ok(updatedCategories);
    }
}