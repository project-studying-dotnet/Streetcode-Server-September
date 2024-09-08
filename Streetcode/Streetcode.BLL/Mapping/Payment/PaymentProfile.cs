using AutoMapper;
using Streetcode.BLL.Dto.Payment;
using Streetcode.BLL.Dto.Toponyms;
using Streetcode.DAL.Entities.Payment;
using Streetcode.DAL.Entities.Toponyms;

namespace Streetcode.BLL.Mapping.Payment;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<InvoiceInfo, PaymentResponseDto>().ReverseMap();
    }
}