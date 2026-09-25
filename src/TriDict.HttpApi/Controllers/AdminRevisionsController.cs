using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriDict.Permissions;
using TriDict.Terminology;
using Volo.Abp.AspNetCore.Mvc;

namespace TriDict.Controllers;

[ApiController]
[Authorize(TriDictPermissions.Concepts)]
[Route("api/v1/admin/revisions")]
public sealed class AdminRevisionsController(ITerminologyAdminAppService service) : AbpControllerBase
{
    [HttpPut("{id:guid}")]
    [Authorize(TriDictPermissions.ConceptsEdit)]
    public Task<ConceptRevisionDto> UpdateAsync(Guid id, [FromBody] UpdateConceptDraftInput input, CancellationToken cancellationToken) =>
        service.UpdateDraftAsync(id, input, cancellationToken);

    [HttpPost("{id:guid}/submit")]
    [Authorize(TriDictPermissions.ConceptsEdit)]
    public Task<ConceptRevisionDto> SubmitAsync(Guid id, [FromBody] ReviewDecisionInput input, CancellationToken cancellationToken) =>
        service.SubmitAsync(id, input.RevisionToken, cancellationToken);

    [HttpPost("{id:guid}/approve")]
    [Authorize(TriDictPermissions.ConceptsReview)]
    public Task<ConceptRevisionDto> ApproveAsync(Guid id, [FromBody] ReviewDecisionInput input, CancellationToken cancellationToken) =>
        service.ApproveAsync(id, input, cancellationToken);

    [HttpPost("{id:guid}/reject")]
    [Authorize(TriDictPermissions.ConceptsReview)]
    public Task<ConceptRevisionDto> RejectAsync(Guid id, [FromBody] ReviewDecisionInput input, CancellationToken cancellationToken) =>
        service.RejectAsync(id, input, cancellationToken);

    [HttpPost("{id:guid}/publish")]
    [Authorize(TriDictPermissions.ConceptsPublish)]
    public Task<ConceptRevisionDto> PublishAsync(Guid id, [FromBody] ReviewDecisionInput input, CancellationToken cancellationToken) =>
        service.PublishAsync(id, input, cancellationToken);
}
