using AutoMapper;
using FluentResults;
using MailKit.Search;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MimeKit.Text;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetByFilter
{
    public class GetStreetcodeByFilterHandler : IRequestHandler<GetStreetcodeByFilterQuery, Result<List<StreetcodeFilterResultDto>>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public GetStreetcodeByFilterHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<List<StreetcodeFilterResultDto>>> Handle(GetStreetcodeByFilterQuery request, CancellationToken cancellationToken)
        {
            string searchQuery = request.Filter.SearchQuery;

            var results = new List<StreetcodeFilterResultDto>();

            var streetcodes = await _repositoryWrapper.StreetcodeRepository.GetAllAsync(
                 predicate: x =>
                (x.Status == DAL.Enums.StreetcodeStatus.Published) &&
                (x.Title.Contains(searchQuery) ||
                (x.Alias != null && x.Alias.Contains(searchQuery)) ||
                x.Teaser.Contains(searchQuery)));

            foreach (var streetcode in streetcodes)
            {
                if(IsStreetcodeFound(streetcode, searchQuery))
                {
                    var matchedField = new[] { streetcode.Title, streetcode.Alias, streetcode.Teaser, streetcode.TransliterationUrl }
                            .FirstOrDefault(field => !string.IsNullOrEmpty(field) && field.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));

                    if (matchedField != null)
                    {
                        results.Add(CreateFilterResult(streetcode, matchedField));
                    }
                }
            }

            foreach (var text in await _repositoryWrapper.TextRepository.GetAllAsync(
    include: i => i.Include(x => x.Streetcode!),
    predicate: x => x.Streetcode!.Status == DAL.Enums.StreetcodeStatus.Published))
            {
                if (text.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateFilterResult(text.Streetcode!, text.Title, "Текст", "text"));
                    continue;
                }

                if (!string.IsNullOrEmpty(text.TextContent) && text.TextContent.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateFilterResult(text.Streetcode!, text.TextContent, "Текст", "text"));
                }
            }

            foreach (var fact in await _repositoryWrapper.FactRepository.GetAllAsync(
    include: i => i.Include(x => x.Streetcode!),
    predicate: x => x.Streetcode!.Status == DAL.Enums.StreetcodeStatus.Published))
            {
                if (fact.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) || fact.FactContent.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(CreateFilterResult(fact.Streetcode!, fact.Title, "Wow-факти", "wow-facts"));
                }
            }

            foreach (var timelineItem in await _repositoryWrapper.TimelineRepository.GetAllAsync(
                include: i => i.Include(x => x.Streetcode!),
                predicate: x => x.Streetcode!.Status == DAL.Enums.StreetcodeStatus.Published))
            {
                if (timelineItem.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    || (!string.IsNullOrEmpty(timelineItem.Description) && timelineItem.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)))
                {
                    results.Add(CreateFilterResult(timelineItem.Streetcode!, timelineItem.Title, "Хронологія", "timeline"));
                }
            }

            foreach (var streetcodeArt in await _repositoryWrapper.ArtRepository.GetAllAsync(
            include: i => i.Include(x => x.StreetcodeArts),
            predicate: x => x.StreetcodeArts.Any(art => art.Streetcode != null && art.Streetcode.Status == DAL.Enums.StreetcodeStatus.Published)))
            {
                if (!string.IsNullOrEmpty(streetcodeArt.Description) && streetcodeArt.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                {
                    streetcodeArt.StreetcodeArts.ForEach(art =>
                    {
                        if (art.Streetcode == null)
                        {
                            return;
                        }

                        results.Add(CreateFilterResult(art.Streetcode, streetcodeArt.Description, "Арт-галерея", "art-gallery"));
                    });
                }
            }

            return results;
        }

        private static StreetcodeFilterResultDto CreateFilterResult(StreetcodeContent streetcode, string content, string? sourceName = null, string? blockName = null)
        {
            return new StreetcodeFilterResultDto
            {
                StreetcodeId = streetcode.Id,
                StreetcodeTransliterationUrl = streetcode.TransliterationUrl,
                StreetcodeIndex = streetcode.Index,
                BlockName = blockName,
                Content = content,
                SourceName = sourceName,
            };
        }

        private bool IsStreetcodeFound(StreetcodeContent streetcode, string searchQuery)
        {
            return streetcode.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(streetcode.Alias) &&
                    streetcode.Alias.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    streetcode.Teaser.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    streetcode.TransliterationUrl.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);
        }
    }
}
