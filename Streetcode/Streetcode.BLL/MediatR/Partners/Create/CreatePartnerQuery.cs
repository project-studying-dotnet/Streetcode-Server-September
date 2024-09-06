using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Partners;

namespace Streetcode.BLL.MediatR.Partners.Create
{
  public record CreatePartnerQuery(CreatePartnerDto newPartner) : IRequest<Result<PartnerDto>>;
}
