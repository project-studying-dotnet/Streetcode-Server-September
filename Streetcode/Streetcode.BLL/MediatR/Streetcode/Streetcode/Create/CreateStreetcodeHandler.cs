using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Extensions;
using Streetcode.BLL.Resources;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Streetcode.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Specification.Media.Image;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;

using StreetcodeTagIndex = DAL.Entities.AdditionalContent.StreetcodeTagIndex;

public class CreateStreetcodeHandler: IRequestHandler<CreateStreetcodeCommand, Result<int>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<ErrorMessages> _stringLocalizer;

    public CreateStreetcodeHandler(
        IRepositoryWrapper repositoryWrapper, 
        IMapper mapper, 
        IStringLocalizer<ErrorMessages> stringLocalizer)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _stringLocalizer = stringLocalizer;
    }

    public async Task<Result<int>> Handle(CreateStreetcodeCommand request, CancellationToken cancellationToken)
    {
        StreetcodeContent streetcode = GetStreetcodeType(request);
        
        streetcode.StreetcodeTagIndices = AddStreetcodeTagIndices(streetcode);
        streetcode.Tags = null!;
        
        streetcode.Images = await AddStreetcodeImages(request.StreetcodeCreateDto);
        
        StreetcodeContent createdStreetcode = await _repositoryWrapper.StreetcodeRepository.CreateAsync(streetcode);

        bool isCreateSuccessful = await _repositoryWrapper.SaveChangesAsync() > 0;
        if (!isCreateSuccessful)
        {
            throw new CustomException(
                _stringLocalizer.GetErrorMessage(ErrorKeys.SaveChangesError), 
                StatusCodes.Status500InternalServerError);
        }
        
        return Result.Ok(createdStreetcode.Id);
    }

    private StreetcodeContent GetStreetcodeType(CreateStreetcodeCommand request) 
        => request.StreetcodeCreateDto.StreetcodeType switch
    {
        DAL.Enums.StreetcodeType.Event => _mapper.Map<EventStreetcode>(request.StreetcodeCreateDto),
        DAL.Enums.StreetcodeType.Person => _mapper.Map<PersonStreetcode>(request.StreetcodeCreateDto),
        _ => throw new CustomException("Streetcode type is unappropriated!", StatusCodes.Status400BadRequest)
    };

    private async Task<List<Image>> AddStreetcodeImages(StreetcodeCreateDto requestStreetcodeCreateDto)
    {
        var imageIds = new List<int> { requestStreetcodeCreateDto.BlackAndWhiteImageDto.Id };

        if (requestStreetcodeCreateDto.HistoryLinksImageDto is not null)
        {
            imageIds.Add(requestStreetcodeCreateDto.HistoryLinksImageDto.Id);
        }

        if (requestStreetcodeCreateDto.GifDto is not null)
        {
            imageIds.Add(requestStreetcodeCreateDto.GifDto.Id);
        }

        List<Image> streetcodeImages = (await _repositoryWrapper.ImageRepository
            .GetItemsBySpecAsync(new GetStreetcodeImagesSpec(imageIds)))
            .ToList();
        
        return streetcodeImages;
    }

    private static List<StreetcodeTagIndex> AddStreetcodeTagIndices(StreetcodeContent streetcode)
    {
        int i = 0;

        return streetcode.Tags
            .Select(tag => new StreetcodeTagIndex { TagId = tag.Id, IsVisible = true, Index = i++ })
            .ToList();
    }
}
