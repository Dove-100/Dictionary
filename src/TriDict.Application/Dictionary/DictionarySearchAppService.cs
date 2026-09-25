using TriDict.Terminology;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TriDict.Dictionary;

public sealed class DictionarySearchAppService(
    IConceptSearchRepository conceptSearchRepository)
    : ApplicationService, IDictionarySearchAppService
{
    public async Task<PagedResultDto<DictionarySearchItemDto>> SearchAsync(
        SearchDictionaryInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var normalizedQuery = TriDictTextNormalizer.NormalizeForSearch(input.Query);
        var page = await conceptSearchRepository.SearchAsync(
            normalizedQuery,
            input.SourceLanguage,
            input.DomainCode,
            (input.Page - 1) * input.PageSize,
            input.PageSize,
            cancellationToken);

        var items = page.Items.Select(Map).ToList();
        return new PagedResultDto<DictionarySearchItemDto>(page.TotalCount, items);
    }

    private static DictionarySearchItemDto Map(Concept concept)
    {
        static string? Preferred(Concept source, string language) =>
            source.Terms.FirstOrDefault(x => x.LanguageTag == language && x.IsPreferred)?.Text;

        var sourceTerm = concept.Terms.OrderBy(x => x.SenseOrder).FirstOrDefault();
        return new DictionarySearchItemDto
        {
            ConceptId = concept.Id,
            ConceptCode = concept.ConceptCode,
            DomainCode = concept.Domain.Code,
            SenseOrder = sourceTerm?.SenseOrder ?? 0,
            UsageContext = sourceTerm?.UsageContext,
            PreferredZh = Preferred(concept, "zh-Hans"),
            PreferredEs = Preferred(concept, "es"),
            PreferredEn = Preferred(concept, "en"),
            Definitions = concept.Definitions
                .Select(x => new DictionaryDefinitionDto
                {
                    LanguageTag = x.LanguageTag,
                    Text = x.Text,
                    ScenarioLabel = x.ScenarioLabel,
                })
                .ToList(),
        };
    }
}
