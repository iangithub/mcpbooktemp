using McpOrderServer.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;


Console.WriteLine("Hello, MCP Server!");


var builder = Host.CreateApplicationBuilder(args);


// 建立 MCP Server，採用 Stdio 傳輸並自動掃描工具
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();    // 掃描工具類別


await builder.Build().RunAsync();