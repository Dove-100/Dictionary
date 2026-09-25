using Microsoft.EntityFrameworkCore;
using TriDict.Terminology;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace TriDict.EntityFrameworkCore;

public sealed class EfCoreConceptSearchRepository(
    IDbContextProvider<TriDictDbContext> dbContextProvider)
    : EfCoreRepository<TriDictDbContext, Concept, Guid>(dbContextProvider), IConceptSearchRepository
{
    public async Task<Concept?> FindWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var context = await GetDbContextAsync();
        return await context.Concepts
            .Include(x => x.Domain)
            .Include(x => x.Terms)
            .Include(x => x.Definitions)
            .Include(x => x.Sources)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<ConceptSearchPage> SearchAsync(
        string normalizedQuery,
        string? sourceLanguage,
        string? domainCode,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var context = await GetDbContextAsync();
        var query = context.Concepts
            .AsNoTracking()
            .Include(x => x.Domain)
            .Include(x => x.Terms)
            .Include(x => x.Definitions)
            .Where(x => x.Status == PublicationStatus.Published)
            .Where(x => x.Terms.Any(term =>
                term.NormalizedText.Contains(normalizedQuery) &&
                (sourceLanguage == null || term.LanguageTag == sourceLanguage)));

        if (!string.IsNullOrWhiteSpace(domainCode))
        {
            query = query.Where(x => x.Domain.Code == domainCode);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.Terms.Any(term => term.NormalizedText == normalizedQuery))
            .ThenBy(x => x.Terms.Min(term => term.SenseOrder))
            .ThenBy(x => x.ConceptCode)
            .Skip(skipCount)
            .Take(maxResultCount)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return new ConceptSearchPage(items, totalCount);
    }
}
