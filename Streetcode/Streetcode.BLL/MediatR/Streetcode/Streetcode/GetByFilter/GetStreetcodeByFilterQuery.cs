using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.AdditionalContent.Filter;
using Streetcode.BLL.Dto.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetByFilter
{
    public record class GetStreetcodeByFilterQuery(StreetcodeFilterRequestDto Filter) : IRequest<Result<List<StreetcodeFilterResultDto>>>;
}
