namespace Streetcode.DAL.Persistence.Configurations.Team;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entities.Team;

public class TeamMemberPositionsConfiguration: IEntityTypeConfiguration<TeamMemberPositions>
{
    public void Configure(EntityTypeBuilder<TeamMemberPositions> builder)
    {
        builder.HasKey(nameof(TeamMemberPositions.TeamMemberId), nameof(TeamMemberPositions.PositionsId));
    }
}