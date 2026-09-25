using Microsoft.EntityFrameworkCore;
using TriDict.Terminology;
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
            entity.Navigation(x => x.Terms).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(x => x.Definitions).UsePropertyAccessMode(PropertyAccessMode.Field);
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
    }
}
