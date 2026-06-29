# IBKR Docker Sync Fix Summary

## Problem Description
When running the application in Docker and clicking 'Sync IBKR Data', the flex query was created successfully and reports were fetched (200 OK), but failed when trying to save the XML files with error: **"Access to the path '/app/documents' is denied"**.

## Root Causes Identified

1. **Incorrect OutputFilePath configuration**: The docker-compose.yml set the path to `/app/documents` without the `[FILE_NAME]` placeholder, causing the code to try writing to a directory instead of a file
2. **Missing `/app/documents` directory**: The Docker container didn't have the documents directory created with proper permissions
3. **403 Forbidden Error Handling**: The IBKR API sometimes returns 403 when the report isn't ready yet, but this wasn't being handled gracefully
4. **No directory existence check**: The code assumed the output directory existed before trying to save files

## Changes Made

### 1. docker-compose.yml - **CRITICAL FIX**
**Fixed**: OutputFilePath to include the `[FILE_NAME]` placeholder
```yaml
# Before:
- IBKR__OutputFilePath=/app/documents
- FinancialModelingPrep__OutputFilePath=/app/documents

# After:
- IBKR__OutputFilePath=/app/documents/[FILE_NAME]
- FinancialModelingPrep__OutputFilePath=/app/documents/[FILE_NAME]
```

This was the main issue - without the placeholder, the code tried to write directly to `/app/documents` (a directory) instead of `/app/documents/filename.xml`.

### 2. Dockerfile (`traderview/traderview.Server/Dockerfile`)
**Added**: Directory creation and permissions for `/app/documents`
```dockerfile
# Create the documents directory for IBKR reports
RUN mkdir -p /app/documents && chmod -R 755 /app/documents
```

### 3. IKBRReportServiceBase.cs
**Enhanced**: 403 Forbidden error handling with retry logic
- Added explicit check for 403 status code
- Logs when 403 is received (usually means report not ready)
- Retries automatically if within retry limit
- Provides better error message if all retries fail
- Wrapped the HTTP request in try-catch for better error handling

**Key changes**:
- Detects 403 Forbidden specifically and retries
- Adds logging to show the URL being fetched
- Continues retry loop instead of failing immediately on 403

### 4. ReportRunnerService.cs
**Added**: Directory existence checks before saving files
- Checks if output directory exists before saving XML
- Creates directory if it doesn't exist
- Applies to both main report and today's report

## Testing Instructions

### 1. Rebuild Docker Image
```powershell
# Stop and remove existing containers
docker-compose down

# Rebuild the images
docker-compose build

# Start the containers
docker-compose up -d
```

### 2. Check Container Logs
```powershell
# View logs from the server
docker logs -f traderview-server
```

### 3. Test IBKR Sync
1. Open the application at http://localhost:3000
2. Click 'Sync IBKR Data'
3. Monitor the Docker logs for:
   - "Report requested successfully. Reference code: XXXXXXXX"
   - "Fetching report from: https://..."
   - "Full report received."
   - "Successfully saved main report to /app/documents/..."
   - "Successfully saved 'Today' report to /app/documents/..."

### 4. Verify Files Were Created
```powershell
# List files in the Docker volume
docker exec traderview-server ls -la /app/documents/

# Or view a file
docker exec traderview-server cat /app/documents/[filename].xml
```

## Expected Behavior After Fix

1. ✅ The `/app/documents` directory exists in the container
2. ✅ 403 errors are handled gracefully with retries
3. ✅ Directory is created automatically if missing
4. ✅ XML files are successfully saved to `/app/documents`
5. ✅ Better logging shows exactly what's happening during sync

## Common Issues & Solutions

### Issue: Still getting 403 errors
**Possible Causes**:
- Report takes longer than expected to generate
- IBKR API rate limiting
- Token or query ID issues

**Solutions**:
- The code now retries automatically (3 attempts with 5-second delays)
- Check your IBKR token and query IDs in `.env` file
- Increase `maxRetries` or `delayInSeconds` in ReportRunnerService.cs if needed

### Issue: Files not appearing in volume
**Check**:
```powershell
# Inspect the volume
docker volume inspect IKBR_Report_Puller_traderview-doc-storage

# Check volume mounting
docker inspect traderview-server | grep -A 10 Mounts
```

### Issue: Permission denied when saving files
**Solution**: The Dockerfile now sets `chmod -R 755` on the documents directory, which should resolve this.

## Environment Variables
Ensure these are set in your `.env` file:
```
IBKR_TOKEN=your_token_here
IBKR_QUERY_ID=your_query_id
IBKR_TODAY_EXEC_ID=your_today_query_id
SQL_PASSWORD=your_db_password
FMP_API_KEY=your_fmp_api_key
```

## Next Steps
1. Rebuild and restart Docker containers
2. Test the sync functionality
3. Check Docker logs for any remaining issues
4. Verify XML files are created in the volume
