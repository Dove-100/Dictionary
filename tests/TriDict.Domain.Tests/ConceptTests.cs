using TriDict.Terminology;
using Volo.Abp;

namespace TriDict.Domain.Tests;

public sealed class ConceptTests
{
    [Fact]
    public void SubmitForReview_ShouldRequireThreePreferredLanguages()
    {
        var concept = CreateConcept();
        concept.AddTerm(Guid.NewGuid(), "zh-Hans", "模型", TermType.Preferred, "noun", true, 1);
        concept.AddDefinition(Guid.NewGuid(), "zh-Hans", "对对象的抽象表示。", "科研");
        concept.AddSource(Guid.NewGuid(), Guid.NewGuid(), EvidenceType.Definition, "第 12 页");

        var exception = Assert.Throws<BusinessException>(() => concept.SubmitForReview());
        Assert.Equal("TriDict:MissingPreferredLanguageTerm", exception.Code);
    }

    [Fact]
    public void Workflow_ShouldPublishACompleteConcept()
    {
        var concept = CreateConcept();
        concept.AddTerm(Guid.NewGuid(), "zh-Hans", "模型", TermType.Preferred, "noun", true, 1, "科研");
        concept.AddTerm(Guid.NewGuid(), "es", "modelo", TermType.Preferred, "noun", true, 1, "investigación");
        concept.AddTerm(Guid.NewGuid(), "en", "model", TermType.Preferred, "noun", true, 1, "research");
        concept.AddDefinition(Guid.NewGuid(), "zh-Hans", "对对象的抽象表示。", "科研");
        concept.AddSource(Guid.NewGuid(), Guid.NewGuid(), EvidenceType.Definition, "第 12 页");

        concept.SubmitForReview();
        concept.Approve();
        concept.Publish();

        Assert.Equal(PublicationStatus.Published, concept.Status);
        Assert.Equal(ReliabilityCode.ExpertReviewed, concept.ReliabilityCode);
        Assert.Equal(1, concept.CurrentVersion);
    }

    private static Concept CreateConcept() =>
        new(Guid.NewGuid(), "RES-MODEL-001", Guid.NewGuid());
}
