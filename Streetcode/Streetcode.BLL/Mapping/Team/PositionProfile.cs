using AutoMapper;
using Streetcode.BLL.Dto.Team;
using Streetcode.DAL.Entities.Team;

namespace Streetcode.BLL.Mapping.Team
{
    public class PositionProfile : Profile
    {
        public PositionProfile()
        {
            CreateMap<Positions, PositionDto>().ReverseMap();
        }
    }
}
