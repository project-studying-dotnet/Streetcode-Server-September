using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.News;

namespace Streetcode.BLL.MediatR.Newss.GetByUrl
{
    public record GetNewsByUrlQuery(string url) : IRequest<Result<NewsDto>>;
}
