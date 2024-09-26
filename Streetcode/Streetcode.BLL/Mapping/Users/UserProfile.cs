using AutoMapper;
using Streetcode.BLL.Dto.Users;
using Streetcode.DAL.Entities.Users;

namespace Streetcode.BLL.Mapping.Users
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserLoginDto>().ReverseMap();
            CreateMap<UserDto, UserLoginDto>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, UserDataDto>().ReverseMap();
            CreateMap<RegisterUserDto, User>()
               .ForMember(dest => dest.Role, opt => opt.Ignore());
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.Ignore());

        }
    }
}
