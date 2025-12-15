# MediaWiki with MariaDB Docker Setup

This Docker Compose configuration runs MediaWiki with a MariaDB (MySQL) database backend, ideal for developing a MediaWiki MCP server application.

## Prerequisites

- Docker daemon running (Docker Desktop, Docker Engine on WSL 2, or similar)
- Docker Compose (included with Docker Desktop)
- At least 2GB of free disk space

## MCP Server Features

The MediaWiki MCP Server is a .NET 8.0 Model Context Protocol implementation that enables AI assistants to interact with your MediaWiki instance through VS Code.

### Available Tools (10 Total)

**Read-Only Access (6 tools):**
- `search_pages` - Search wiki pages by keyword
- `get_page_content` - Retrieve full page content
- `get_page_info` - Get page metadata (ID, length, revision)
- `list_all_pages` - Browse all published pages
- `get_recent_changes` - View recent edits
- `get_site_info` - Retrieve wiki configuration

**Draft Management (4 tools):**
- `create_draft_page` - Create new content in Draft namespace for review
- `edit_draft_page` - Modify existing drafts based on feedback
- `publish_draft` - Move approved content to main namespace
- `delete_page` - Remove unsuitable or rejected pages

### Draft Workflow

All content created by the AI goes to the `Draft:` namespace by default:

1. **Review**: Navigate to `http://localhost/wiki/Draft:Page_Name`
2. **Approve**: Use the `publish_draft` tool to move to main namespace
3. **Revise**: Use `edit_draft_page` to request changes
4. **Reject**: Use `delete_page` to remove unsuitable content


## Quick Start

**Important:** Ensure the Docker daemon is running before proceeding. If you get a "system cannot find the file" error, the Docker daemon isn't accessible. You can:
- Start Docker Desktop, or
- Start the Docker daemon on WSL 2/your system
- Wait 2-3 minutes for full startup

Then start the services:
```bash
docker-compose up -d
```

2. **Wait for services to be ready:**
```bash
docker-compose ps
```

Both services should show as "running".

3. **Access MediaWiki:**
Open your browser and navigate to `http://localhost`

4. **Login credentials:**
- Username: `admin`
- Password: `Password123!`

### Set Up the MCP Server in VS Code

5. **Install the MCP Extension:**
   - Open VS Code Extensions (`Ctrl+Shift+X`)
   - Search for and install "MCP Extension" (by ModelContextProtocol)

6. **Configure the MCP Server:**
   - Open VS Code Settings (`Ctrl+,`)
   - Search for "mcp.json" or navigate to your MCP extension settings
   - Add this configuration:

```json
{
  "servers": {
    "media-wiki": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "c:\\Users\\Justi\\source\\repos\\mediawiki-mcp\\MediaWikiMCP"
      ],
      "env": {
        "MEDIAWIKI_URL": "http://localhost"
      }
    }
  }
}
```

**Note:** Update the path to match your installation directory.

7. **Verify the connection:**
   - Restart VS Code or reload the window (`Ctrl+R`)
   - Open the MCP extension panel
   - You should see "media-wiki" listed with 10 available tools (6 read-only + 4 draft management)

## Services

### MariaDB (Port 3306)
- **Container Name:** mediawiki-mariadb
- **Root Password:** rootpass123
- **Database:** mediawiki
- **DB User:** mediawiki
- **DB Password:** MediaWikiPass123!
- **Volume:** mariadb_data (persists database)

### MediaWiki (Port 80)
- **Container Name:** mediawiki
- **URL:** http://localhost
- **Admin User:** admin
- **Admin Password:** Password123!
- **Volume:** mediawiki_data (persists wiki files and images)

## Useful Commands

### View logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f mediawiki
docker-compose logs -f mariadb
```

### Access MariaDB
```bash
docker exec -it mediawiki-mariadb mysql -u mediawiki -p mediawiki
```
Then enter password: `MediaWikiPass123!`

### Stop services
```bash
docker-compose down
```

### Stop and remove volumes (clean slate)
```bash
docker-compose down -v
```

### Rebuild images
```bash
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

## Configuration

- **LocalSettings.php:** Main MediaWiki configuration file
- **docker-compose.yml:** Docker service definitions

Modify `LocalSettings.php` to customize MediaWiki settings. Changes take effect after restarting the container.

## Troubleshooting

Check the logs: `docker-compose logs mediawiki` or `docker-compose logs mariadb`