using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TriDict.Dictionary;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace TriDict.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/dictionary")]
public sealed class DictionaryController(IDictionarySearchAppService searchAppService) : AbpControllerBase
{
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResultDto<DictionarySearchItemDto>), StatusCodes.Status200OK)]
    public Task<PagedResultDto<DictionarySearchItemDto>> SearchAsync(
        [FromQuery] SearchDictionaryInput input,
        CancellationToken cancellationToken) =>
        searchAppService.SearchAsync(input, cancellationToken);
}
