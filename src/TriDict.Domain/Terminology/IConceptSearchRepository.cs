using Volo.Abp.Domain.Repositories;

namespace TriDict.Terminology;

public interface IConceptSearchRepository : IRepository<Concept, Guid>
{
    Task<Concept?> FindWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ConceptSearchPage> SearchAsync(
        string normalizedQuery,
        string? sourceLanguage,
        string? domainCode,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default);
}

public sealed record ConceptSearchPage(IReadOnlyList<Concept> Items, long TotalCount);
