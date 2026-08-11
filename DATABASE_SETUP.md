# Database Setup Guide

This guide explains how to set up the SQL Server database for the TraderView application using Docker.

## Prerequisites

- Docker Desktop installed and running
- PowerShell (Windows) or Bash (Linux/Mac)

## Quick Start

### Option 1: Automated Setup (Recommended)

Run the setup script to automatically create and initialize the database:

```powershell
.\setup-database.ps1
```

This script will:
1. Check if Docker is running
2. Create a `.env` file if it doesn't exist
3. Start the SQL Server container
4. Wait for SQL Server to be ready
5. Create the TradingBE database
6. Run the database creation script to set up all tables

### Option 2: Manual Setup

1. **Start the SQL Server container:**

```bash
docker-compose up -d sqlserver
```

2. **Wait for the container to be healthy:**

```bash
docker ps
# Wait until the STATUS shows "healthy"
```

3. **Run the database creation script:**

```bash
docker exec traderview-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C -i /tmp/DatabaseCreationScript.sql
```

(First copy the script: `docker cp DatabaseCreationScript.sql traderview-sqlserver:/tmp/`)

## Database Connection Details

Once the database is set up, you can connect using:

- **Server:** `localhost,1433` (or `sqlserver` from within Docker network)
- **Database:** `TradingBE`
- **Username:** `sa`
- **Password:** `DevPassword123!@#` (set in User Secrets and `.env` file)

### Connection String

```
Server=localhost,1433;Database=TradingBE;User Id=sa;Password=DevPassword123!@#;TrustServerCertificate=true;Encrypt=false;
```

## Configure Application

### For Local Development (Visual Studio)

Your User Secrets are **already configured** with the database connection! The setup script automatically uses the password from User Secrets located at:

```
%APPDATA%\Microsoft\UserSecrets\16bb43dd-74cd-4a0f-bb7f-cf97fda4b833\secrets.json
```

Current configuration:
```json
{
  "Database": {
	"User": "sa",
	"Password": "DevPassword123!@#",
	"Host": "localhost,1433",
	"DbName": "TradingBE"
  }
}
```

To change the password:
1. Update it in User Secrets (right-click `traderview.Server` → Manage User Secrets)
2. Update it in `.env` file
3. Restart the SQL Server container: `docker-compose restart sqlserver`

### For Docker Deployment

The connection is already configured in `docker-compose.yml` and will use the SQL Server container.

## Database Schema

The database includes the following tables:

1. **Instruments** - Trading instrument information
2. **TradeExecutions** - All trade execution records from IBKR reports
3. **OpenPositions** - Open position snapshots
4. **HistoricalData** - Historical price data
5. **EconomicCalendar** - Economic calendar events
6. **Notes** - Trading notes
7. **Lists** - Custom instrument lists

For the complete schema, see `DatabaseCreationScript.sql`.

## Common Commands

### Start the database

```bash
docker-compose up -d sqlserver
```

### Stop the database

```bash
docker-compose stop sqlserver
```

### Stop and remove the database (data will be preserved in volume)

```bash
docker-compose down
```

### View database logs

```bash
docker logs traderview-sqlserver
```

### Connect to SQL Server from command line

```bash
docker exec -it traderview-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C
```

### Delete all data (WARNING: This removes the database volume)

```bash
docker-compose down -v
```

## Troubleshooting

### Container won't start

Check Docker logs:
```bash
docker logs traderview-sqlserver
```

### Password authentication fails

1. Make sure the password in `.env` matches what you're using
2. The password must meet SQL Server complexity requirements (uppercase, lowercase, numbers, special characters)
3. Restart the container after changing the password:
   ```bash
   docker-compose down
   docker-compose up -d sqlserver
   ```

### Can't connect from application

1. Verify the container is running and healthy:
   ```bash
   docker ps
   ```

2. Check if port 1433 is accessible:
   ```bash
   docker port traderview-sqlserver
   ```

3. Test connection:
   ```bash
   docker exec traderview-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C -Q "SELECT 1"
   ```

### Database script fails to run

1. Ensure the script file exists in the project root
2. Manually copy and run:
   ```bash
   docker cp DatabaseCreationScript.sql traderview-sqlserver:/tmp/
   docker exec traderview-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C -i /tmp/DatabaseCreationScript.sql
   ```

## Security Notes

⚠️ **Important:** The password `DevPassword123!@#` is for **local development only**. 

For production:
1. Use a strong, unique password
2. Store it securely (Azure Key Vault, AWS Secrets Manager, etc.)
3. Never commit `.env` file or User Secrets to source control
4. Consider using Windows Authentication or Azure AD authentication

The `.gitignore` file should already exclude:
- `.env` (Docker environment variables)
- User Secrets are stored outside the repository by default

## Backup and Restore

### Create a backup

```bash
docker exec traderview-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C -Q "BACKUP DATABASE TradingBE TO DISK = '/var/opt/mssql/backup/TradingBE.bak'"
```

### Restore from backup

```bash
docker exec traderview-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C -Q "RESTORE DATABASE TradingBE FROM DISK = '/var/opt/mssql/backup/TradingBE.bak' WITH REPLACE"
```

## Additional Resources

- [SQL Server Docker Documentation](https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
