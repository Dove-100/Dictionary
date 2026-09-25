namespace TriDict.Terminology;

public enum PublicationStatus
{
    Draft = 0,
    InReview = 1,
    Rejected = 2,
    Approved = 3,
    Published = 4,
    Deprecated = 5
}

public enum ReliabilityCode
{
    Unverified = 0,
    CorpusSupported = 1,
    ExpertReviewed = 2,
    Authoritative = 3
}

public enum TermType
{
    Preferred = 0,
    Admitted = 1,
    Deprecated = 2,
    Abbreviation = 3,
    Symbol = 4,
    Variant = 5
}

public enum ReviewAction
{
    Submitted = 0,
    Approved = 1,
    Rejected = 2,
    Published = 3
}

public enum EvidenceType
{
    Term = 0,
    Definition = 1,
    Example = 2,
    General = 3
}

public enum ImportStatus
{
    Pending = 0,
    Validating = 1,
    Completed = 2,
    Failed = 3
}
