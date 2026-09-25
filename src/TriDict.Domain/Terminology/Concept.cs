using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TriDict.Terminology;

public sealed class Concept : FullAuditedAggregateRoot<Guid>
{
    private readonly List<Term> _terms = [];
    private readonly List<Definition> _definitions = [];

    public string ConceptCode { get; private set; } = string.Empty;
    public Guid DomainId { get; private set; }
    public DomainCategory Domain { get; private set; } = null!;
    public PublicationStatus Status { get; private set; }
    public ReliabilityCode ReliabilityCode { get; private set; }
    public int CurrentVersion { get; private set; }
    public IReadOnlyCollection<Term> Terms => _terms;
    public IReadOnlyCollection<Definition> Definitions => _definitions;

    private Concept()
    {
    }

    public Concept(Guid id, string conceptCode, Guid domainId)
        : base(id)
    {
        SetConceptCode(conceptCode);
        DomainId = domainId;
        Status = PublicationStatus.Draft;
        ReliabilityCode = ReliabilityCode.Unverified;
        CurrentVersion = 1;
    }

    public Concept(Guid id, string conceptCode, DomainCategory domain)
        : this(id, conceptCode, domain.Id)
    {
        Domain = domain;
    }

    public Concept SetConceptCode(string conceptCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conceptCode);
        ConceptCode = conceptCode.Trim().ToUpperInvariant();
        return this;
    }

    public Term AddTerm(
        Guid termId,
        string languageTag,
        string text,
        TermType termType,
        string partOfSpeech,
        bool isPreferred,
        int senseOrder,
        string? usageContext = null,
        string? region = null)
    {
        var normalizedText = TriDictTextNormalizer.NormalizeForSearch(text);
        if (_terms.Any(x => x.LanguageTag == languageTag && x.NormalizedText == normalizedText))
        {
            throw new BusinessException("TriDict:DuplicateTerm");
        }

        if (isPreferred && _terms.Any(x => x.LanguageTag == languageTag && x.IsPreferred))
        {
            throw new BusinessException("TriDict:DuplicatePreferredTerm");
        }

        var term = new Term(
            termId,
            Id,
            languageTag,
            text,
            normalizedText,
            termType,
            partOfSpeech,
            isPreferred,
            senseOrder,
            usageContext,
            region);
        _terms.Add(term);
        return term;
    }

    public Definition AddDefinition(
        Guid definitionId,
        string languageTag,
        string text,
        string? scenarioLabel = null,
        Guid? sourceId = null)
    {
        if (_definitions.Any(x => x.LanguageTag == languageTag && x.Text == text))
        {
            throw new BusinessException("TriDict:DuplicateDefinition");
        }

        var definition = new Definition(definitionId, Id, languageTag, text, scenarioLabel, sourceId);
        _definitions.Add(definition);
        return definition;
    }

    public void SubmitForReview()
    {
        var requiredLanguages = new[] { "zh-Hans", "es", "en" };
        if (requiredLanguages.Any(language => !_terms.Any(x => x.LanguageTag == language && x.IsPreferred)))
        {
            throw new BusinessException("TriDict:MissingPreferredLanguageTerm");
        }

        if (_definitions.Count == 0)
        {
            throw new BusinessException("TriDict:MissingDefinition");
        }

        Status = PublicationStatus.InReview;
    }

    public void Approve()
    {
        if (Status != PublicationStatus.InReview)
        {
            throw new BusinessException("TriDict:InvalidApprovalState");
        }

        Status = PublicationStatus.Approved;
        ReliabilityCode = ReliabilityCode.ExpertReviewed;
        CurrentVersion++;
    }

    public void Publish()
    {
        if (Status != PublicationStatus.Approved)
        {
            throw new BusinessException("TriDict:InvalidPublishState");
        }

        Status = PublicationStatus.Published;
    }
}
