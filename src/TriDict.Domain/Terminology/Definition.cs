using Volo.Abp.Domain.Entities;

namespace TriDict.Terminology;

public sealed class Definition : Entity<Guid>
{
    public Guid ConceptId { get; private set; }
    public string LanguageTag { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    public string? ScenarioLabel { get; private set; }
    public Guid? SourceId { get; private set; }

    private Definition()
    {
    }

    internal Definition(
        Guid id,
        Guid conceptId,
        string languageTag,
        string text,
        string? scenarioLabel,
        Guid? sourceId)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(languageTag);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        ConceptId = conceptId;
        LanguageTag = languageTag;
        Text = text.Normalize();
        ScenarioLabel = scenarioLabel;
        SourceId = sourceId;
    }
}
