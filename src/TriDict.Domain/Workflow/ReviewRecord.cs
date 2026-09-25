using TriDict.Terminology;
using Volo.Abp.Domain.Entities;

namespace TriDict.Workflow;

public sealed class ReviewRecord : Entity<Guid>
{
    public Guid RevisionId { get; private set; }
    public ReviewAction Action { get; private set; }
    public string? Comment { get; private set; }
    public Guid ActorId { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private ReviewRecord()
    {
    }

    public ReviewRecord(
        Guid id,
        Guid revisionId,
        ReviewAction action,
        Guid actorId,
        string? comment = null)
        : base(id)
    {
        RevisionId = revisionId;
        Action = action;
        ActorId = actorId;
        Comment = comment?.Trim();
        OccurredAt = DateTime.UtcNow;
    }
}
