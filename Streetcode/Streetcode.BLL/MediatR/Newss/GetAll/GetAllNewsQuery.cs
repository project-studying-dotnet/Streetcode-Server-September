using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.News;

namespace Streetcode.BLL.MediatR.Newss.GetAll
{
    public record GetAllNewsQuery() : IRequest<Result<IEnumerable<NewsDto>>>;
}
