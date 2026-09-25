using TriDict.Terminology;
using Volo.Abp.Domain.Entities.Auditing;

namespace TriDict.Importing;

public sealed class ImportJob : FullAuditedAggregateRoot<Guid>
{
    private readonly List<ImportRowError> _errors = [];

    public string FileName { get; private set; } = string.Empty;
    public string TemplateVersion { get; private set; } = string.Empty;
    public ImportStatus Status { get; private set; }
    public int TotalRows { get; private set; }
    public int ValidRows { get; private set; }
    public int InvalidRows { get; private set; }
    public Guid RequestedBy { get; private set; }
    public string? FailureReason { get; private set; }
    public IReadOnlyCollection<ImportRowError> Errors => _errors;

    private ImportJob()
    {
    }

    public ImportJob(Guid id, string fileName, string templateVersion, Guid requestedBy)
        : base(id)
    {
        FileName = fileName;
        TemplateVersion = templateVersion;
        RequestedBy = requestedBy;
        Status = ImportStatus.Pending;
    }

    public void StartValidation()
    {
        Status = ImportStatus.Validating;
        _errors.Clear();
    }

    public void AddError(Guid id, int rowNumber, string field, string code, string message)
    {
        _errors.Add(new ImportRowError(id, Id, rowNumber, field, code, message));
    }

    public void Complete(int totalRows, int validRows)
    {
        TotalRows = totalRows;
        ValidRows = validRows;
        InvalidRows = totalRows - validRows;
        Status = ImportStatus.Completed;
    }

    public void Fail(string reason)
    {
        FailureReason = reason;
        Status = ImportStatus.Failed;
    }
}
