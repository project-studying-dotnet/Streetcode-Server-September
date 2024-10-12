using Ardalis.Specification;

namespace Streetcode.DAL.Specification.Media.Image;

using Image = Entities.Media.Images.Image;

public sealed class GetStreetcodeImagesSpec: Specification<Image>
{
    public GetStreetcodeImagesSpec(List<int> imageIds)
    {
        Query.Where(image => imageIds.Contains(image.Id));
    }
}