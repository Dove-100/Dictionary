using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TriDict.Dictionary;

public interface IDictionarySearchAppService : IApplicationService
{
    Task<PagedResultDto<DictionarySearchItemDto>> SearchAsync(
        SearchDictionaryInput input,
        CancellationToken cancellationToken = default);
}
