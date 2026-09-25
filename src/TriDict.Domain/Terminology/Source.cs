using Volo.Abp.Domain.Entities.Auditing;

namespace TriDict.Terminology;

public sealed class Source : FullAuditedAggregateRoot<Guid>
{
    public string Title { get; private set; } = string.Empty;
    public string? Author { get; private set; }
    public string? Publisher { get; private set; }
    public int? Year { get; private set; }
    public string? Url { get; private set; }
    public string? Identifier { get; private set; }
    public string License { get; private set; } = string.Empty;
    public DateTime? AccessedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Source()
    {
    }

    public Source(
        Guid id,
        string title,
        string license,
        string? author = null,
        string? publisher = null,
        int? year = null,
        string? url = null,
        string? identifier = null,
        DateTime? accessedAt = null)
        : base(id)
    {
        Update(title, license, author, publisher, year, url, identifier, accessedAt);
        IsActive = true;
    }

    public void Update(
        string title,
        string license,
        string? author,
        string? publisher,
        int? year,
        string? url,
        string? identifier,
        DateTime? accessedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(license);
        if (string.IsNullOrWhiteSpace(url) && string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("A source requires a URL or a persistent identifier.");
        }

        Title = title.Trim();
        License = license.Trim();
        Author = author?.Trim();
        Publisher = publisher?.Trim();
        Year = year;
        Url = url?.Trim();
        Identifier = identifier?.Trim();
        AccessedAt = accessedAt;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
