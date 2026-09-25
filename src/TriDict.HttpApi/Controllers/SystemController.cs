using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace TriDict.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/system")]
public sealed class SystemController : AbpControllerBase
{
    [HttpGet("info")]
    public object GetInfo() => new
    {
        service = "TriDict API",
        apiVersion = "v1",
        framework = ".NET 10 + ABP 10.6",
    };
}
