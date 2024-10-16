using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Dto.Streetcode.TextContent.Term;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;

using RelatedTerm = DAL.Entities.Streetcode.TextContent.RelatedTerm;

public class CreateRelatedTermHandler : IRequestHandler<CreateRelatedTermCommand, Result<RelatedTermFullDto>>
{
    private readonly IRepositoryWrapper _repository;
    private readonly IMapper _mapper;

    public CreateRelatedTermHandler(IRepositoryWrapper repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<RelatedTermFullDto>> Handle(
        CreateRelatedTermCommand request, 
        CancellationToken cancellationToken)
    {
        var existingRelatedTerm = await _repository.RelatedTermRepository
            .GetFirstOrDefaultAsync(
                predicate: rt => rt.TermId == request.RelatedTerm.TermId
                                 && rt.Word == request.RelatedTerm.Word,
                include: rt => rt.Include(x => x.Term));

        if (existingRelatedTerm != null)
        {
            var existingTermDto = _mapper.Map<RelatedTermFullDto>(existingRelatedTerm) 
                                  ?? throw new CustomException(
                                      "Error while mapping RelatedTerm to RelatedTermFullDto", 
                                      StatusCodes.Status400BadRequest);
            return Result.Ok(existingTermDto);
        }
            
        var relatedTerm = _mapper.Map<RelatedTerm>(request.RelatedTerm) 
                          ?? throw new CustomException(
                              "Error while mapping RelatedTermCreateDto to RelatedTerm", 
                              StatusCodes.Status400BadRequest);

        var createdRelatedTerm = await _repository.RelatedTermRepository.CreateAsync(relatedTerm);
        var isSuccessResult = await _repository.SaveChangesAsync() == 1;

        if(!isSuccessResult)
        {
            throw new CustomException("No changes made", StatusCodes.Status500InternalServerError);
        }

        createdRelatedTerm.Term =
            await _repository.TermRepository.GetFirstOrDefaultAsync(term => term.Id == createdRelatedTerm.TermId)
            ?? throw new CustomException("Related Term not existing", StatusCodes.Status500InternalServerError);

        var createdRelatedTermDto = _mapper.Map<RelatedTermFullDto>(createdRelatedTerm)
                                    ?? throw new CustomException(
                                        "Error while mapping RelatedTerm to RelatedTermFullDto",
                                        StatusCodes.Status400BadRequest);

        return Result.Ok(createdRelatedTermDto);
    }
}