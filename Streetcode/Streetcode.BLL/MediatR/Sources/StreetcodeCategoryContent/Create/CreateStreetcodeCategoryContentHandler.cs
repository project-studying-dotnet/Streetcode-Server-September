using MediatR;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;

using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Exceptions.CustomExceptions;
using Interfaces.Logging;
using StreetcodeCategoryContent = DAL.Entities.Sources.StreetcodeCategoryContent;

public class CreateStreetcodeCategoryContentHandler: IRequestHandler<CreateStreetcodeCategoryContentCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;

    public CreateStreetcodeCategoryContentHandler(
        IRepositoryWrapper repositoryWrapper, 
        IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
    }
    
    public async Task<Result<Unit>> Handle(CreateStreetcodeCategoryContentCommand request, CancellationToken cancellationToken)
    {
        var streetcodeCategoryContent = _mapper.Map<StreetcodeCategoryContent>(request.CategoryContentCreateDto);
        
        if (streetcodeCategoryContent is null)
        {
            throw new CustomException("Cannot convert null to StreetcodeContent", StatusCodes.Status204NoContent);
        }

        await _repositoryWrapper.StreetcodeCategoryContentRepository.CreateAsync(streetcodeCategoryContent);
        var isResultSuccessful = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (!isResultSuccessful)
        {
            throw new CustomException("Failed to create SourceLinkCategory", StatusCodes.Status400BadRequest);
        }

        return Result.Ok(Unit.Value);
    }
}