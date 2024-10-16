using AutoMapper;
using Streetcode.Identity.Models.Dto;

namespace Streetcode.Identity.Models.Mapper;

public class UsersProfile : Profile
{
    public UsersProfile()
    {
        CreateMap<ApplicationUser, LoginDto>().ReverseMap();
        CreateMap<UserDto, RegisterDto>().ReverseMap();
        CreateMap<ApplicationUser, RegisterDto>().ReverseMap();
        CreateMap<ApplicationUser, UserDto>().ReverseMap();
        CreateMap<ApplicationUser, UserDataDto>().ReverseMap();
    }
}
