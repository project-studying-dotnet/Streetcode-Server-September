using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Toponyms;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Entities.Toponyms;

namespace Streetcode.BLL.MediatR.Toponyms.GetAll;

public class GetAllToponymsHandler : IRequestHandler<GetAllToponymsQuery,
    Result<GetAllToponymsResponseDto>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;

    public GetAllToponymsHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
    }

    public async Task<Result<GetAllToponymsResponseDto>> Handle(GetAllToponymsQuery query, CancellationToken cancellationToken)
    {
        var filterRequest = query.request;

        var toponyms = _repositoryWrapper.ToponymRepository
             .FindAll();

        if (filterRequest.Title is not null)
        {
            FindStreetcodesWithMatchTitle(ref toponyms, filterRequest.Title);
        }

        var toponymDtos = _mapper.Map<IEnumerable<ToponymDto>>(toponyms.AsEnumerable());

        var response = new GetAllToponymsResponseDto
        {
            Pages = 1,
            Toponyms = toponymDtos
        };

        return Result.Ok(response);
    }

    private static void FindStreetcodesWithMatchTitle(
        ref IQueryable<Toponym> toponyms,
        string title)
    {
        toponyms = toponyms.Where(s => s.StreetName
            .ToLower()
            .Contains(title
            .ToLower()))
            .GroupBy(s => s.StreetName)
            .Select(g => g.First());
    }
}