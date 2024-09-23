using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Term.Create;
using Term = DAL.Entities.Streetcode.TextContent.Term;

public class CreateTermHandler: IRequestHandler<CreateTermCommand, Result<int>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;

    public CreateTermHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
    }
    
    public async Task<Result<int>> Handle(CreateTermCommand request, CancellationToken cancellationToken)
    {
        Term term = _mapper.Map<Term>(request.TermCreateDto);

        if (term is null)
        {
            throw new CustomException("Error while mapping TermCreateDto to Term", StatusCodes.Status400BadRequest);
        }

        await _repositoryWrapper.TermRepository.CreateAsync(term);
        bool isCreateSuccessful = await _repositoryWrapper.SaveChangesAsync() == 1;

        if (!isCreateSuccessful)
        {
            throw new CustomException("Error while creating Term", StatusCodes.Status500InternalServerError);
        }

        return Result.Ok(term.Id);
    }
}