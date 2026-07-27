# Docker Quick Start - Yahoo Finance

## 🚀 Quick Setup (3 steps)

### 1. Create .env file
```bash
cp .env.example .env
```

### 2. Edit .env - Add these lines:
```bash
# Choose Yahoo Finance (no API key needed!)
MARKET_DATA_SERVICE=yahoo

# Required: Database password
SQL_PASSWORD=YourPassword123!

# Required: IBKR credentials
IBKR_TOKEN=your_token
IBKR_QUERY_ID=your_query_id
IBKR_TODAY_EXEC_ID=your_exec_id
```

### 3. Start Docker
```bash
docker-compose up -d
```

## ✅ That's it! Yahoo Finance is now running

## 📊 Verify It's Working

```bash
# Check logs
docker-compose logs traderview-server | grep -i yahoo

# Should see:
# "Using market data service: YahooFinance"
```

## 🔄 Switch to Financial Modeling Prep

Edit `.env`:
```bash
MARKET_DATA_SERVICE=fmp
FMP_API_KEY=your_actual_api_key
```

Restart:
```bash
docker-compose restart traderview-server
```

## 📍 Access Your App

- **Frontend**: http://localhost:3000
- **API**: http://localhost:5000

## 🛑 Stop Everything

```bash
docker-compose down
```

## 📖 Full Documentation

See `DOCKER_MARKET_DATA_CONFIGURATION.md` for complete guide.

## 💡 Pro Tips

1. **No API Key Needed** - Yahoo Finance is free, no registration required
2. **Fast Switching** - Change `MARKET_DATA_SERVICE` in .env and restart
3. **Both Available** - Keep both configs, switch anytime
4. **Check Logs** - Use `docker-compose logs -f` to monitor in real-time

## ⚠️ Important

- Never commit `.env` file (already in .gitignore ✓)
- Use strong passwords for `SQL_PASSWORD`
- Yahoo Finance has no economic calendar API

## 🆘 Having Issues?

```bash
# Rebuild everything
docker-compose down
docker-compose build --no-cache
docker-compose up -d

# Check what's wrong
docker-compose logs traderview-server
```
