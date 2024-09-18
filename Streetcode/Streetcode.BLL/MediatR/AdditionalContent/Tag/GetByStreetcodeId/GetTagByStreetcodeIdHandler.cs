using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Dto.AdditionalContent.Subtitles;
using Streetcode.BLL.Dto.AdditionalContent.Tag;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Specification.AdditionalContent.TagSpecification;

namespace Streetcode.BLL.MediatR.AdditionalContent.Tag.GetByStreetcodeId;

public class GetTagByStreetcodeIdHandler : IRequestHandler<GetTagByStreetcodeIdQuery, Result<IEnumerable<StreetcodeTagDto>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetTagByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<StreetcodeTagDto>>> Handle(GetTagByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {

        var spec = new GetTagByStreetcodeIdSpec(request.StreetcodeId);

        var tagIndexed = await _repositoryWrapper.StreetcodeTagIndexRepository.GetItemsBySpecAsync(spec);

        if (tagIndexed is null)
        {
            string errorMsg = $"Cannot find any tag by the streetcode id: {request.StreetcodeId}";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        return Result.Ok(_mapper.Map<IEnumerable<StreetcodeTagDto>>(tagIndexed.OrderBy(ti => ti.Index)));
    }
}
