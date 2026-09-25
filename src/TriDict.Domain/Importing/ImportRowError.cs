using Volo.Abp.Domain.Entities;

namespace TriDict.Importing;

public sealed class ImportRowError : Entity<Guid>
{
    public Guid ImportJobId { get; private set; }
    public int RowNumber { get; private set; }
    public string Field { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;

    private ImportRowError()
    {
    }

    internal ImportRowError(Guid id, Guid importJobId, int rowNumber, string field, string code, string message)
        : base(id)
    {
        ImportJobId = importJobId;
        RowNumber = rowNumber;
        Field = field;
        Code = code;
        Message = message;
    }
}
