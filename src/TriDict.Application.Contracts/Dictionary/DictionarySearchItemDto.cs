namespace TriDict.Dictionary;

public sealed class DictionarySearchItemDto
{
    public Guid ConceptId { get; set; }
    public string ConceptCode { get; set; } = string.Empty;
    public string DomainCode { get; set; } = string.Empty;
    public int SenseOrder { get; set; }
    public string? UsageContext { get; set; }
    public string? PreferredZh { get; set; }
    public string? PreferredEs { get; set; }
    public string? PreferredEn { get; set; }
    public IReadOnlyList<DictionaryDefinitionDto> Definitions { get; set; } = [];
}

public sealed class DictionaryDefinitionDto
{
    public string LanguageTag { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? ScenarioLabel { get; set; }
}
