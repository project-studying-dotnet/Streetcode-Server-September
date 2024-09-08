using AutoMapper;
using Streetcode.BLL.Dto.Transactions;
using Streetcode.DAL.Entities.Transactions;

namespace Streetcode.BLL.Mapping.Transactions;

public class TransactionLinkProfile : Profile
{
    public TransactionLinkProfile()
    {
        CreateMap<TransactionLink, TransactLinkDto>()
           .ReverseMap();
	}
}