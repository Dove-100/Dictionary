using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriDict.Permissions;
using TriDict.Sources;
using Volo.Abp.AspNetCore.Mvc;

namespace TriDict.Controllers;

[ApiController]
[Authorize(TriDictPermissions.Sources)]
[Route("api/v1/admin/sources")]
public sealed class SourcesController(ISourceAppService service) : AbpControllerBase
{
    [HttpGet]
    public Task<List<SourceDto>> GetListAsync(CancellationToken cancellationToken) => service.GetListAsync(cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<SourceDto> GetAsync(Guid id, CancellationToken cancellationToken) => service.GetAsync(id, cancellationToken);

    [HttpPost]
    [Authorize(TriDictPermissions.SourcesManage)]
    public Task<SourceDto> CreateAsync([FromBody] CreateUpdateSourceInput input, CancellationToken cancellationToken) =>
        service.CreateAsync(input, cancellationToken);

    [HttpPut("{id:guid}")]
    [Authorize(TriDictPermissions.SourcesManage)]
    public Task<SourceDto> UpdateAsync(Guid id, [FromBody] CreateUpdateSourceInput input, CancellationToken cancellationToken) =>
        service.UpdateAsync(id, input, cancellationToken);

    [HttpPut("{id:guid}/active")]
    [Authorize(TriDictPermissions.SourcesManage)]
    public Task<SourceDto> SetActiveAsync(Guid id, [FromQuery] bool active, CancellationToken cancellationToken) =>
        service.SetActiveAsync(id, active, cancellationToken);
}
