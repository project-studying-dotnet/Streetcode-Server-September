namespace Streetcode.DAL.Persistence;

using Microsoft.EntityFrameworkCore;
using Entities.AdditionalContent;
using Entities.AdditionalContent.Coordinates.Types;
using Entities.Analytics;
using Entities.Feedback;
using Entities.Media;
using Entities.Media.Images;
using Entities.News;
using Entities.Partners;
using Entities.Sources;
using Entities.Streetcode;
using Entities.Streetcode.TextContent;
using Entities.Team;
using Entities.Timeline;
using Entities.Toponyms;
using Entities.Transactions;
using Entities.Users;

public class StreetcodeDbContext : DbContext
{
    public StreetcodeDbContext()
    {
    }

    public StreetcodeDbContext(DbContextOptions<StreetcodeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Art>? Arts { get; set; }
    public DbSet<Audio>? Audios { get; set; }
    public DbSet<ToponymCoordinate>? ToponymCoordinates { get; set; }
    public DbSet<StreetcodeCoordinate>? StreetcodeCoordinates { get; set; }
    public DbSet<Fact>? Facts { get; set; }
    public DbSet<HistoricalContext>? HistoricalContexts { get; set; }
    public DbSet<Image>? Images { get; set; }
    public DbSet<ImageDetails>? ImageDetailses { get; set; }
    public DbSet<Partner>? Partners { get; set; }
    public DbSet<PartnerSourceLink>? PartnerSourceLinks { get; set; }
    public DbSet<RelatedFigure>? RelatedFigures { get; set; }
    public DbSet<Response>? Responses { get; set; }
    public DbSet<StreetcodeContent>? Streetcodes { get; set; }
    public DbSet<Subtitle>? Subtitles { get; set; }
    public DbSet<StatisticRecord>? StatisticRecords { get; set; }
    public DbSet<Tag>? Tags { get; set; }
    public DbSet<Term>? Terms { get; set; }
    public DbSet<RelatedTerm>? RelatedTerms { get; set; }
    public DbSet<Text>? Texts { get; set; }
    public DbSet<TimelineItem>? TimelineItems { get; set; }
    public DbSet<Toponym>? Toponyms { get; set; }
    public DbSet<TransactionLink>? TransactionLinks { get; set; }
    public DbSet<Video>? Videos { get; set; }
    public DbSet<StreetcodeCategoryContent>? StreetcodeCategoryContent { get; set; }
    public DbSet<StreetcodeArt>? StreetcodeArts { get; set; }
    public DbSet<User>? Users { get; set; }
    public DbSet<StreetcodeTagIndex>? StreetcodeTagIndices { get; set; }
    public DbSet<TeamMember>? TeamMembers { get; set; }
    public DbSet<TeamMemberLink>? TeamMemberLinks { get; set; }
    public DbSet<Positions>? Positions { get; set; }
    public DbSet<News>? News { get; set; }
    public DbSet<SourceLinkCategory>? SourceLinks { get; set; }
    public DbSet<StreetcodeImage>? StreetcodeImages { get; set; }
    public DbSet<HistoricalContextTimeline>? HistoricalContextsTimelines { get; set; }
    public DbSet<StreetcodePartner>? StreetcodePartners { get; set; }
    public DbSet<TeamMemberPositions>? TeamMemberPosition { get; set; }
    public DbSet<RefreshToken>? RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseCollation("SQL_Ukrainian_CP1251_CI_AS");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StreetcodeDbContext).Assembly);
    }
}