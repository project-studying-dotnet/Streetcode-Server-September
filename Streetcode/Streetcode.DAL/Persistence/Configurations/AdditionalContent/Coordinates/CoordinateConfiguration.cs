namespace Streetcode.DAL.Persistence.Configurations.AdditionalContent.Coordinates;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entities.AdditionalContent.Coordinates;
using Entities.AdditionalContent.Coordinates.Types;

public class CoordinateConfiguration: IEntityTypeConfiguration<Coordinate>
{
    public void Configure(EntityTypeBuilder<Coordinate> builder)
    {
        builder.HasDiscriminator<string>("CoordinateType")
            .HasValue<Coordinate>("coordinate_base")
            .HasValue<StreetcodeCoordinate>("coordinate_streetcode")
            .HasValue<ToponymCoordinate>("coordinate_toponym");
    }
}