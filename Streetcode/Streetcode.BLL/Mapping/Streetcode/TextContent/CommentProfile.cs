using AutoMapper;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.DAL.Entities.Streetcode.TextContent;

namespace Streetcode.BLL.Mapping.Streetcode.TextContent;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<Comment, CommentDto>().ReverseMap();
        CreateMap<Comment, CommentCreateDto>().ReverseMap();
    }
}
