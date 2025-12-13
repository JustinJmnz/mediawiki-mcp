# MediaWiki with MariaDB Docker Setup

This Docker Compose configuration runs MediaWiki with a MariaDB (MySQL) database backend, ideal for developing a MediaWiki MCP server application.

## Prerequisites

- Docker daemon running (Docker Desktop, Docker Engine on WSL 2, or similar)
- Docker Compose (included with Docker Desktop)
- At least 2GB of free disk space

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

## Development Notes

- The MediaWiki container uses the official `mediawiki:latest` image
- MariaDB is a MySQL-compatible database officially supported by MediaWiki
- Both services communicate on the `mediawiki-network` Docker network
- Volumes ensure data persistence between container restarts

## Troubleshooting

### MediaWiki shows an error page
1. Check MariaDB is running: `docker-compose ps`
2. View MediaWiki logs: `docker-compose logs mediawiki`
3. Verify database credentials in `LocalSettings.php`
4. Ensure `$wgServer` is set correctly in `LocalSettings.php`

### Port already in use
- Change port mappings in `docker-compose.yml`:
  - MariaDB: Change `3306:3306` to `3307:3306`
  - MediaWiki: Change `80:80` to `8080:80`

### Container crashes on startup
Check the logs: `docker-compose logs mediawiki` or `docker-compose logs mariadb`

## Next Steps for MCP Server Development

1. Set up your MCP server application with MediaWiki API access
2. Use the credentials and connection details above
3. Connect to MediaWiki at `http://localhost`
4. Connect to MariaDB at `localhost:3306`

Refer to [MediaWiki API Documentation](https://www.mediawiki.org/wiki/API:Main_page) for integration details.
