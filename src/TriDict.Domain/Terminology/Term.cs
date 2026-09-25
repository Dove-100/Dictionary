using Volo.Abp.Domain.Entities;

namespace TriDict.Terminology;

public sealed class Term : Entity<Guid>
{
    public Guid ConceptId { get; private set; }
    public string LanguageTag { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    public string NormalizedText { get; private set; } = string.Empty;
    public TermType TermType { get; private set; }
    public string PartOfSpeech { get; private set; } = string.Empty;
    public string? Region { get; private set; }
    public bool IsPreferred { get; private set; }
    public int SenseOrder { get; private set; }
    public string? UsageContext { get; private set; }

    private Term()
    {
    }

    internal Term(
        Guid id,
        Guid conceptId,
        string languageTag,
        string text,
        string normalizedText,
        TermType termType,
        string partOfSpeech,
        bool isPreferred,
        int senseOrder,
        string? usageContext,
        string? region)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(languageTag);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(partOfSpeech);

        ConceptId = conceptId;
        LanguageTag = languageTag;
        Text = text.Normalize();
        NormalizedText = normalizedText;
        TermType = termType;
        PartOfSpeech = partOfSpeech;
        IsPreferred = isPreferred;
        SenseOrder = senseOrder;
        UsageContext = usageContext;
        Region = region;
    }
}
