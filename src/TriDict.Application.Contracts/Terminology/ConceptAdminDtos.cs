using System.ComponentModel.DataAnnotations;

namespace TriDict.Terminology;

public sealed class TermInput
{
    [Required, StringLength(TriDictConsts.MaxLanguageTagLength)]
    public string LanguageTag { get; set; } = string.Empty;
    [Required, StringLength(TriDictConsts.MaxTermLength)]
    public string Text { get; set; } = string.Empty;
    public TermType TermType { get; set; } = TermType.Preferred;
    [Required, StringLength(64)]
    public string PartOfSpeech { get; set; } = string.Empty;
    public bool IsPreferred { get; set; }
    public int SenseOrder { get; set; }
    [StringLength(TriDictConsts.MaxContextLength)]
    public string? UsageContext { get; set; }
    [StringLength(TriDictConsts.MaxLanguageTagLength)]
    public string? Region { get; set; }
}

public sealed class DefinitionInput
{
    [Required, StringLength(TriDictConsts.MaxLanguageTagLength)]
    public string LanguageTag { get; set; } = string.Empty;
    [Required, StringLength(TriDictConsts.MaxDefinitionLength)]
    public string Text { get; set; } = string.Empty;
    [StringLength(TriDictConsts.MaxContextLength)]
    public string? ScenarioLabel { get; set; }
    public Guid? SourceId { get; set; }
}

public sealed class SourceLinkInput
{
    public Guid SourceId { get; set; }
    public EvidenceType EvidenceType { get; set; } = EvidenceType.General;
    [StringLength(TriDictConsts.MaxContextLength)]
    public string? Locator { get; set; }
}

public sealed class CreateConceptInput
{
    [Required, StringLength(TriDictConsts.MaxCodeLength)]
    public string ConceptCode { get; set; } = string.Empty;
    public Guid DomainId { get; set; }
    public ReliabilityCode ReliabilityCode { get; set; } = ReliabilityCode.Unverified;
    [Required, StringLength(TriDictConsts.MaxCommentLength)]
    public string ChangeSummary { get; set; } = string.Empty;
    public List<TermInput> Terms { get; set; } = [];
    public List<DefinitionInput> Definitions { get; set; } = [];
    public List<SourceLinkInput> Sources { get; set; } = [];
}

public sealed class UpdateConceptDraftInput
{
    [Required]
    public string RevisionToken { get; set; } = string.Empty;
    public Guid DomainId { get; set; }
    public ReliabilityCode ReliabilityCode { get; set; } = ReliabilityCode.Unverified;
    [Required, StringLength(TriDictConsts.MaxCommentLength)]
    public string ChangeSummary { get; set; } = string.Empty;
    public List<TermInput> Terms { get; set; } = [];
    public List<DefinitionInput> Definitions { get; set; } = [];
    public List<SourceLinkInput> Sources { get; set; } = [];
}

public sealed class StartRevisionInput
{
    [Required, StringLength(TriDictConsts.MaxCommentLength)]
    public string ChangeSummary { get; set; } = string.Empty;
}

public sealed class ReviewDecisionInput
{
    [Required]
    public string RevisionToken { get; set; } = string.Empty;
    [StringLength(TriDictConsts.MaxCommentLength)]
    public string? Comment { get; set; }
}

public sealed class ConceptRevisionDto
{
    public Guid Id { get; set; }
    public Guid ConceptId { get; set; }
    public int Version { get; set; }
    public PublicationStatus Status { get; set; }
    public string ChangeSummary { get; set; } = string.Empty;
    public string RevisionToken { get; set; } = string.Empty;
    public Guid DomainId { get; set; }
    public ReliabilityCode ReliabilityCode { get; set; }
    public List<TermInput> Terms { get; set; } = [];
    public List<DefinitionInput> Definitions { get; set; } = [];
    public List<SourceLinkInput> Sources { get; set; } = [];
    public string? ReviewComment { get; set; }
}

public sealed class ConceptAdminDto
{
    public Guid Id { get; set; }
    public string ConceptCode { get; set; } = string.Empty;
    public Guid DomainId { get; set; }
    public PublicationStatus Status { get; set; }
    public int CurrentVersion { get; set; }
    public ConceptRevisionDto? ActiveRevision { get; set; }
}
