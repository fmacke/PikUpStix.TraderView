# Complete Docker Setup and Run Guide

## Quick Start - Run Everything

### 1. Setup Database (First Time Only)

Open PowerShell in the project root and run:

```powershell
.\setup-database.ps1
```

Wait for the success message showing connection details.

### 2. Start All Services

```powershell
docker-compose up -d
```

That's it! Your application is now running.

---

## Detailed Steps

### Initial Setup (First Time)

1. **Ensure Docker Desktop is running**
   ```powershell
   docker version
   ```

2. **Run the database setup script**
   ```powershell
   .\setup-database.ps1
   ```

   This will:
   - Create SQL Server container
   - Initialize TradingBE database
   - Create all tables and indexes
   - Use password from your User Secrets

3. **Verify database is running**
   ```powershell
   docker ps
   ```

   You should see `traderview-sqlserver` with status "healthy"

### Start the Application

**Option A: Background mode (recommended)**
```powershell
docker-compose up -d
```

**Option B: Interactive mode (see logs)**
```powershell
docker-compose up
```

Press `Ctrl+C` to stop when in interactive mode.

### Access Your Application

Once running:
- **Frontend (Client):** http://localhost:3000
- **Backend API (Server):** http://localhost:5000
- **Swagger/API Docs:** http://localhost:5000/swagger (in Development mode)
- **Database:** localhost,1433

---

## Common Commands

### Check Running Containers
```powershell
docker-compose ps
```

### View Logs
```powershell
# All services
docker-compose logs

# Specific service
docker-compose logs traderview-server
docker-compose logs traderview-client
docker-compose logs sqlserver

# Follow logs in real-time
docker-compose logs -f traderview-server
```

### Stop All Services
```powershell
docker-compose stop
```

### Stop and Remove Containers
```powershell
docker-compose down
```

### Restart Everything
```powershell
docker-compose restart
```

### Restart Specific Service
```powershell
docker-compose restart traderview-server
```

### Rebuild After Code Changes
```powershell
# Rebuild and restart
docker-compose up -d --build

# Rebuild specific service
docker-compose up -d --build traderview-server
```

### Clean Everything and Start Fresh
```powershell
# Stop and remove containers (keeps database data)
docker-compose down

# Remove volumes too (DELETES DATABASE DATA!)
docker-compose down -v

# Then setup again
.\setup-database.ps1
docker-compose up -d
```

---

## Troubleshooting

### Database Not Ready
If services fail to start, the database might not be ready yet:

```powershell
# Check database health
docker inspect --format='{{.State.Health.Status}}' traderview-sqlserver

# Wait for it to show "healthy", then restart services
docker-compose restart traderview-server
```

### Connection Refused
```powershell
# Ensure all containers are on the same network
docker network ls
docker network inspect pikupstixtraderview_traderview-network
```

### Port Already in Use
If you get "port already allocated":

```powershell
# Check what's using the port (e.g., 1433, 5000, 3000)
netstat -ano | findstr :1433
netstat -ano | findstr :5000
netstat -ano | findstr :3000

# Stop the conflicting process or change the port in docker-compose.yml
```

### View Container Details
```powershell
# Inspect a container
docker inspect traderview-sqlserver

# Execute commands inside container
docker exec -it traderview-sqlserver bash
```

### Database Connection Issues
```powershell
# Test database connection
docker exec traderview-sqlserver /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "DevPassword123!@#" -C -Q "SELECT 1"
```

---

## Development Workflow

### Working Locally (Without Docker)

If you want to run the app locally but use the Docker database:

1. **Start only the database:**
   ```powershell
   docker-compose up -d sqlserver
   ```

2. **Run the application in Visual Studio:**
   - Press F5 or Ctrl+F5
   - The app will connect to localhost,1433 using User Secrets

### Making Code Changes

**For local development:** Just edit code and restart in Visual Studio

**For Docker deployment:**
1. Make your code changes
2. Rebuild the Docker images:
   ```powershell
   docker-compose up -d --build
   ```

### Updating Database Schema

If you modify the database schema:

1. **Update DatabaseCreationScript.sql**
2. **Run the script against the database:**
   ```powershell
   docker cp DatabaseCreationScript.sql traderview-sqlserver:/tmp/
   docker exec traderview-sqlserver /opt/mssql-tools18/bin/sqlcmd `
	 -S localhost -U sa -P "DevPassword123!@#" -C -i /tmp/DatabaseCreationScript.sql
   ```

---

## Environment Variables

The application uses these sources for configuration (in order of precedence):

1. **User Secrets** (Visual Studio, local development)
2. **Environment Variables** (Docker, from docker-compose.yml)
3. **appsettings.json** (default fallback)

To modify Docker environment variables, edit `docker-compose.yml` or create/update `.env` file.

---

## Complete Startup Sequence

```powershell
# 1. Start Docker Desktop (if not running)

# 2. Setup database (first time only)
.\setup-database.ps1

# 3. Start all services
docker-compose up -d

# 4. Check status
docker-compose ps

# 5. View logs (optional)
docker-compose logs -f

# 6. Access your app at http://localhost:3000
```

---

## Shutdown Sequence

```powershell
# Stop all services (keeps data)
docker-compose stop

# Or stop and remove containers (still keeps data in volumes)
docker-compose down

# Database data persists in Docker volume 'pikupstixtraderview_sqlserver_data'
```

---

## Tips

- Run `docker-compose ps` frequently to check container status
- Use `docker-compose logs -f [service]` to debug issues
- The database data persists in a Docker volume even after `docker-compose down`
- To completely reset: `docker-compose down -v` (WARNING: deletes all data!)
- For development, running locally with Docker database is often faster than full Docker
