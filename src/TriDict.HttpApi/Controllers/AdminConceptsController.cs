using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TriDict.Permissions;
using TriDict.Terminology;
using Volo.Abp.AspNetCore.Mvc;

namespace TriDict.Controllers;

[ApiController]
[Authorize(TriDictPermissions.Concepts)]
[Route("api/v1/admin/concepts")]
public sealed class AdminConceptsController(ITerminologyAdminAppService service) : AbpControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConceptAdminDto), StatusCodes.Status200OK)]
    public Task<ConceptAdminDto> GetAsync(Guid id, CancellationToken cancellationToken) =>
        service.GetAsync(id, cancellationToken);

    [HttpPost]
    [Authorize(TriDictPermissions.ConceptsCreate)]
    [ProducesResponseType(typeof(ConceptAdminDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ConceptAdminDto>> CreateAsync([FromBody] CreateConceptInput input, CancellationToken cancellationToken)
    {
        var result = await service.CreateDraftAsync(input, cancellationToken);
        return CreatedAtAction(nameof(GetAsync), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/revisions")]
    [Authorize(TriDictPermissions.ConceptsEdit)]
    public Task<ConceptRevisionDto> StartRevisionAsync(Guid id, [FromBody] StartRevisionInput input, CancellationToken cancellationToken) =>
        service.StartRevisionAsync(id, input, cancellationToken);
}
