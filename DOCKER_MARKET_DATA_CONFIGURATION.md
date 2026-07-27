# Docker Compose Configuration Guide

## Overview
The Docker Compose setup now supports both Yahoo Finance and Financial Modeling Prep as market data sources.

## Environment Variables Setup

### Step 1: Create .env File

Copy the example file and fill in your values:
```bash
cp .env.example .env
```

### Step 2: Configure Market Data Service

Edit your `.env` file and set the preferred market data service:

#### Option 1: Use Yahoo Finance (No API Key Required)
```bash
MARKET_DATA_SERVICE=yahoo
YAHOO_FINANCE_BASE_URL=https://query1.finance.yahoo.com
```

#### Option 2: Use Financial Modeling Prep (API Key Required)
```bash
MARKET_DATA_SERVICE=fmp
FMP_API_KEY=your_actual_api_key_here
```

## Complete .env File Example

```env
# Database
SQL_PASSWORD=YourStrongPassword123!

# IBKR API
IBKR_TOKEN=your_ibkr_token_here
IBKR_QUERY_ID=123456
IBKR_TODAY_EXEC_ID=654321

# Market Data Service Selection
MARKET_DATA_SERVICE=yahoo

# Financial Modeling Prep (only needed if MARKET_DATA_SERVICE=fmp)
FMP_API_KEY=your_fmp_api_key

# Yahoo Finance (optional, has defaults)
YAHOO_FINANCE_BASE_URL=https://query1.finance.yahoo.com
```

## Environment Variables Reference

### Market Data Configuration

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `MARKET_DATA_SERVICE` | No | `fmp` | Market data service to use: `yahoo` or `fmp` |
| `FMP_API_KEY` | If using FMP | - | Your Financial Modeling Prep API key |
| `YAHOO_FINANCE_BASE_URL` | No | `https://query1.finance.yahoo.com` | Yahoo Finance API endpoint |

### Database Configuration

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `SQL_PASSWORD` | Yes | - | SQL Server SA password |

### IBKR Configuration

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `IBKR_TOKEN` | Yes | - | Interactive Brokers API token |
| `IBKR_QUERY_ID` | Yes | - | IBKR Flex Query ID |
| `IBKR_TODAY_EXEC_ID` | Yes | - | IBKR Today Executions Query ID |

## Docker Compose Environment Variables

The following environment variables are passed to the container:

### Market Data Variables
```yaml
- MarketData__PreferredService=${MARKET_DATA_SERVICE:-fmp}
- FinancialModelingPrep__ApiKey=${FMP_API_KEY}
- FinancialModelingPrep__BaseUrl=https://financialmodelingprep.com/stable
- FinancialModelingPrep__OutputFilePath=/app/documents/[FILE_NAME]
- YahooFinance__BaseUrl=${YAHOO_FINANCE_BASE_URL:-https://query1.finance.yahoo.com}
- YahooFinance__OutputFilePath=/app/documents/[FILE_NAME]
```

## Running with Docker Compose

### Start Services
```bash
# Using Yahoo Finance
docker-compose up -d

# Or explicitly set in command
MARKET_DATA_SERVICE=yahoo docker-compose up -d
```

### Stop Services
```bash
docker-compose down
```

### View Logs
```bash
# All services
docker-compose logs -f

# Server only
docker-compose logs -f traderview-server

# Client only
docker-compose logs -f traderview-client
```

### Rebuild After Changes
```bash
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

## Switching Between Market Data Services

### Switch to Yahoo Finance
```bash
# Method 1: Update .env file
echo "MARKET_DATA_SERVICE=yahoo" >> .env

# Method 2: Set environment variable
export MARKET_DATA_SERVICE=yahoo

# Restart containers
docker-compose restart traderview-server
```

### Switch to Financial Modeling Prep
```bash
# Method 1: Update .env file
echo "MARKET_DATA_SERVICE=fmp" >> .env
echo "FMP_API_KEY=your_api_key" >> .env

