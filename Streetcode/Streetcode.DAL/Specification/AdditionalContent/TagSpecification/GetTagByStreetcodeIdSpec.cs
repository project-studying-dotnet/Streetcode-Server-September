using Ardalis.Specification;
using Streetcode.DAL.Entities.AdditionalContent;

namespace Streetcode.DAL.Specification.AdditionalContent.TagSpecification
{
    public class GetTagByStreetcodeIdSpec : Specification<StreetcodeTagIndex>
    {
        public GetTagByStreetcodeIdSpec(int streetcodeId)
        {
            Query
                .Where(t => t.StreetcodeId == streetcodeId)
                .Include(t => t.Tag)
                .Include(t => t.Streetcode);
        }
    }
}
