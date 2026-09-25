using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace TriDict;

[DependsOn(typeof(AbpDddDomainModule), typeof(TriDictDomainSharedModule))]
public sealed class TriDictDomainModule : AbpModule;
