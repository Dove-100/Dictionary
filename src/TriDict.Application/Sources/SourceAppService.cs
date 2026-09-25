using Microsoft.AspNetCore.Authorization;
using TriDict.Permissions;
using TriDict.Terminology;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace TriDict.Sources;

public sealed class SourceAppService(
    IRepository<Source, Guid> sourceRepository,
    IGuidGenerator guidGenerator)
    : ApplicationService, ISourceAppService
{
    [Authorize(TriDictPermissions.Sources)]
    public async Task<List<SourceDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var sources = await sourceRepository.GetListAsync(includeDetails: false, cancellationToken);
        return sources.OrderBy(x => x.Title).Select(Map).ToList();
    }

    [Authorize(TriDictPermissions.Sources)]
    public async Task<SourceDto> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Map(await sourceRepository.GetAsync(id, includeDetails: false, cancellationToken));

    [Authorize(TriDictPermissions.SourcesManage)]
    public async Task<SourceDto> CreateAsync(CreateUpdateSourceInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var source = new Source(
            guidGenerator.Create(), input.Title, input.License, input.Author, input.Publisher,
            input.Year, input.Url, input.Identifier, input.AccessedAt);
        await sourceRepository.InsertAsync(source, autoSave: true, cancellationToken);
        return Map(source);
    }

    [Authorize(TriDictPermissions.SourcesManage)]
    public async Task<SourceDto> UpdateAsync(Guid id, CreateUpdateSourceInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var source = await sourceRepository.GetAsync(id, includeDetails: false, cancellationToken);
        source.Update(input.Title, input.License, input.Author, input.Publisher, input.Year,
            input.Url, input.Identifier, input.AccessedAt);
        await sourceRepository.UpdateAsync(source, autoSave: true, cancellationToken);
        return Map(source);
    }

    [Authorize(TriDictPermissions.SourcesManage)]
    public async Task<SourceDto> SetActiveAsync(Guid id, bool active, CancellationToken cancellationToken = default)
    {
        var source = await sourceRepository.GetAsync(id, includeDetails: false, cancellationToken);
        if (active) source.Activate(); else source.Deactivate();
        await sourceRepository.UpdateAsync(source, autoSave: true, cancellationToken);
        return Map(source);
    }

    private static SourceDto Map(Source source) => new()
    {
        Id = source.Id,
        Title = source.Title,
        License = source.License,
        Author = source.Author,
        Publisher = source.Publisher,
        Year = source.Year,
        Url = source.Url,
        Identifier = source.Identifier,
        AccessedAt = source.AccessedAt,
        IsActive = source.IsActive,
    };
}
