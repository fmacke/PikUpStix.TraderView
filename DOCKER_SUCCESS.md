# Docker Setup Complete! ✅

## Running Containers

Your application is now fully containerized and running:

1. **SQL Server Database** (`sql-server`)
   - Port: `localhost:1433`
   - Database: `TradingBE` (restored from backup)
   - Credentials: `sa` / `Gogogo123!`

2. **ASP.NET Core Server** (`traderview-server`)
   - Port: `localhost:5000`
   - Built with: .NET 10.0
   - API endpoints available at: `http://localhost:5000/api/`

3. **React Client** (`traderview-client`)
   - Port: `localhost:3000`
   - Built with: Vite + React
   - Served by: nginx
   - Access at: `http://localhost:3000`

## How We Solved the MCR Image Pull Issue

The problem was that Docker Desktop had a persistent proxy configuration blocking `mcr.microsoft.com` access. We worked around this by:

1. Using **skopeo** (running inside a Docker container) to copy images directly into the Docker daemon
2. Command used: `docker run --rm -v /var/run/docker.sock:/var/run/docker.sock quay.io/skopeo/stable copy docker://mcr.microsoft.com/[IMAGE] docker-daemon:[IMAGE]`

This bypassed the proxy issue and successfully loaded:
- `mcr.microsoft.com/mssql/server:2019-latest`
- `mcr.microsoft.com/dotnet/sdk:10.0`
- `mcr.microsoft.com/dotnet/aspnet:10.0`

## Key Changes Made

### 1. Server Dockerfile (`traderview/traderview.Server/Dockerfile`)
- Now uses .NET 10.0 SDK and runtime images
- Added `/p:BuildingInsideDocker=true` flag to skip client project build during Docker build

### 2. Server Project (`traderview.Server.csproj`)
- Made client project reference conditional: `Condition="'$(BuildingInsideDocker)' != 'true'"`
- This prevents Node.js requirement errors during Docker build

### 3. Client Build Configuration
- Created `vite.config.production.ts` for production builds
- Skips dev certificate generation (which requires `dotnet dev-certs`)
- Dockerfile updated to use: `npm run build -- --config vite.config.production.ts`

### 4. Docker Ignore
- Added `db_backup/` to `.dockerignore` to reduce build context size

## Usage

### Start the application:
```powershell
docker-compose up
```

### Stop the application:
```powershell
docker-compose down
```

### Rebuild after code changes:
```powershell
docker-compose up --build
```

### View logs:
```powershell
docker-compose logs -f traderview-server
docker-compose logs -f traderview-client
```

## Development Notes

- **Local development** still works normally with `npm run dev` and `dotnet run`
- The conditional project reference means Visual Studio can still manage both projects together
- The database is persistent in the SQL Server container (no volume mount yet - consider adding one for production)
- Client API calls route through nginx proxy: `/api/` → `http://traderview-server:8080/api/`

## Next Steps (Optional)

1. Add a volume mount for SQL Server data persistence:
   ```yaml
   volumes:
	 - sqldata:/var/opt/mssql/data
   ```

2. Set up HTTPS for production (currently HTTP only)

3. Add health checks to docker-compose.yml

4. Consider multi-stage optimization to reduce image sizes further

## Troubleshooting

If you need to pull additional MCR images in the future, use the skopeo workaround:

```powershell
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock quay.io/skopeo/stable copy docker://mcr.microsoft.com/[IMAGE_NAME]:[TAG] docker-daemon:mcr.microsoft.com/[IMAGE_NAME]:[TAG]
```

Replace `[IMAGE_NAME]` and `[TAG]` with the image you need.
