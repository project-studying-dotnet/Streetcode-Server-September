using AutoMapper;
using Streetcode.BLL.Dto.News;
using Streetcode.DAL.Entities.News;

namespace Streetcode.BLL.Mapping.Newss
{
    public class NewsProfile : Profile
    {
        public NewsProfile()
        {
            CreateMap<News, NewsDto>().ReverseMap();
        }
    }
}
