using Ardalis.Specification;
using Streetcode.DAL.Entities.AdditionalContent;

namespace Streetcode.DAL.Specification.AdditionalContent.TagSpecification
{
    public class GetTagByTitleSpec : Specification<Tag>, ISingleResultSpecification<Tag>
    {
        public GetTagByTitleSpec(string title)
        {
            Query
                .Where(t => t.Title == title)
                .Include(t => t.StreetcodeTagIndices)
                .Include(t => t.Streetcodes);
        }
    }
}
