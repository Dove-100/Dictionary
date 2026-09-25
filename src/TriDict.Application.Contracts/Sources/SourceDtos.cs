using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TriDict.Sources;

public sealed class CreateUpdateSourceInput
{
    [Required, StringLength(TriDictConsts.MaxTitleLength)]
    public string Title { get; set; } = string.Empty;
    [Required, StringLength(TriDictConsts.MaxLicenseLength)]
    public string License { get; set; } = string.Empty;
    [StringLength(256)] public string? Author { get; set; }
    [StringLength(256)] public string? Publisher { get; set; }
    public int? Year { get; set; }
    [StringLength(TriDictConsts.MaxUrlLength)] public string? Url { get; set; }
    [StringLength(256)] public string? Identifier { get; set; }
    public DateTime? AccessedAt { get; set; }
}

public sealed class SourceDto : EntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? Publisher { get; set; }
    public int? Year { get; set; }
    public string? Url { get; set; }
    public string? Identifier { get; set; }
    public DateTime? AccessedAt { get; set; }
    public bool IsActive { get; set; }
}

public interface ISourceAppService : IApplicationService
{
    Task<List<SourceDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<SourceDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SourceDto> CreateAsync(CreateUpdateSourceInput input, CancellationToken cancellationToken = default);
    Task<SourceDto> UpdateAsync(Guid id, CreateUpdateSourceInput input, CancellationToken cancellationToken = default);
    Task<SourceDto> SetActiveAsync(Guid id, bool active, CancellationToken cancellationToken = default);
}
