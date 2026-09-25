using Volo.Abp.Authorization;
using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace TriDict;

[DependsOn(
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule),
    typeof(TriDictDomainSharedModule))]
public sealed class TriDictApplicationContractsModule : AbpModule;
