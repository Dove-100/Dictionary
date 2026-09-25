using Volo.Abp.Domain.Entities;

namespace TriDict.Terminology;

public sealed class DomainCategory : AggregateRoot<Guid>
{
    public Guid? ParentId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string NameZh { get; private set; } = string.Empty;
    public string NameEs { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string Path { get; private set; } = string.Empty;
    public int Sort { get; private set; }

    private DomainCategory()
    {
    }

    public DomainCategory(
        Guid id,
        string code,
        string nameZh,
        string nameEs,
        string nameEn,
        string path,
        int sort,
        Guid? parentId = null)
        : base(id)
    {
        Code = code;
        NameZh = nameZh;
        NameEs = nameEs;
        NameEn = nameEn;
        Path = path;
        Sort = sort;
        ParentId = parentId;
    }
}
