using TriDict.Terminology;
using Volo.Abp;

namespace TriDict.Workflow;

public sealed record ConceptRevisionSnapshot(
    Guid DomainId,
    ReliabilityCode ReliabilityCode,
    IReadOnlyList<TermSnapshot> Terms,
    IReadOnlyList<DefinitionSnapshot> Definitions,
    IReadOnlyList<SourceLinkSnapshot> Sources)
{
    public static ConceptRevisionSnapshot FromConcept(Concept concept) => new(
        concept.DomainId,
        concept.ReliabilityCode,
        concept.Terms.Select(x => new TermSnapshot(
            x.LanguageTag,
            x.Text,
            x.TermType,
            x.PartOfSpeech,
            x.IsPreferred,
            x.SenseOrder,
            x.UsageContext,
            x.Region)).ToList(),
        concept.Definitions.Select(x => new DefinitionSnapshot(
            x.LanguageTag,
            x.Text,
            x.ScenarioLabel,
            x.SourceId)).ToList(),
        concept.Sources.Select(x => new SourceLinkSnapshot(
            x.SourceId,
            x.EvidenceType,
            x.Locator)).ToList());

    public void ValidateForReview()
    {
        var requiredLanguages = new[] { "zh-Hans", "es", "en" };
        if (requiredLanguages.Any(language => !Terms.Any(x => x.LanguageTag == language && x.IsPreferred)))
        {
            throw new BusinessException("TriDict:MissingPreferredLanguageTerm");
        }

        if (Definitions.Count == 0)
        {
            throw new BusinessException("TriDict:MissingDefinition");
        }

        if (Sources.Count == 0)
        {
            throw new BusinessException("TriDict:MissingSource");
        }

        if (Definitions.Any(definition => definition.SourceId.HasValue &&
            !Sources.Any(source => source.SourceId == definition.SourceId.Value)))
        {
            throw new BusinessException("TriDict:DefinitionSourceNotLinked");
        }

        var duplicatePreferred = Terms
            .Where(x => x.IsPreferred)
            .GroupBy(x => x.LanguageTag)
            .Any(group => group.Count() > 1);
        if (duplicatePreferred)
        {
            throw new BusinessException("TriDict:DuplicatePreferredTerm");
        }
    }
}

public sealed record TermSnapshot(
    string LanguageTag,
    string Text,
    TermType TermType,
    string PartOfSpeech,
    bool IsPreferred,
    int SenseOrder,
    string? UsageContext,
    string? Region);

public sealed record DefinitionSnapshot(
    string LanguageTag,
    string Text,
    string? ScenarioLabel,
    Guid? SourceId);

public sealed record SourceLinkSnapshot(
    Guid SourceId,
    EvidenceType EvidenceType,
    string? Locator);
