using System.Globalization;
using AutoMapper;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Streetcode.Types;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.Mapping.Streetcode;

public class StreetcodeProfile : Profile
{
    public StreetcodeProfile()
    {
        CreateMap<StreetcodeContent, StreetcodeDto>()
            .ForMember(x => x.StreetcodeType, conf => conf.MapFrom(s => GetStreetcodeType(s)))
            .ReverseMap();
        CreateMap<StreetcodeContent, StreetcodeShortDto>().ReverseMap();
        CreateMap<StreetcodeContent, StreetcodeMainPageDto>()
             .ForPath(dto => dto.Text, conf => conf
                .MapFrom(e => e.Text!.Title))
            .ForPath(dto => dto.ImageId, conf => conf
                .MapFrom(e => e.Images.Select(i => i.Id).LastOrDefault()));

        CreateMap<StreetcodeMainBlockCreateDto, StreetcodeCreateDto>()
            .ForMember(s => s.AudioDto, conf => conf.Ignore())
            .ForMember(s => s.GifDto, conf => conf.Ignore())
            .ForMember(s => s.BlackAndWhiteImageDto, conf => conf.Ignore())
            .ForMember(s => s.HistoryLinksImageDto, conf => conf.Ignore());

        CreateMap<StreetcodeCreateDto, StreetcodeContent>()
            .ForMember(dest => dest.DateString, conf => conf.MapFrom(src => CreateDateString(
                src.EventStartOrPersonBirthDate,
                src.EventEndOrPersonDeathDate)))
            .ForMember(dest => dest.AudioId, conf => conf.MapFrom(
                src => src.AudioDto == null ? (int?)null : src.AudioDto.Id))
            .ForMember(dest => dest.Images, conf => conf.Ignore())
            .ForMember(dest => dest.Coordinates, conf => conf.Ignore())
            .ForMember(dest => dest.Audio, conf => conf.Ignore())
            .ForMember(dest => dest.Alias, conf => conf.Ignore())
            .ForMember(dest => dest.Comments, conf => conf.Ignore())
            .ForMember(dest => dest.Facts, conf => conf.Ignore())
            .ForMember(dest => dest.Observers, conf => conf.Ignore())
            .ForMember(dest => dest.Partners, conf => conf.Ignore())
            .ForMember(dest => dest.Status, conf => conf.Ignore())
            .ForMember(dest => dest.Subtitles, conf => conf.Ignore())
            .ForMember(dest => dest.Targets, conf => conf.Ignore())
            .ForMember(dest => dest.Toponyms, conf => conf.Ignore())
            .ForMember(dest => dest.Videos, conf => conf.Ignore())
            .ForMember(dest => dest.StatisticRecords, conf => conf.Ignore())
            .ForMember(dest => dest.StreetcodeArts, conf => conf.Ignore())
            .ForMember(dest => dest.TimelineItems, conf => conf.Ignore())
            .ForMember(dest => dest.TransactionLink, conf => conf.Ignore())
            .ForMember(dest => dest.ViewCount, conf => conf.Ignore())
            .ForMember(dest => dest.CreatedAt, conf => conf.Ignore())
            .ForMember(dest => dest.UpdatedAt, conf => conf.Ignore())
            .ForMember(dest => dest.SourceLinkCategories, conf => conf.Ignore())
            .ForMember(dest => dest.StreetcodeCategoryContents, conf => conf.Ignore())
            .ForMember(dest => dest.StreetcodeTagIndices, conf => conf.Ignore());

        CreateMap<StreetcodeCreateDto, PersonStreetcode>()
            .IncludeBase<StreetcodeCreateDto, StreetcodeContent>();
        
        CreateMap<StreetcodeCreateDto, EventStreetcode>()
            .IncludeBase<StreetcodeCreateDto, StreetcodeContent>();
    }

    private static StreetcodeType GetStreetcodeType(StreetcodeContent streetcode)
    {
        return streetcode is EventStreetcode ? StreetcodeType.Event : StreetcodeType.Person;
    }
    
    private static string CreateDateString(DateTime startDate, DateTime? endDate)
    {
        var culturalInfo = new CultureInfo("uk-UA");
        const string dateFormat = "dd MMMM yyyy";

        string dateString = startDate.ToString(dateFormat, culturalInfo) + " року";
        
        if (endDate is not null)
        {
            dateString += $" - {endDate.Value.ToString(dateFormat, culturalInfo)} року";
        }

        return dateString;
    }
}
