using System.ComponentModel;
using MediaWikiMCP.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace MediaWikiMCP;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        // Configure logging to output errors to stderr
        builder.Logging.AddConsole(consoleLogOptions =>
        {
            consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        // Add the MediaWiki service as a singleton
        var wikiUrl = Environment.GetEnvironmentVariable("MEDIAWIKI_URL") ?? "http://localhost";
        builder.Services.AddSingleton(new MediaWikiService(wikiUrl, new HttpClient()));

        // Add MCP Server
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        var app = builder.Build();
        await app.RunAsync();
    }
}

/// <summary>
/// MediaWiki MCP Tools - provides access to wiki operations
/// </summary>
[McpServerToolType]
public class MediaWikiTools
{
    private readonly MediaWikiService _wikiService;

    public MediaWikiTools(MediaWikiService wikiService)
    {
        _wikiService = wikiService;
    }

    [McpServerTool]
    [Description("Search for pages in the wiki by keyword")]
    public async Task<object> SearchPages(string query)
    {
        return await _wikiService.SearchPages(query);
    }

    [McpServerTool]
    [Description("Get the full text content of a wiki page")]
    public async Task<object> GetPageContent(string title)
    {
        return await _wikiService.GetPageContent(title);
    }

    [McpServerTool]
    [Description("Get metadata about a wiki page")]
    public async Task<object> GetPageInfo(string title)
    {
        return await _wikiService.GetPageInfo(title);
    }

    [McpServerTool]
    [Description("Get recent changes to the wiki")]
    public async Task<object> GetRecentChanges(int limit = 10)
    {
        return await _wikiService.GetRecentChanges(limit);
    }

    [McpServerTool]
    [Description("List all pages in the wiki")]
    public async Task<object> ListAllPages(int limit = 50)
    {
        return await _wikiService.ListAllPages(limit);
    }

    [McpServerTool]
    [Description("Get general information about the wiki site")]
    public async Task<object> GetSiteInfo()
    {
        return await _wikiService.GetSiteInfo();
    }
}
