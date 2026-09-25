using NSubstitute;
using TriDict.Dictionary;
using TriDict.Terminology;

namespace TriDict.Application.Tests;

public sealed class DictionarySearchAppServiceTests
{
    [Fact]
    public async Task SearchAsync_ShouldReturnEachSenseAsAnIndependentConcept()
    {
        var domain = new DomainCategory(Guid.NewGuid(), "RES", "科研教育", "investigación", "research", "/RES", 1);
        var first = CreatePublishedConcept(domain, "POLY-ES-MODELO-01", "模型", "modelo", "model", 1, "科研/计算建模");
        var second = CreatePublishedConcept(domain, "POLY-ES-MODELO-02", "型号", "modelo", "product model", 2, "工业产品");
        var repository = Substitute.For<IConceptSearchRepository>();
        repository.SearchAsync("modelo", "es", null, 0, 20, Arg.Any<CancellationToken>())
            .Returns(new ConceptSearchPage([first, second], 2));
        var service = new DictionarySearchAppService(repository);

        var result = await service.SearchAsync(new SearchDictionaryInput
        {
            Query = " Modelo ",
            SourceLanguage = "es",
        });

        Assert.Equal(2, result.TotalCount);
        Assert.Collection(
            result.Items,
            item => Assert.Equal("科研/计算建模", item.UsageContext),
            item => Assert.Equal("工业产品", item.UsageContext));
    }

    private static Concept CreatePublishedConcept(
        DomainCategory domain,
        string code,
        string zh,
        string es,
        string en,
        int senseOrder,
        string usageContext)
    {
        var concept = new Concept(Guid.NewGuid(), code, domain);
        concept.AddTerm(Guid.NewGuid(), "zh-Hans", zh, TermType.Preferred, "noun", true, senseOrder, usageContext);
        concept.AddTerm(Guid.NewGuid(), "es", es, TermType.Preferred, "noun", true, senseOrder, usageContext);
        concept.AddTerm(Guid.NewGuid(), "en", en, TermType.Preferred, "noun", true, senseOrder, usageContext);
        concept.AddDefinition(Guid.NewGuid(), "zh-Hans", $"{usageContext}定义", usageContext);
        concept.AddSource(Guid.NewGuid(), Guid.NewGuid(), EvidenceType.Definition, "test fixture");
        concept.SubmitForReview();
        concept.Approve();
        concept.Publish();
        return concept;
    }
}
