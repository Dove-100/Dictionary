using TriDict;
using Volo.Abp;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
await builder.AddApplicationAsync<TriDictHttpApiHostModule>();
var app = builder.Build();
await app.InitializeApplicationAsync();
await app.RunAsync();

public partial class Program;
