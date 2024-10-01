using AutoMapper;
using Streetcode.Identity.Models.Dto;

namespace Streetcode.Identity.Models.Mapper;

public class UsersProfile : Profile
{
    public UsersProfile()
    {
        CreateMap<ApplicationUser, LoginDto>().ReverseMap();
        //CreateMap<UserDto, UserLoginDto>().ReverseMap();
        //CreateMap<ApplicationUser, UserDto>().ReverseMap();
        CreateMap<ApplicationUser, UserDataDto>().ReverseMap();
    }
}
