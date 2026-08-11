# User Secrets to Docker Configuration

This document explains how User Secrets are automatically transferred to Docker configuration.

## Overview

The application now automatically reads your Visual Studio User Secrets and generates a `.env` file for Docker. This means:

✅ **One source of truth**: Update secrets in Visual Studio User Secrets only
✅ **Automatic sync**: Scripts generate `.env` from User Secrets
✅ **Secure**: `.env` is in `.gitignore` and never committed
✅ **Consistent**: Same credentials work for local dev and Docker

## How It Works

### 1. User Secrets Storage

Your secrets are stored in three locations (one per project):

```
%APPDATA%\Microsoft\UserSecrets\
├── ff3a21ae-059b-4f53-8589-8b474f460b89\  (traderview.Server)
├── 16bb43dd-74cd-4a0f-bb7f-cf97fda4b833\  (Console)
└── c681a7e8-a2d6-4ed7-9098-379bcf579200\  (Library)
	└── secrets.json
```

### 2. Automatic .env Generation

When you run `start-app.ps1` or `setup-database.ps1`, they automatically:

1. Read your User Secrets
2. Generate a `.env` file with all the values
3. Docker Compose uses this `.env` file for container environment variables

### 3. Configuration Flow

```
User Secrets (Visual Studio)
	  ↓
generate-env.ps1 (reads secrets)
	  ↓
.env file (auto-generated)
	  ↓
Docker Compose (uses .env)
	  ↓
Containers (get environment variables)
```

## Managing Secrets

### View Your Current Secrets

In Visual Studio:
1. Right-click on any project → **Manage User Secrets**
2. View/edit the `secrets.json` file

### Update Secrets

**Option 1: Visual Studio (Recommended)**
1. Right-click `traderview.Server` → **Manage User Secrets**
2. Edit the JSON file
3. Save
4. Re-run `.\generate-env.ps1` to update Docker config

**Option 2: PowerShell**
```powershell
# Update database password
$secretsPath = "$env:APPDATA\Microsoft\UserSecrets\ff3a21ae-059b-4f53-8589-8b474f460b89\secrets.json"
$secrets = Get-Content $secretsPath | ConvertFrom-Json
$secrets.Database.Password = "NewPassword123!@#"
$secrets | ConvertTo-Json -Depth 10 | Set-Content $secretsPath

# Regenerate .env
.\generate-env.ps1
```

### Regenerate .env File

After updating User Secrets, regenerate the `.env` file:

```powershell
.\generate-env.ps1
```

This will:
- Read all User Secrets
- Create a new `.env` file
- Show a summary of configuration

## Configuration Values

### Database Configuration

From User Secrets:
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

To Docker `.env`:
```
SQL_PASSWORD=DevPassword123!@#
```

### IBKR Configuration

From User Secrets:
```json
{
  "IBKR": {
	"Token": "your_token_here",
	"QueryId": "your_query_id_here",
	"QueryTodayExecutionsId": "your_today_exec_id_here"
  }
}
```

To Docker `.env`:
```
IBKR_TOKEN=your_token_here
IBKR_QUERY_ID=your_query_id_here
IBKR_TODAY_EXEC_ID=your_today_exec_id_here
```

### Financial Modeling Prep API

From User Secrets:
```json
{
  "FinancialModelingPrep": {
	"ApiKey": "your_api_key_here"
  }
}
```

To Docker `.env`:
```
FMP_API_KEY=your_api_key_here
```

## Scripts Reference

### generate-env.ps1

**Purpose**: Reads User Secrets and generates `.env` file

**Usage**:
```powershell
.\generate-env.ps1
```

**What it does**:
- Checks all three User Secrets locations
- Extracts configuration values
- Creates `.env` file in project root
- Shows configuration summary

**Output**: `.env` file with all Docker environment variables

### start-app.ps1

**Purpose**: Complete application startup

**Usage**:
```powershell
.\start-app.ps1
```

**What it does**:
1. Checks Docker is running
2. **Generates .env from User Secrets** ← Automatic sync!
3. Sets up database (if needed)
4. Starts all Docker containers
5. Shows access URLs

### setup-database.ps1

**Purpose**: Database setup only

**Usage**:
```powershell
.\setup-database.ps1
```

**What it does**:
1. **Generates .env from User Secrets** ← Automatic sync!
2. Starts SQL Server container
3. Creates database and schema
4. Uses password from User Secrets

## Workflow Examples

