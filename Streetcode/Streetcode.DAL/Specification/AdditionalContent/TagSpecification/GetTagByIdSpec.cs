using Ardalis.Specification;
using Streetcode.DAL.Entities.AdditionalContent;

namespace Streetcode.DAL.Specification.AdditionalContent.TagSpecification
{
    public class GetTagByIdSpec : Specification<Tag>, ISingleResultSpecification<Tag>
    {
        public GetTagByIdSpec(int id)
        {
            Query
                .Where(t => t.Id == id)
                .Include(t => t.StreetcodeTagIndices)
                .Include(t => t.Streetcodes);
        }
    }
}
