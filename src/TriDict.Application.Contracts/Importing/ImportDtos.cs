using System.ComponentModel.DataAnnotations;
using TriDict.Terminology;
using Volo.Abp.Application.Services;

namespace TriDict.Importing;

public sealed class CreateCsvImportInput
{
    [Required, StringLength(TriDictConsts.MaxFileNameLength)]
    public string FileName { get; set; } = string.Empty;
    [Required]
    public string CsvContent { get; set; } = string.Empty;
}

public sealed class ImportRowErrorDto
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class ImportJobDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string TemplateVersion { get; set; } = string.Empty;
    public ImportStatus Status { get; set; }
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public string? FailureReason { get; set; }
    public List<ImportRowErrorDto> Errors { get; set; } = [];
}

public interface IImportAppService : IApplicationService
{
    Task<ImportJobDto> ImportCsvAsync(CreateCsvImportInput input, CancellationToken cancellationToken = default);
    Task<ImportJobDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
}
