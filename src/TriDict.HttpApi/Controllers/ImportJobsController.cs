using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TriDict.Importing;
using TriDict.Permissions;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace TriDict.Controllers;

[ApiController]
[Authorize(TriDictPermissions.Imports)]
[Route("api/v1/admin/import-jobs")]
public sealed class ImportJobsController(IImportAppService service) : AbpControllerBase
{
    [HttpGet]
    [Authorize(TriDictPermissions.ImportsView)]
    public Task<PagedResultDto<ImportJobDto>> GetListAsync([FromQuery] PagedResultRequestDto input, CancellationToken cancellationToken) =>
        service.GetListAsync(input, cancellationToken);

    [HttpPost("csv")]
    [Authorize(TriDictPermissions.ImportsExecute)]
    [RequestSizeLimit(ImportAppServiceLimits.MaxRequestBytes)]
    public async Task<ImportJobDto> ImportCsvAsync([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (!string.Equals(Path.GetExtension(file.FileName), ".csv", StringComparison.OrdinalIgnoreCase))
            throw new BadHttpRequestException("当前端点仅接受 CSV 文件。");
        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, new UTF8Encoding(false, true), detectEncodingFromByteOrderMarks: true);
        var content = await reader.ReadToEndAsync(cancellationToken);
        return await service.ImportCsvAsync(new CreateCsvImportInput { FileName = file.FileName, CsvContent = content }, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [Authorize(TriDictPermissions.ImportsView)]
    public Task<ImportJobDto> GetAsync(Guid id, CancellationToken cancellationToken) => service.GetAsync(id, cancellationToken);
}

internal static class ImportAppServiceLimits
{
    public const long MaxRequestBytes = 2 * 1024 * 1024 + 64 * 1024;
}
