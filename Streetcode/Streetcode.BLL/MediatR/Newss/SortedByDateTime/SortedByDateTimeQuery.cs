using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.News;

namespace Streetcode.BLL.MediatR.Newss.SortedByDateTime
{
    public record SortedByDateTimeQuery() : IRequest<Result<List<NewsDto>>>;
}
