using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Partners;

namespace Streetcode.BLL.MediatR.Partners.GetAllPartnerShort
{
    public record GetAllPartnersShortQuery : IRequest<Result<IEnumerable<PartnerShortDto>>>;
}