# Method 2: Set environment variables
export MARKET_DATA_SERVICE=fmp
export FMP_API_KEY=your_api_key

# Restart containers
docker-compose restart traderview-server
```

## Service Comparison in Docker

| Feature | Yahoo Finance | Financial Modeling Prep |
|---------|--------------|-------------------------|
| **Setup Complexity** | ⭐ Easy (no API key) | ⭐⭐ Moderate (needs API key) |
| **Configuration** | Just set service name | Need API key in .env |
| **Historical Data** | ✅ Yes | ✅ Yes |
| **Economic Calendar** | ❌ No | ✅ Yes |
| **Cost** | 💚 Free | 💛 Free tier + Paid |

## Troubleshooting

### Service Not Switching
1. Check `.env` file for correct `MARKET_DATA_SERVICE` value
2. Restart the container: `docker-compose restart traderview-server`
3. Check logs: `docker-compose logs traderview-server`

### Yahoo Finance Not Working
1. Verify internet connectivity from container
2. Try alternative URL: Set `YAHOO_FINANCE_BASE_URL=https://query2.finance.yahoo.com`
3. Check container logs for error messages

### FMP Not Working
1. Verify `FMP_API_KEY` is set correctly in `.env`
2. Check API key validity at Financial Modeling Prep dashboard
3. Ensure API key hasn't reached rate limits

### Container Cannot Connect to Database
1. Verify `SQL_PASSWORD` is correct in `.env`
2. Check if SQL Server is running and accessible
3. Verify `host.docker.internal` resolves correctly

## Checking Active Service

View container logs to see which service is initialized:
```bash
docker-compose logs traderview-server | grep -i "market\|yahoo\|fmp"
```

Expected output:
```
Using market data service: YahooFinance
```
or
```
Using market data service: FinancialModellingPrep
```

## Volume and Data Persistence

Market data files are stored in the `traderview-doc-storage` Docker volume:

```bash
# Inspect volume
docker volume inspect traderview-doc-storage

# Access volume data
docker run --rm -v traderview-doc-storage:/data alpine ls -la /data
```

## Network Configuration

Services communicate via the `traderview-network` bridge network:
- **traderview-server**: Port 5000 (host) → 8080 (container)
- **traderview-client**: Port 3000 (host) → 80 (container)

## Security Best Practices

1. **Never commit .env file** - It contains sensitive credentials
2. **Use strong passwords** - Especially for `SQL_PASSWORD`
3. **Rotate API keys** - Regularly update `IBKR_TOKEN` and `FMP_API_KEY`
4. **Limit exposure** - Only expose necessary ports

## Example Production .env

```env
# Database
SQL_PASSWORD=UltraSecurePassword!2024$Complex

# IBKR
IBKR_TOKEN=production_token_here
IBKR_QUERY_ID=123456
IBKR_TODAY_EXEC_ID=654321

# Market Data - Yahoo Finance (no API key needed)
MARKET_DATA_SERVICE=yahoo
YAHOO_FINANCE_BASE_URL=https://query1.finance.yahoo.com
```

## Alternative: Using docker-compose.override.yml

For local development overrides:

```yaml
# docker-compose.override.yml
version: '3.8'
services:
  traderview-server:
	environment:
	  - MarketData__PreferredService=yahoo
	  - Logging__LogLevel__Default=Debug
```

This file is automatically merged with `docker-compose.yml` and should also be in `.gitignore`.

## Further Reading

- [Docker Compose Environment Variables](https://docs.docker.com/compose/environment-variables/)
- [Docker Networking](https://docs.docker.com/network/)
- [Yahoo Finance Service Documentation](./IKBR_Report_Puller/Services/MarketData/YahooFinanceService_README.md)
- [Market Data Configuration](./IKBR_Report_Puller.Console/MARKET_DATA_CONFIGURATION.md)
