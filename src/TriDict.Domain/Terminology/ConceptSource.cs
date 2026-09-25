using Volo.Abp.Domain.Entities;

namespace TriDict.Terminology;

public sealed class ConceptSource : Entity<Guid>
{
    public Guid ConceptId { get; private set; }
    public Guid SourceId { get; private set; }
    public EvidenceType EvidenceType { get; private set; }
    public string? Locator { get; private set; }

    private ConceptSource()
    {
    }

    internal ConceptSource(
        Guid id,
        Guid conceptId,
        Guid sourceId,
        EvidenceType evidenceType,
        string? locator)
        : base(id)
    {
        ConceptId = conceptId;
        SourceId = sourceId;
        EvidenceType = evidenceType;
        Locator = locator?.Trim();
    }
}
