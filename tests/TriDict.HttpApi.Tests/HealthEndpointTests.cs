using Microsoft.AspNetCore.Mvc.Testing;

namespace TriDict.HttpApi.Tests;

public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(_ => { }).CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnSuccess()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SystemInfoEndpoint_ShouldReturnApiVersion()
    {
        var payload = await _client.GetStringAsync("/api/v1/system/info");
        Assert.Contains("v1", payload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OpenApiEndpoint_ShouldExposeDictionarySearch()
    {
        var payload = await _client.GetStringAsync("/openapi/v1.json");
        Assert.Contains("/api/v1/dictionary/search", payload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OpenApiEndpoint_ShouldExposeStageTwoAdminEndpoints()
    {
        var payload = await _client.GetStringAsync("/openapi/v1.json");
        Assert.Contains("/api/v1/admin/concepts", payload, StringComparison.Ordinal);
        Assert.Contains("/api/v1/admin/revisions/{id}/approve", payload, StringComparison.Ordinal);
        Assert.Contains("/api/v1/admin/sources", payload, StringComparison.Ordinal);
        Assert.Contains("/api/v1/admin/import-jobs/csv", payload, StringComparison.Ordinal);
        Assert.Contains("/api/v1/admin/concepts/domains", payload, StringComparison.Ordinal);
        Assert.Contains("/api/v1/admin/import-jobs", payload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AdminPage_ShouldServeEditorAndItsAssets()
    {
        var page = await _client.GetAsync("/admin/");
        page.EnsureSuccessStatusCode();
        Assert.Contains("术语运营台", await page.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        var script = await _client.GetAsync("/admin/app.js");
        script.EnsureSuccessStatusCode();
        Assert.Contains("审核差异", await script.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        var template = await _client.GetAsync("/admin/template-v1.csv");
        template.EnsureSuccessStatusCode();
        Assert.Contains("ConceptCode,DomainCode", await template.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }
}
