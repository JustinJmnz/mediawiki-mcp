# MediaWiki MCP Server

A Model Context Protocol (MCP) server for MediaWiki that enables LLM integration with your wiki instance using the official ModelContextProtocol .NET SDK.

## Quick Start

### Prerequisites
- .NET 8.0 SDK or later
- Running MediaWiki instance (local or remote)

### Build
```powershell
cd MediaWikiMCP
dotnet build
```

### Run
```powershell
dotnet run
```

### Configure
Set the `MEDIAWIKI_URL` environment variable to your wiki instance:
```powershell
$env:MEDIAWIKI_URL = "http://your-wiki-instance"
dotnet run
```

Default is `http://localhost`.

## Available Tools

The server provides 6 read-only tools for wiki interaction:

1. **search_pages** (query: string)
   - Full-text search for pages in the wiki
   
2. **get_page_content** (title: string)
   - Retrieve complete page text/wikitext
   
3. **get_page_info** (title: string)
   - Get page metadata (ID, size, last modified timestamp)
   
4. **get_recent_changes** (limit?: integer, default: 10, max: 100)
   - List recent edits with user, timestamp, edit comments
   
5. **list_all_pages** (limit?: integer, default: 50, max: 500)
   - List all pages in the wiki with metadata
   
6. **get_site_info** ()
   - Get general wiki information (site name, server name, main page)

## Integration with MCP Clients

### Claude Desktop
Add to your `.config/Claude/claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "mediawiki": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/MediaWikiMCP"],
      "env": {
        "MEDIAWIKI_URL": "http://localhost"
      }
    }
  }
}
```

### Other MCP Clients
The server communicates via stdio, so any MCP-compatible client can connect to it.

## Architecture

- **Program.cs** - MCP server using official `ModelContextProtocol` SDK
  - Attribute-based tool registration (`[McpServerToolType]`, `[McpServerTool]`)
  - Dependency injection via Microsoft.Extensions.Hosting
  - Proper protocol implementation with error handling

- **Services/MediaWikiService.cs** - MediaWiki REST API client
  - HTTP communication with wiki instance
  - JSON deserialization
  - Read-only operations

## Protocol Details

- **MCP Protocol Version**: 2024-11-05
- **Server Name**: mediawiki-mcp
- **Communication**: stdin/stdout (JSON-RPC 2.0)
- **Capabilities**: Tools only (no resources or prompts)

## Building and Testing

### Debug Build
```powershell
dotnet build
```

### Release Build
```powershell
dotnet build -c Release
```

### Run Tests
```powershell
dotnet test
```

## Environment Variables

- `MEDIAWIKI_URL` - Base URL of the MediaWiki instance (default: `http://localhost`)

## Dependencies

- `ModelContextProtocol` (0.1.0-prerelease) - Official MCP protocol implementation
- `Microsoft.Extensions.Hosting` (8.0.0) - Hosting and DI
- `Microsoft.Extensions.DependencyInjection` (8.0.0) - Dependency injection
- `Microsoft.Extensions.Logging.Console` (8.0.0) - Logging
- `System.Text.Json` (8.0.0) - JSON serialization

## Implementation Notes

1. **Attribute-Based Registration**: Tools are automatically discovered via attributes rather than manual registration.

2. **Dependency Injection**: MediaWikiService is injected into the tools class, enabling testing and flexibility.

3. **Async All The Way**: All tool methods are async to properly handle async MediaWiki API calls.

4. **Error Handling**: Tool errors are caught and returned as MCP error responses.

5. **Logging Configuration**: Stderr is used for diagnostics, preserving stdout for MCP protocol.

## Limitations

- **Read-Only**: No write operations (create/edit pages)
- **Tools Only**: Resources and prompts endpoints not implemented
- **Single Wiki**: Configured for one MediaWiki instance

## See Also

- [MCP_IMPLEMENTATION.md](./MCP_IMPLEMENTATION.md) - Detailed implementation documentation
- [ModelContextProtocol GitHub](https://github.com/modelcontextprotocol/python-sdk)
