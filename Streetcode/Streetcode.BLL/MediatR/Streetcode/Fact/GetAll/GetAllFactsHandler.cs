using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.AdditionalContent.Subtitles;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.GetAll;

public class GetAllFactsHandler : IRequestHandler<GetAllFactsQuery, Result<IEnumerable<FactDto>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;

    public GetAllFactsHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<FactDto>>> Handle(GetAllFactsQuery request, CancellationToken cancellationToken)
    {
        var facts = await _repositoryWrapper.FactRepository.GetAllAsync();

        if (facts is null)
            throw new CustomException($"Cannot find any fact", StatusCodes.Status204NoContent);

        return Result.Ok(_mapper.Map<IEnumerable<FactDto>>(facts));
    }
}