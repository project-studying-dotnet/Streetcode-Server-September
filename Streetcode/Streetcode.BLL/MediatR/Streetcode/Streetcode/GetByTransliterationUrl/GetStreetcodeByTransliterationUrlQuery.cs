using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetByTransliterationUrl
{
    public record GetStreetcodeByTransliterationUrlQuery(string url) : IRequest<Result<StreetcodeDto>>;
}
