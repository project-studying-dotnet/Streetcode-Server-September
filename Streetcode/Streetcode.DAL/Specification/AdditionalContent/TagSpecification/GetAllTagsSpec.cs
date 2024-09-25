using Ardalis.Specification;
using Streetcode.DAL.Entities.AdditionalContent;

namespace Streetcode.DAL.Specification.AdditionalContent.TagSpecification
{
    public class GetAllTagsSpec : Specification<Tag>
    {
        public GetAllTagsSpec()
        {
            Query
                .Include(t => t.StreetcodeTagIndices)
                .Include(t => t.Streetcodes);
        }
    }
}
