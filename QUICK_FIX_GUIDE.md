# Quick Fix Guide - Docker Permission Error

## The Problem
```
Full report received.
An error occurred: Access to the path '/app/documents' is denied.
```

## The Solution
The configuration was missing the `[FILE_NAME]` placeholder!

### What Changed in docker-compose.yml:
```yaml
# ❌ WRONG - Missing [FILE_NAME] placeholder
- IBKR__OutputFilePath=/app/documents

# ✅ CORRECT - Includes [FILE_NAME] placeholder  
- IBKR__OutputFilePath=/app/documents/[FILE_NAME]
```

## Why This Matters
The code does this:
```csharp
string filePath = outputFilePath.Replace("[FILE_NAME]", "20260108_123456.xml");
```

- **With correct config**: `/app/documents/[FILE_NAME]` → `/app/documents/20260108_123456.xml` ✅
- **With wrong config**: `/app/documents` → `/app/documents` (tries to write to directory!) ❌

## Steps to Apply Fix

### 1. Stop Current Containers
```powershell
docker-compose down
```

### 2. Rebuild Images (to get Dockerfile changes)
```powershell
docker-compose build
```

### 3. Start Containers (will use new docker-compose.yml)
```powershell
docker-compose up -d
```

### 4. Test the Sync
```powershell
# Watch the logs
docker logs -f traderview-server
```

Then click "Sync IBKR Data" in the UI.

### 5. Verify Files Were Created
```powershell
# List files in the documents directory
docker exec traderview-server ls -la /app/documents/

# You should see files like:
# 20260108_123456_TraderSyncAccess.xml
# 20260108_TraderSyncAccess_today.xml
```

## Expected Output in Logs
```
Report requested successfully. Reference code: 7238013773
Attempt 1 of 3: Fetching the full report in 5 seconds...
Fetching report from: https://gdcdyn.interactivebrokers.com/...
Full report received.
Successfully saved main report to /app/documents/20260108_123456_TraderSyncAccess.xml
Successfully saved 'Today' report to /app/documents/20260108_TraderSyncAccess_today.xml
```

## No More Errors! 🎉

The fix is simple but critical. The `[FILE_NAME]` placeholder allows the code to properly construct file paths instead of trying to write to a directory.
