using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TriDict.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.Modularity;

namespace TriDict;

[DependsOn(
    typeof(TriDictHttpApiModule),
    typeof(TriDictApplicationModule),
    typeof(TriDictEntityFrameworkCoreModule))]
public sealed class TriDictHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Authentication:Authority"];
                options.Audience = configuration["Authentication:Audience"];
                options.RequireHttpsMetadata = configuration.GetValue("Authentication:RequireHttpsMetadata", true);
            });
        context.Services.AddAuthorization();
        context.Services.AddHealthChecks();
        context.Services.AddOpenApi("v1");
        Configure<AbpExceptionHttpStatusCodeOptions>(options =>
        {
            options.Map("TriDict:RevisionConflict", HttpStatusCode.Conflict);
        });
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy => policy
                .WithOrigins(configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });

        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(TriDictApplicationModule).Assembly);
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseConfiguredEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health", new HealthCheckOptions());
            endpoints.MapOpenApi("/openapi/{documentName}.json");
        });
    }
}