### First-Time Setup

```powershell
# 1. Configure User Secrets in Visual Studio
# Right-click traderview.Server → Manage User Secrets
# Add your API keys, passwords, etc.

# 2. Run start-app (will auto-generate .env)
.\start-app.ps1

# That's it! Everything is configured
```

### Update API Keys

```powershell
# 1. Update User Secrets in Visual Studio
# Right-click traderview.Server → Manage User Secrets
# Update the API key

# 2. Regenerate .env
.\generate-env.ps1

# 3. Restart containers to pick up new values
docker-compose restart

# Or rebuild if needed
docker-compose up -d --build
```

### Change Database Password

```powershell
# 1. Update User Secrets in Visual Studio
# Change Database.Password value

# 2. Regenerate .env
.\generate-env.ps1

# 3. Recreate database with new password
docker-compose down
.\setup-database.ps1
```

## Troubleshooting

### .env file not being created

**Problem**: `generate-env.ps1` doesn't create `.env`

**Solution**:
1. Check User Secrets exist:
   ```powershell
   Test-Path "$env:APPDATA\Microsoft\UserSecrets\ff3a21ae-059b-4f53-8589-8b474f460b89\secrets.json"
   ```
2. If false, create User Secrets in Visual Studio
3. Run `.\generate-env.ps1` again

### Docker uses old values

**Problem**: Updated User Secrets but Docker still has old values

**Solution**:
```powershell
# 1. Regenerate .env
.\generate-env.ps1

# 2. Restart containers
docker-compose down
docker-compose up -d
```

### Password mismatch between database and application

**Problem**: Application can't connect to database

**Solution**:
```powershell
# 1. Verify all User Secrets have same password
$ids = @(
	"ff3a21ae-059b-4f53-8589-8b474f460b89",
	"16bb43dd-74cd-4a0f-bb7f-cf97fda4b833",
	"c681a7e8-a2d6-4ed7-9098-379bcf579200"
)
foreach ($id in $ids) {
	$path = "$env:APPDATA\Microsoft\UserSecrets\$id\secrets.json"
	if (Test-Path $path) {
		$s = Get-Content $path | ConvertFrom-Json
		Write-Host "$id : $($s.Database.Password)"
	}
}

# 2. Update all to match (if needed)
# Use Visual Studio or PowerShell

# 3. Regenerate .env and restart
.\generate-env.ps1
docker-compose down
.\setup-database.ps1
```

### Manual .env override

If you need to temporarily override values without changing User Secrets:

```powershell
# Edit .env file directly
notepad .env

# Restart containers
docker-compose restart
```

**Note**: The `.env` file will be regenerated on next script run, so manual changes are temporary.

## Security Best Practices

### ✅ Do's

- ✅ Store secrets in User Secrets (managed by scripts)
- ✅ Keep `.env` in `.gitignore`
- ✅ Use strong passwords
- ✅ Rotate credentials regularly
- ✅ Use different passwords for dev/staging/prod

### ❌ Don'ts

- ❌ Don't commit `.env` to source control
- ❌ Don't commit User Secrets (they're outside repo by default)
- ❌ Don't share `.env` file in chat/email
- ❌ Don't use production credentials in dev environment
- ❌ Don't hardcode secrets in code

## Production Considerations

For production deployments:

1. **Don't use User Secrets** (they're for development only)
2. **Use proper secret management**:
   - Azure Key Vault
   - AWS Secrets Manager
   - HashiCorp Vault
   - Kubernetes Secrets
3. **Set environment variables** directly in production environment
4. **Use CI/CD pipelines** to inject secrets at deploy time

Example production configuration:
```yaml
# docker-compose.prod.yml
services:
  traderview-server:
	environment:
	  - Database__Password=${DB_PASSWORD}  # From CI/CD secret
	  - IBKR__Token=${IBKR_TOKEN}          # From secret manager
```

## Summary

- **User Secrets → .env → Docker** is automatic
- Run `start-app.ps1` and everything just works
- Update secrets in Visual Studio User Secrets
- Re-run `generate-env.ps1` to sync to Docker
- `.env` is auto-generated and git-ignored
- Same credentials work everywhere (local + Docker)

This approach gives you the best of both worlds:
- **Developer-friendly**: Manage secrets in familiar Visual Studio UI
- **Docker-compatible**: Containers get proper environment variables
- **Secure**: No secrets in source control
- **Consistent**: One source of truth for all configuration
