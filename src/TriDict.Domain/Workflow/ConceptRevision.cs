using System.Text.Json;
using TriDict.Terminology;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TriDict.Workflow;

public sealed class ConceptRevision : FullAuditedAggregateRoot<Guid>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Guid ConceptId { get; private set; }
    public int Version { get; private set; }
    public string SnapshotJson { get; private set; } = string.Empty;
    public string ChangeSummary { get; private set; } = string.Empty;
    public PublicationStatus Status { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid? SubmittedBy { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public Guid? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public Guid? PublishedBy { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public string? ReviewComment { get; private set; }
    public string RevisionToken { get; private set; } = string.Empty;

    private ConceptRevision()
    {
    }

    public ConceptRevision(
        Guid id,
        Guid conceptId,
        int version,
        ConceptRevisionSnapshot snapshot,
        string changeSummary,
        Guid createdBy)
        : base(id)
    {
        ConceptId = conceptId;
        Version = version;
        CreatedBy = createdBy;
        Status = PublicationStatus.Draft;
        SetDraft(snapshot, changeSummary);
    }

    public ConceptRevisionSnapshot GetSnapshot() =>
        JsonSerializer.Deserialize<ConceptRevisionSnapshot>(SnapshotJson, JsonOptions)
        ?? throw new BusinessException("TriDict:InvalidRevisionSnapshot");

    public void EnsureToken(string expectedRevisionToken)
    {
        if (RevisionToken != expectedRevisionToken)
        {
            throw new BusinessException("TriDict:RevisionConflict");
        }
    }

    public void UpdateDraft(
        ConceptRevisionSnapshot snapshot,
        string changeSummary,
        string expectedRevisionToken)
    {
        EnsureToken(expectedRevisionToken);

        if (Status is not PublicationStatus.Draft and not PublicationStatus.Rejected)
        {
            throw new BusinessException("TriDict:RevisionNotEditable");
        }

        Status = PublicationStatus.Draft;
        ReviewComment = null;
        SetDraft(snapshot, changeSummary);
    }

    public void Submit(Guid actorId)
    {
        if (Status != PublicationStatus.Draft)
        {
            throw new BusinessException("TriDict:InvalidSubmitState");
        }

        GetSnapshot().ValidateForReview();
        Status = PublicationStatus.InReview;
        SubmittedBy = actorId;
        SubmittedAt = DateTime.UtcNow;
        RefreshToken();
    }

    public void Approve(Guid reviewerId, string? comment)
    {
        if (Status != PublicationStatus.InReview)
        {
            throw new BusinessException("TriDict:InvalidApprovalState");
        }

        if (SubmittedBy == reviewerId)
        {
            throw new BusinessException("TriDict:SelfReviewNotAllowed");
        }

        Status = PublicationStatus.Approved;
        ReviewedBy = reviewerId;
        ReviewedAt = DateTime.UtcNow;
        ReviewComment = comment?.Trim();
        RefreshToken();
    }

    public void Reject(Guid reviewerId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (Status != PublicationStatus.InReview)
        {
            throw new BusinessException("TriDict:InvalidRejectionState");
        }

        if (SubmittedBy == reviewerId)
        {
            throw new BusinessException("TriDict:SelfReviewNotAllowed");
        }

        Status = PublicationStatus.Rejected;
        ReviewedBy = reviewerId;
        ReviewedAt = DateTime.UtcNow;
        ReviewComment = reason.Trim();
        RefreshToken();
    }

    public void MarkPublished(Guid publisherId)
    {
        if (Status != PublicationStatus.Approved)
        {
            throw new BusinessException("TriDict:InvalidPublishState");
        }

        Status = PublicationStatus.Published;
        PublishedBy = publisherId;
        PublishedAt = DateTime.UtcNow;
        RefreshToken();
    }

    private void SetDraft(ConceptRevisionSnapshot snapshot, string changeSummary)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(changeSummary);
        SnapshotJson = JsonSerializer.Serialize(snapshot, JsonOptions);
        ChangeSummary = changeSummary.Trim();
        RefreshToken();
    }

    private void RefreshToken() => RevisionToken = Guid.NewGuid().ToString("N");
}
