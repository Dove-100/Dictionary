using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace TriDict;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(TriDictApplicationContractsModule),
    typeof(TriDictDomainModule))]
public sealed class TriDictApplicationModule : AbpModule;
