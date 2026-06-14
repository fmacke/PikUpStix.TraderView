# SQL Server Recovery - Workaround Guide

## Problem
Cannot pull SQL Server Docker image due to persistent MCR proxy issue.
Your database backup is safe at: `C:\Users\finn\source\repos\IKBR_Report_Puller\db_backup\TradingBE.bak`

## Solution Options

### Option 1: Use SQL Server Express Locally (Recommended)
1. Download SQL Server 2019 Express from Microsoft:
   https://www.microsoft.com/en-us/sql-server/sql-server-downloads

2. Install SQL Server 2019 Express Edition (free)

3. Restore your database using SSMS or command line:
   ```sql
   RESTORE DATABASE [TradingBE] 
   FROM DISK = 'C:\Users\finn\source\repos\IKBR_Report_Puller\db_backup\TradingBE.bak'
   WITH MOVE 'TradingBE' TO 'C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\DATA\TradingBE.mdf',
		MOVE 'TradingBE_log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\DATA\TradingBE_log.ldf',
		REPLACE
   ```

4. Update your connection string in `appsettings.json` to use:
   `Server=localhost\\SQLEXPRESS;Database=TradingBE;...`

### Option 2: Try on a Different Network
The MCR connection issue appears to be network-specific (EE ISP).
- Try using your phone's hotspot
- Try a different WiFi network  
- Try a VPN that doesn't use port 3128

### Option 3: Download SQL Server Image Manually
1. On a different computer/network that can access MCR:
   ```powershell
   docker pull mcr.microsoft.com/mssql/server:2019-latest
   docker save mcr.microsoft.com/mssql/server:2019-latest -o mssql-2019.tar
   ```

2. Copy the .tar file to your computer

3. Load the image:
   ```powershell
   docker load -i mssql-2019.tar
   ```

4. Run the restore-database.ps1 script

## Current Status
- ✅ Database backup created successfully (1.3GB)
- ❌ Docker cannot pull from MCR due to proxy at http.docker.internal:3128
- ❌ Docker containers inherit the same proxy issue
- ✅ Docker Hub works (can pull nginx, node, docker images)
- ❌ Microsoft Container Registry blocked

## Running Your App Without Docker
Until the network issue is resolved, you can run locally:

1. **Server**: Run from Visual Studio using "https" profile (not Docker)
2. **Client**: Run `npm run dev` in traderview.client folder
3. **Database**: Install SQL Server Express locally

All Docker files are ready and will work once the network issue is resolved.
