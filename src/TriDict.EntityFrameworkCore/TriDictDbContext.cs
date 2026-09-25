using Microsoft.EntityFrameworkCore;
using TriDict.Importing;
using TriDict.Terminology;
using TriDict.Workflow;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace TriDict.EntityFrameworkCore;

public sealed class TriDictDbContext(DbContextOptions<TriDictDbContext> options)
    : AbpDbContext<TriDictDbContext>(options)
{
    public DbSet<Concept> Concepts => Set<Concept>();
    public DbSet<Term> Terms => Set<Term>();
    public DbSet<Definition> Definitions => Set<Definition>();
    public DbSet<DomainCategory> Domains => Set<DomainCategory>();
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<ConceptSource> ConceptSources => Set<ConceptSource>();
    public DbSet<ConceptRevision> ConceptRevisions => Set<ConceptRevision>();
    public DbSet<ReviewRecord> ReviewRecords => Set<ReviewRecord>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<ImportRowError> ImportRowErrors => Set<ImportRowError>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<DomainCategory>(entity =>
        {
            entity.ConfigureByConvention();
            entity.ToTable(TriDictConsts.DbTablePrefix + "domains", TriDictConsts.DbSchema);
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(TriDictConsts.MaxCodeLength).IsRequired();
            entity.Property(x => x.NameZh).HasMaxLength(128).IsRequired();
            entity.Property(x => x.NameEs).HasMaxLength(128).IsRequired();
            entity.Property(x => x.NameEn).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Path).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Path).IsUnique();
        });

        builder.Entity<Concept>(entity =>
        {
            entity.ConfigureByConvention();
            entity.ToTable(TriDictConsts.DbTablePrefix + "concepts", TriDictConsts.DbSchema);
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ConceptCode).HasMaxLength(TriDictConsts.MaxCodeLength).IsRequired();
            entity.HasIndex(x => x.ConceptCode).IsUnique();
            entity.HasIndex(x => new { x.DomainId, x.Status });
            entity.HasOne(x => x.Domain).WithMany().HasForeignKey(x => x.DomainId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Terms).WithOne().HasForeignKey(x => x.ConceptId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Definitions).WithOne().HasForeignKey(x => x.ConceptId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Sources).WithOne().HasForeignKey(x => x.ConceptId).OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(x => x.Terms).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(x => x.Definitions).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(x => x.Sources).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<Term>(entity =>
        {
            entity.ConfigureByConvention();
            entity.ToTable(TriDictConsts.DbTablePrefix + "terms", TriDictConsts.DbSchema);
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LanguageTag).HasMaxLength(TriDictConsts.MaxLanguageTagLength).IsRequired();
            entity.Property(x => x.Text).HasMaxLength(TriDictConsts.MaxTermLength).IsRequired();
            entity.Property(x => x.NormalizedText).HasMaxLength(TriDictConsts.MaxTermLength).IsRequired();
            entity.Property(x => x.PartOfSpeech).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Region).HasMaxLength(TriDictConsts.MaxLanguageTagLength);
            entity.Property(x => x.UsageContext).HasMaxLength(TriDictConsts.MaxContextLength);
            entity.HasIndex(x => new { x.LanguageTag, x.NormalizedText });
            entity.HasIndex(x => new { x.ConceptId, x.LanguageTag, x.IsPreferred })
                .HasFilter("\"IsPreferred\" = TRUE")
                .IsUnique();
        });

        builder.Entity<Definition>(entity =>
        {
            entity.ConfigureByConvention();
            entity.ToTable(TriDictConsts.DbTablePrefix + "definitions", TriDictConsts.DbSchema);
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LanguageTag).HasMaxLength(TriDictConsts.MaxLanguageTagLength).IsRequired();
            entity.Property(x => x.Text).HasMaxLength(TriDictConsts.MaxDefinitionLength).IsRequired();
            entity.Property(x => x.ScenarioLabel).HasMaxLength(TriDictConsts.MaxContextLength);
            entity.HasIndex(x => new { x.ConceptId, x.LanguageTag });
        });

        builder.Entity<Source>(entity =>
        {
            entity.ConfigureByConvention();
            entity.ToTable(TriDictConsts.DbTablePrefix + "sources", TriDictConsts.DbSchema);
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(TriDictConsts.MaxTitleLength).IsRequired();
            entity.Property(x => x.Author).HasMaxLength(256);
            entity.Property(x => x.Publisher).HasMaxLength(256);
            entity.Property(x => x.Url).HasMaxLength(TriDictConsts.MaxUrlLength);
            entity.Property(x => x.Identifier).HasMaxLength(256);
            entity.Property(x => x.License).HasMaxLength(TriDictConsts.MaxLicenseLength).IsRequired();
            entity.HasIndex(x => x.Identifier);
            entity.HasIndex(x => x.Url);
        });

        builder.Entity<ConceptSource>(entity =>
        {
            entity.ConfigureByConvention();
            entity.ToTable(TriDictConsts.DbTablePrefix + "concept_sources", TriDictConsts.DbSchema);
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Locator).HasMaxLength(TriDictConsts.MaxContextLength);
            entity.HasOne<Source>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.ConceptId, x.SourceId, x.EvidenceType }).IsUnique();
        });

        builder.Entity<ConceptRevision>(entity =>
        {
            entity.ConfigureByConvention();
            entity.ToTable(TriDictConsts.DbTablePrefix + "concept_revisions", TriDictConsts.DbSchema);
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SnapshotJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.ChangeSummary).HasMaxLength(TriDictConsts.MaxCommentLength).IsRequired();
            entity.Property(x => x.ReviewComment).HasMaxLength(TriDictConsts.MaxCommentLength);
            entity.Property(x => x.RevisionToken).HasMaxLength(32).IsRequired();
            entity.HasOne<Concept>().WithMany().HasForeignKey(x => x.ConceptId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ConceptId, x.Version }).IsUnique();
            entity.HasIndex(x => new { x.ConceptId, x.Status });
        });

        builder.Entity<ReviewRecord>(entity =>
        {
            entity.ConfigureByConvention();
            entity.ToTable(TriDictConsts.DbTablePrefix + "review_records", TriDictConsts.DbSchema);
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Comment).HasMaxLength(TriDictConsts.MaxCommentLength);
            entity.HasOne<ConceptRevision>().WithMany().HasForeignKey(x => x.RevisionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.RevisionId, x.OccurredAt });
        });

        builder.Entity<OutboxMessage>(entity =>
        {
            entity.ConfigureByConvention();
            entity.ToTable(TriDictConsts.DbTablePrefix + "outbox_messages", TriDictConsts.DbSchema);
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.ProcessedAt);
        });

        builder.Entity<ImportJob>(entity =>
        {
            entity.ConfigureByConvention();
            entity.ToTable(TriDictConsts.DbTablePrefix + "import_jobs", TriDictConsts.DbSchema);
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FileName).HasMaxLength(TriDictConsts.MaxFileNameLength).IsRequired();
            entity.Property(x => x.TemplateVersion).HasMaxLength(32).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(TriDictConsts.MaxCommentLength);
            entity.HasMany(x => x.Errors).WithOne().HasForeignKey(x => x.ImportJobId).OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(x => x.Errors).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(x => x.Errors).AutoInclude();
            entity.HasIndex(x => new { x.Status, x.CreationTime });
        });

        builder.Entity<ImportRowError>(entity =>
        {
            entity.ConfigureByConvention();
            entity.ToTable(TriDictConsts.DbTablePrefix + "import_row_errors", TriDictConsts.DbSchema);
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Field).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(TriDictConsts.MaxCommentLength).IsRequired();
            entity.HasIndex(x => new { x.ImportJobId, x.RowNumber });
        });
    }
}
