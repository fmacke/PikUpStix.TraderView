# Quick Start Guide - Database Setup

## For Impatient Developers 🚀

Want to get the database running quickly? Just run:

```powershell
.\setup-database.ps1
```

That's it! The script will:
- ✅ Check Docker is running
- ✅ Create .env file with defaults
- ✅ Start SQL Server container
- ✅ Create TradingBE database
- ✅ Set up all tables and indexes
- ✅ Show you connection details

## What You Get

**Database:** `TradingBE`  
**Server:** `localhost,1433`  
**Username:** `sa`  
**Password:** `DevPassword123!@#` (already set in User Secrets)

## Next Steps

### 1. User Secrets Already Configured ✅

Your User Secrets have been pre-configured with:

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

The setup script will use this password automatically!

### 2. Run the Application

Press F5 in Visual Studio or:

```bash
dotnet run --project traderview/traderview.Server
```

### 3. (Optional) Add Your API Keys

Edit `.env` file to add:
- IBKR credentials
- Financial Modeling Prep API key
- Other configuration

## Useful Commands

| Task | Command |
|------|---------|
| Start database | `docker-compose up -d sqlserver` |
| Stop database | `docker-compose stop sqlserver` |
| View logs | `docker logs traderview-sqlserver` |
| Reset database | `docker-compose down -v` then re-run setup |

## Having Issues?

See the full [DATABASE_SETUP.md](DATABASE_SETUP.md) guide for troubleshooting.

## Connect with SQL Tools

You can use:
- **Azure Data Studio**
- **SQL Server Management Studio (SSMS)**
- **Visual Studio SQL Server Object Explorer**
- **DBeaver**

Connection details are shown after running the setup script.
