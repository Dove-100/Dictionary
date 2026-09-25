using Volo.Abp.Domain.Entities;

namespace TriDict.Workflow;

public sealed class OutboxMessage : Entity<Guid>
{
    public string EventType { get; private set; } = string.Empty;
    public Guid AggregateId { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private OutboxMessage()
    {
    }

    public OutboxMessage(Guid id, string eventType, Guid aggregateId, string payload)
        : base(id)
    {
        EventType = eventType;
        AggregateId = aggregateId;
        Payload = payload;
        OccurredAt = DateTime.UtcNow;
    }

    public void MarkProcessed() => ProcessedAt = DateTime.UtcNow;
}
