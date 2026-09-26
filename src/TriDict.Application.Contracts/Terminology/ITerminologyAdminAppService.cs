using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;

namespace TriDict.Terminology;

public interface ITerminologyAdminAppService : IApplicationService
{
    Task<PagedResultDto<ConceptAdminDto>> GetListAsync(ConceptListInput input, CancellationToken cancellationToken = default);
    Task<List<DomainOptionDto>> GetDomainsAsync(CancellationToken cancellationToken = default);
    Task<ConceptAdminDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConceptAdminDto> CreateDraftAsync(CreateConceptInput input, CancellationToken cancellationToken = default);
    Task<ConceptRevisionDto> UpdateDraftAsync(Guid revisionId, UpdateConceptDraftInput input, CancellationToken cancellationToken = default);
    Task<ConceptRevisionDto> StartRevisionAsync(Guid conceptId, StartRevisionInput input, CancellationToken cancellationToken = default);
    Task<ConceptRevisionDto> SubmitAsync(Guid revisionId, string revisionToken, CancellationToken cancellationToken = default);
    Task<ConceptRevisionDto> ApproveAsync(Guid revisionId, ReviewDecisionInput input, CancellationToken cancellationToken = default);
    Task<ConceptRevisionDto> RejectAsync(Guid revisionId, ReviewDecisionInput input, CancellationToken cancellationToken = default);
    Task<ConceptRevisionDto> PublishAsync(Guid revisionId, ReviewDecisionInput input, CancellationToken cancellationToken = default);
}
