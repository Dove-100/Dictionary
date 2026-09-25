using Volo.Abp.Application.Services;

namespace TriDict.Terminology;

public interface ITerminologyAdminAppService : IApplicationService
{
    Task<ConceptAdminDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConceptAdminDto> CreateDraftAsync(CreateConceptInput input, CancellationToken cancellationToken = default);
    Task<ConceptRevisionDto> UpdateDraftAsync(Guid revisionId, UpdateConceptDraftInput input, CancellationToken cancellationToken = default);
    Task<ConceptRevisionDto> StartRevisionAsync(Guid conceptId, StartRevisionInput input, CancellationToken cancellationToken = default);
    Task<ConceptRevisionDto> SubmitAsync(Guid revisionId, string revisionToken, CancellationToken cancellationToken = default);
    Task<ConceptRevisionDto> ApproveAsync(Guid revisionId, ReviewDecisionInput input, CancellationToken cancellationToken = default);
    Task<ConceptRevisionDto> RejectAsync(Guid revisionId, ReviewDecisionInput input, CancellationToken cancellationToken = default);
    Task<ConceptRevisionDto> PublishAsync(Guid revisionId, ReviewDecisionInput input, CancellationToken cancellationToken = default);
}
