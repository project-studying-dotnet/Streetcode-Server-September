using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Create;

using Text = DAL.Entities.Streetcode.TextContent.Text;

public class CreateTextHandler: IRequestHandler<CreateTextCommand, Result<TextDto>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;

    public CreateTextHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
    }
    
    public async Task<Result<TextDto>> Handle(CreateTextCommand request, CancellationToken cancellationToken)
    {
        var text = _mapper.Map<Text>(request.TextCreateDto) ?? throw new CustomException(
            "Error while mapping TextCreateDto to Text",
            StatusCodes.Status400BadRequest);

        var createdText = await _repositoryWrapper.TextRepository.CreateAsync(text);
        var isResultSuccess = await _repositoryWrapper.SaveChangesAsync() == 1;

        if (!isResultSuccess)
        {
            throw new CustomException("No changes saved", StatusCodes.Status500InternalServerError);
        }

        var createdTextDto = _mapper.Map<TextDto>(createdText) ?? throw new CustomException(
            "Error while mapping Text to TextDto", 
            StatusCodes.Status400BadRequest);

        return Result.Ok(createdTextDto);
    }
}