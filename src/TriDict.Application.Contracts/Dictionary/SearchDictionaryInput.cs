using System.ComponentModel.DataAnnotations;

namespace TriDict.Dictionary;

public sealed class SearchDictionaryInput
{
    [Required]
    [StringLength(TriDictConsts.MaxTermLength, MinimumLength = 1)]
    public string Query { get; set; } = string.Empty;

    [StringLength(TriDictConsts.MaxLanguageTagLength)]
    public string? SourceLanguage { get; set; }

    [StringLength(TriDictConsts.MaxCodeLength)]
    public string? DomainCode { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}
