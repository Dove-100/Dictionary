using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using TriDict.Permissions;
using TriDict.Workflow;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace TriDict.Terminology;

public sealed class TerminologyAdminAppService(
    IConceptSearchRepository conceptRepository,
    IRepository<ConceptRevision, Guid> revisionRepository,
    IRepository<DomainCategory, Guid> domainRepository,
    IRepository<Source, Guid> sourceRepository,
    IRepository<ReviewRecord, Guid> reviewRepository,
    IRepository<OutboxMessage, Guid> outboxRepository,
    IGuidGenerator guidGenerator)
    : ApplicationService, ITerminologyAdminAppService
{
    [Authorize(TriDictPermissions.Concepts)]
    public async Task<PagedResultDto<ConceptAdminDto>> GetListAsync(ConceptListInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var concepts = await conceptRepository.GetQueryableAsync();
        var revisions = await revisionRepository.GetQueryableAsync();
        var latest = revisions.Where(revision => !revisions.Any(other =>
            other.ConceptId == revision.ConceptId && other.Version > revision.Version));
        var query = from concept in concepts
                    join revision in latest on concept.Id equals revision.ConceptId
                    select new { concept, revision };
        if (!string.IsNullOrWhiteSpace(input.Query))
        {
            var code = input.Query.Trim().ToUpperInvariant();
            query = query.Where(x => x.concept.ConceptCode.Contains(code));
        }
        if (input.RevisionStatus is { } status)
        {
            query = query.Where(x => x.revision.Status == status);
        }

        var count = await AsyncExecuter.LongCountAsync(query, cancellationToken);
        var page = await AsyncExecuter.ToListAsync(query
            .OrderByDescending(x => x.revision.CreationTime)
            .ThenBy(x => x.concept.ConceptCode)
            .Skip(input.SkipCount)
            .Take(Math.Clamp(input.MaxResultCount, 1, 100)), cancellationToken);
        return new PagedResultDto<ConceptAdminDto>(count,
            page.Select(x => Map(x.concept, x.revision)).ToList());
    }

    [Authorize(TriDictPermissions.Concepts)]
    public async Task<List<DomainOptionDto>> GetDomainsAsync(CancellationToken cancellationToken = default)
    {
        var domains = await domainRepository.GetListAsync(includeDetails: false, cancellationToken);
        return domains.OrderBy(x => x.Sort).ThenBy(x => x.Code)
            .Select(x => new DomainOptionDto { Id = x.Id, Code = x.Code, NameZh = x.NameZh }).ToList();
    }

    [Authorize(TriDictPermissions.Concepts)]
    public async Task<ConceptAdminDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var concept = await RequireConceptAsync(id, cancellationToken);
        var query = await revisionRepository.GetQueryableAsync();
        var revision = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(x => x.ConceptId == id).OrderByDescending(x => x.Version), cancellationToken);
        ConceptRevision? published = null;
        if (concept.CurrentVersion > 0)
        {
            published = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x =>
                x.ConceptId == id && x.Version == concept.CurrentVersion), cancellationToken);
        }
        var result = Map(concept, revision);
        result.PublishedRevision = published is null ? null : Map(published);
        return result;
    }

    [Authorize(TriDictPermissions.ConceptsCreate)]
    public async Task<ConceptAdminDto> CreateDraftAsync(CreateConceptInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var actorId = RequireActor();
        var existing = await conceptRepository.FindAsync(x => x.ConceptCode == input.ConceptCode.Trim().ToUpperInvariant(), cancellationToken: cancellationToken);
        if (existing is not null)
        {
            throw new BusinessException("TriDict:DuplicateConceptCode");
        }

        var snapshot = BuildSnapshot(input.DomainId, input.ReliabilityCode, input.Terms, input.Definitions, input.Sources);
        await ValidateReferencesAsync(snapshot, cancellationToken);
        var concept = new Concept(guidGenerator.Create(), input.ConceptCode, input.DomainId);
        concept.ReplaceDraftContent(snapshot);
        var revision = new ConceptRevision(guidGenerator.Create(), concept.Id, 1, snapshot, input.ChangeSummary, actorId);
        await conceptRepository.InsertAsync(concept, autoSave: false, cancellationToken);
        await revisionRepository.InsertAsync(revision, autoSave: true, cancellationToken);
        return Map(concept, revision);
    }

    [Authorize(TriDictPermissions.ConceptsEdit)]
    public async Task<ConceptRevisionDto> UpdateDraftAsync(Guid revisionId, UpdateConceptDraftInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var revision = await revisionRepository.GetAsync(revisionId, includeDetails: false, cancellationToken);
        var concept = await RequireConceptAsync(revision.ConceptId, cancellationToken);
        var snapshot = BuildSnapshot(input.DomainId, input.ReliabilityCode, input.Terms, input.Definitions, input.Sources);
        await ValidateReferencesAsync(snapshot, cancellationToken);
        revision.UpdateDraft(snapshot, input.ChangeSummary, input.RevisionToken);
        if (concept.CurrentVersion == 0)
        {
            concept.ReplaceDraftContent(snapshot);
            await conceptRepository.UpdateAsync(concept, autoSave: false, cancellationToken);
        }
        await revisionRepository.UpdateAsync(revision, autoSave: true, cancellationToken);
        return Map(revision);
    }

    [Authorize(TriDictPermissions.ConceptsEdit)]
    public async Task<ConceptRevisionDto> StartRevisionAsync(Guid conceptId, StartRevisionInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var concept = await RequireConceptAsync(conceptId, cancellationToken);
        if (concept.Status != PublicationStatus.Published)
        {
            throw new BusinessException("TriDict:ConceptNotPublished");
        }

        var query = await revisionRepository.GetQueryableAsync();
        var hasActive = await AsyncExecuter.AnyAsync(query.Where(x => x.ConceptId == conceptId && x.Status != PublicationStatus.Published), cancellationToken);
        if (hasActive)
        {
            throw new BusinessException("TriDict:ActiveRevisionExists");
        }

        var revision = new ConceptRevision(
            guidGenerator.Create(), concept.Id, concept.CurrentVersion + 1,
            ConceptRevisionSnapshot.FromConcept(concept), input.ChangeSummary, RequireActor());
        await revisionRepository.InsertAsync(revision, autoSave: true, cancellationToken);
        return Map(revision);
    }

    [Authorize(TriDictPermissions.ConceptsEdit)]
    public async Task<ConceptRevisionDto> SubmitAsync(Guid revisionId, string revisionToken, CancellationToken cancellationToken = default)
    {
        var revision = await revisionRepository.GetAsync(revisionId, includeDetails: false, cancellationToken);
        revision.EnsureToken(revisionToken);
        await ValidateReferencesAsync(revision.GetSnapshot(), cancellationToken);
        revision.Submit(RequireActor());
        var concept = await RequireConceptAsync(revision.ConceptId, cancellationToken);
        if (concept.CurrentVersion == 0) concept.SubmitForReview();
        await AddRecordAsync(revision.Id, ReviewAction.Submitted, RequireActor(), null, cancellationToken);
        await conceptRepository.UpdateAsync(concept, autoSave: false, cancellationToken);
        await revisionRepository.UpdateAsync(revision, autoSave: true, cancellationToken);
        return Map(revision);
    }

    [Authorize(TriDictPermissions.ConceptsReview)]
    public async Task<ConceptRevisionDto> ApproveAsync(Guid revisionId, ReviewDecisionInput input, CancellationToken cancellationToken = default)
    {
        var revision = await revisionRepository.GetAsync(revisionId, includeDetails: false, cancellationToken);
        revision.EnsureToken(input.RevisionToken);
        var actorId = RequireActor();
        revision.Approve(actorId, input.Comment);
        var concept = await RequireConceptAsync(revision.ConceptId, cancellationToken);
        if (concept.CurrentVersion == 0) concept.Approve();
        await AddRecordAsync(revision.Id, ReviewAction.Approved, actorId, input.Comment, cancellationToken);
        await conceptRepository.UpdateAsync(concept, autoSave: false, cancellationToken);
        await revisionRepository.UpdateAsync(revision, autoSave: true, cancellationToken);
        return Map(revision);
    }

    [Authorize(TriDictPermissions.ConceptsReview)]
    public async Task<ConceptRevisionDto> RejectAsync(Guid revisionId, ReviewDecisionInput input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input.Comment))
        {
            throw new BusinessException("TriDict:RejectionReasonRequired");
        }
        var revision = await revisionRepository.GetAsync(revisionId, includeDetails: false, cancellationToken);
        revision.EnsureToken(input.RevisionToken);
        var actorId = RequireActor();
        revision.Reject(actorId, input.Comment);
        var concept = await RequireConceptAsync(revision.ConceptId, cancellationToken);
        if (concept.CurrentVersion == 0) concept.Reject();
        await AddRecordAsync(revision.Id, ReviewAction.Rejected, actorId, input.Comment, cancellationToken);
        await conceptRepository.UpdateAsync(concept, autoSave: false, cancellationToken);
        await revisionRepository.UpdateAsync(revision, autoSave: true, cancellationToken);
        return Map(revision);
    }

    [Authorize(TriDictPermissions.ConceptsPublish)]
    public async Task<ConceptRevisionDto> PublishAsync(Guid revisionId, ReviewDecisionInput input, CancellationToken cancellationToken = default)
    {
        var revision = await revisionRepository.GetAsync(revisionId, includeDetails: false, cancellationToken);
        revision.EnsureToken(input.RevisionToken);
        var snapshot = revision.GetSnapshot();
        await ValidateReferencesAsync(snapshot, cancellationToken);
        var actorId = RequireActor();
        var concept = await RequireConceptAsync(revision.ConceptId, cancellationToken);
        concept.ApplyPublishedRevision(snapshot, revision.Version);
        revision.MarkPublished(actorId);
        await AddRecordAsync(revision.Id, ReviewAction.Published, actorId, input.Comment, cancellationToken);
        var payload = JsonSerializer.Serialize(new { ConceptId = concept.Id, RevisionId = revision.Id, revision.Version });
        await outboxRepository.InsertAsync(new OutboxMessage(guidGenerator.Create(), "Terminology.ConceptPublished", concept.Id, payload), autoSave: false, cancellationToken);
        await conceptRepository.UpdateAsync(concept, autoSave: false, cancellationToken);
        await revisionRepository.UpdateAsync(revision, autoSave: true, cancellationToken);
        return Map(revision);
    }

    private async Task<Concept> RequireConceptAsync(Guid id, CancellationToken cancellationToken) =>
        await conceptRepository.FindWithDetailsAsync(id, cancellationToken)
        ?? throw new BusinessException("TriDict:ConceptNotFound");

    private Guid RequireActor() => CurrentUser.Id ?? throw new BusinessException("TriDict:AuthenticatedUserRequired");

    private async Task ValidateReferencesAsync(ConceptRevisionSnapshot snapshot, CancellationToken cancellationToken)
    {
        if (!await domainRepository.AnyAsync(x => x.Id == snapshot.DomainId, cancellationToken: cancellationToken))
        {
            throw new BusinessException("TriDict:DomainNotFound");
        }
        var sourceIds = snapshot.Sources.Select(x => x.SourceId).Distinct().ToList();
        if (sourceIds.Count == 0) return;
        var query = await sourceRepository.GetQueryableAsync();
        var activeCount = await AsyncExecuter.CountAsync(query.Where(x => sourceIds.Contains(x.Id) && x.IsActive), cancellationToken);
        if (activeCount != sourceIds.Count)
        {
            throw new BusinessException("TriDict:InactiveOrMissingSource");
        }
    }

    private async Task AddRecordAsync(Guid revisionId, ReviewAction action, Guid actorId, string? comment, CancellationToken cancellationToken) =>
        await reviewRepository.InsertAsync(new ReviewRecord(guidGenerator.Create(), revisionId, action, actorId, comment), autoSave: false, cancellationToken);

    private static ConceptRevisionSnapshot BuildSnapshot(
        Guid domainId, ReliabilityCode reliabilityCode,
        IEnumerable<TermInput> terms, IEnumerable<DefinitionInput> definitions, IEnumerable<SourceLinkInput> sources)
    {
        var snapshot = new ConceptRevisionSnapshot(
            domainId,
            reliabilityCode,
            terms.Select(x => new TermSnapshot(x.LanguageTag, x.Text, x.TermType, x.PartOfSpeech, x.IsPreferred, x.SenseOrder, x.UsageContext, x.Region)).ToList(),
            definitions.Select(x => new DefinitionSnapshot(x.LanguageTag, x.Text, x.ScenarioLabel, x.SourceId)).ToList(),
            sources.Select(x => new SourceLinkSnapshot(x.SourceId, x.EvidenceType, x.Locator)).ToList());
        var validationConcept = new Concept(Guid.NewGuid(), "VALIDATION", domainId);
        validationConcept.ReplaceDraftContent(snapshot);
        return snapshot;
    }

    private static ConceptAdminDto Map(Concept concept, ConceptRevision? revision) => new()
    {
        Id = concept.Id,
        ConceptCode = concept.ConceptCode,
        DomainId = concept.DomainId,
        Status = concept.Status,
        CurrentVersion = concept.CurrentVersion,
        ActiveRevision = revision is null ? null : Map(revision),
    };

    private static ConceptRevisionDto Map(ConceptRevision revision)
    {
        var snapshot = revision.GetSnapshot();
        return new ConceptRevisionDto
        {
            Id = revision.Id,
            ConceptId = revision.ConceptId,
            Version = revision.Version,
            Status = revision.Status,
            ChangeSummary = revision.ChangeSummary,
            RevisionToken = revision.RevisionToken,
            DomainId = snapshot.DomainId,
            ReliabilityCode = snapshot.ReliabilityCode,
            Terms = snapshot.Terms.Select(x => new TermInput { LanguageTag = x.LanguageTag, Text = x.Text, TermType = x.TermType, PartOfSpeech = x.PartOfSpeech, IsPreferred = x.IsPreferred, SenseOrder = x.SenseOrder, UsageContext = x.UsageContext, Region = x.Region }).ToList(),
            Definitions = snapshot.Definitions.Select(x => new DefinitionInput { LanguageTag = x.LanguageTag, Text = x.Text, ScenarioLabel = x.ScenarioLabel, SourceId = x.SourceId }).ToList(),
            Sources = snapshot.Sources.Select(x => new SourceLinkInput { SourceId = x.SourceId, EvidenceType = x.EvidenceType, Locator = x.Locator }).ToList(),
            ReviewComment = revision.ReviewComment,
        };
    }
}
