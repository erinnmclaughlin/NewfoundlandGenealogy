using Microsoft.EntityFrameworkCore;

namespace NewfoundlandGenealogy.CensusData;

public sealed class CensusDbContext(DbContextOptions<CensusDbContext> options) : DbContext(options)
{
    public DbSet<Census> Censuses => Set<Census>();
    public DbSet<CensusDistrict> CensusDistricts => Set<CensusDistrict>();
    //public DbSet<CensusDwellingGroup> CensusDwellingGroups =>  Set<CensusDwellingGroup>();
    //public DbSet<CensusFamilyGroup> CensusFamilyGroups =>  Set<CensusFamilyGroup>();
    public DbSet<CensusTranscription> CensusTranscriptions =>  Set<CensusTranscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Census>(builder =>
        {
            builder.Property(x => x.Id).HasMaxLength(100);
            builder.Property(x => x.DisplayName).HasMaxLength(100);
            builder.Property(x => x.NgbUrl).HasMaxLength(200);
            builder.Property(x => x.Notes).HasMaxLength(int.MaxValue).HasColumnType("text");
        });

        modelBuilder.Entity<CensusDistrict>(builder =>
        {
            builder.Property(x => x.Id).HasMaxLength(100);
            builder.Property(x => x.CensusId).HasMaxLength(100);
            builder.Property(x => x.DisplayName).HasMaxLength(100);
            builder.Property(x => x.NgbUrl).HasMaxLength(200);
            builder.Property(x => x.Notes).HasMaxLength(int.MaxValue).HasColumnType("text");

            builder.HasKey(x => new { x.CensusId, x.Id });
            builder.HasOne<Census>().WithMany(x => x.Districts).HasForeignKey(x => x.CensusId);
        });

        modelBuilder.Entity<CensusTranscription>(builder =>
        {
            builder.Property(x => x.Id).HasMaxLength(100);
            builder.Property(x => x.CensusId).HasMaxLength(100);
            builder.Property(x => x.DistrictId).HasMaxLength(100);
            builder.Property(x => x.DisplayName).HasMaxLength(100);
            builder.Property(x => x.NgbUrl).HasMaxLength(200);
            builder.Property(x => x.MarkdownContent).HasMaxLength(int.MaxValue).HasColumnType("text");
            builder.Property(x => x.Notes).HasMaxLength(int.MaxValue).HasColumnType("text");
            builder.PrimitiveCollection(x => x.ColumnNames).HasDefaultValueSql("'{}'");
            
            builder.HasKey(x => new { x.CensusId, x.DistrictId, x.Id });
            builder.HasOne<CensusDistrict>().WithMany(x => x.Transcriptions).HasForeignKey(x => new { x.CensusId, x.DistrictId });
        });
    }
}