using Innocap.Mcp.FundAdmin.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);

// CRITICAL: stdout is reserved for MCP frames. All logging MUST go to stderr.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddSingleton<IGlossaryRepository, YamlGlossaryRepository>();
builder.Services.AddSingleton<IKnownFieldsRegistry, YamlKnownFieldsRegistry>();
builder.Services.AddSingleton<IFundRegistry, YamlFundRegistry>();
builder.Services.AddSingleton<IJiraProjectRegistry, YamlJiraProjectRegistry>();
builder.Services.AddSingleton<ISpecifyDocSearch, FileSystemSpecifyDocSearch>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
