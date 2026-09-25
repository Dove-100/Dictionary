using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace TriDict;

[DependsOn(typeof(AbpAspNetCoreMvcModule), typeof(TriDictApplicationContractsModule))]
public sealed class TriDictHttpApiModule : AbpModule;
