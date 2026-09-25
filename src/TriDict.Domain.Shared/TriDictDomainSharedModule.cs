using Volo.Abp.Modularity;
using Volo.Abp.Validation;

namespace TriDict;

[DependsOn(typeof(AbpValidationModule))]
public sealed class TriDictDomainSharedModule : AbpModule;
