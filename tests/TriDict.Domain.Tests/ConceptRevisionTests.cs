using TriDict.Terminology;
using TriDict.Workflow;
using Volo.Abp;

namespace TriDict.Domain.Tests;

public sealed class ConceptRevisionTests
{
    [Fact]
    public void Submit_ShouldRejectSnapshotWithoutSource()
    {
        var snapshot = CompleteSnapshot() with { Sources = [] };
        var revision = new ConceptRevision(Guid.NewGuid(), Guid.NewGuid(), 1, snapshot, "初次录入", Guid.NewGuid());

        var exception = Assert.Throws<BusinessException>(() => revision.Submit(Guid.NewGuid()));

        Assert.Equal("TriDict:MissingSource", exception.Code);
    }

    [Fact]
    public void Approve_ShouldPreventSelfReview()
    {
        var contributor = Guid.NewGuid();
        var revision = new ConceptRevision(Guid.NewGuid(), Guid.NewGuid(), 1, CompleteSnapshot(), "初次录入", contributor);
        revision.Submit(contributor);

        var exception = Assert.Throws<BusinessException>(() => revision.Approve(contributor, "同意"));

        Assert.Equal("TriDict:SelfReviewNotAllowed", exception.Code);
    }

    [Fact]
    public void UpdateDraft_ShouldUseOptimisticRevisionTokenAndKeepSeparatedSenses()
    {
        var snapshot = CompleteSnapshot() with
        {
            Definitions =
            [
                new DefinitionSnapshot("zh-Hans", "金融机构保管现金的设施。", "金融", null),
                new DefinitionSnapshot("zh-Hans", "河流两侧的陆地。", "地理", null),
            ]
        };
        var revision = new ConceptRevision(Guid.NewGuid(), Guid.NewGuid(), 1, snapshot, "多义词义项拆分", Guid.NewGuid());

        var exception = Assert.Throws<BusinessException>(() => revision.UpdateDraft(snapshot, "并发修改", "stale-token"));

        Assert.Equal("TriDict:RevisionConflict", exception.Code);
        Assert.Equal(2, revision.GetSnapshot().Definitions.Count);
        Assert.Contains(revision.GetSnapshot().Definitions, x => x.ScenarioLabel == "金融");
        Assert.Contains(revision.GetSnapshot().Definitions, x => x.ScenarioLabel == "地理");
    }

    private static ConceptRevisionSnapshot CompleteSnapshot() => new(
        Guid.NewGuid(),
        ReliabilityCode.Unverified,
        [
            new TermSnapshot("zh-Hans", "银行", TermType.Preferred, "noun", true, 1, "金融", null),
            new TermSnapshot("es", "banco", TermType.Preferred, "noun", true, 1, "finanzas", null),
            new TermSnapshot("en", "bank", TermType.Preferred, "noun", true, 1, "finance", null),
        ],
        [new DefinitionSnapshot("zh-Hans", "金融机构。", "金融", null)],
        [new SourceLinkSnapshot(Guid.NewGuid(), EvidenceType.Definition, "第 1 页")]);
}
