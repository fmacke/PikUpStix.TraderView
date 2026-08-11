#!/bin/bash

# Bash script to setup and initialize the TraderView database in Docker

set -e

echo -e "\033[36mTraderView Database Setup Script\033[0m"
echo -e "\033[36m=================================\033[0m"
echo ""

# Check if Docker is running
echo -e "\033[33mChecking Docker status...\033[0m"
if ! docker version &> /dev/null; then
	echo -e "\033[31m✗ Docker is not running. Please start Docker and try again.\033[0m"
	exit 1
fi
echo -e "\033[32m✓ Docker is running\033[0m"

# Check if .env file exists
if [ ! -f ".env" ]; then
	echo -e "\033[33mCreating .env file with default values...\033[0m"

	cat > .env << 'EOF'
# SQL Server Configuration
SQL_PASSWORD=YourStrong!Passw0rd

# IBKR Configuration (add your values)
IBKR_TOKEN=your_token_here
IBKR_QUERY_ID=your_query_id_here
IBKR_TODAY_EXEC_ID=your_today_exec_id_here

# Financial Modeling Prep API (add your API key)
FMP_API_KEY=your_fmp_api_key_here

# Market Data Service (yahoo or fmp)
MARKET_DATA_SERVICE=fmp

# Yahoo Finance (optional)
YAHOO_FINANCE_BASE_URL=https://query1.finance.yahoo.com
EOF

	echo -e "\033[32m✓ Created .env file. Please update it with your configuration values.\033[0m"
	echo ""
fi

# Start the SQL Server container
echo -e "\033[33mStarting SQL Server container...\033[0m"
docker-compose up -d sqlserver

echo ""
echo -e "\033[33mWaiting for SQL Server to be ready...\033[0m"
sleep 10

# Wait for SQL Server to be healthy
MAX_ATTEMPTS=30
ATTEMPT=0
IS_HEALTHY=false

while [ $ATTEMPT -lt $MAX_ATTEMPTS ] && [ "$IS_HEALTHY" = false ]; do
	ATTEMPT=$((ATTEMPT + 1))
	echo -e "\033[33mChecking SQL Server health (attempt $ATTEMPT/$MAX_ATTEMPTS)...\033[0m"

	CONTAINER_HEALTH=$(docker inspect --format='{{.State.Health.Status}}' traderview-sqlserver 2>/dev/null || echo "unknown")

	if [ "$CONTAINER_HEALTH" = "healthy" ]; then
		IS_HEALTHY=true
		echo -e "\033[32m✓ SQL Server is healthy!\033[0m"
	else
		sleep 2
	fi
done

if [ "$IS_HEALTHY" = false ]; then
	echo -e "\033[31m✗ SQL Server did not become healthy in time. Check Docker logs:\033[0m"
	echo -e "\033[33m  docker logs traderview-sqlserver\033[0m"
	exit 1
fi

# Read the SQL password from .env file
SQL_PASSWORD="YourStrong!Passw0rd"
if [ -f ".env" ]; then
	if grep -q "SQL_PASSWORD=" .env; then
		SQL_PASSWORD=$(grep "SQL_PASSWORD=" .env | cut -d '=' -f2-)
	fi
fi

echo ""
echo -e "\033[33mRunning database creation script...\033[0m"

# Copy and execute the database creation script
docker cp DatabaseCreationScript.sql traderview-sqlserver:/tmp/DatabaseCreationScript.sql

docker exec traderview-sqlserver /opt/mssql-tools18/bin/sqlcmd \
	-S localhost \
	-U sa \
	-P "$SQL_PASSWORD" \
	-C \
	-i /tmp/DatabaseCreationScript.sql

if [ $? -eq 0 ]; then
	echo -e "\033[32m✓ Database schema created successfully!\033[0m"
else
	echo -e "\033[31m✗ Error creating database schema. Check the output above.\033[0m"
	exit 1
fi

echo ""
echo -e "\033[36m=================================\033[0m"
echo -e "\033[32mDatabase Setup Complete!\033[0m"
echo -e "\033[36m=================================\033[0m"
echo ""
echo -e "\033[33mConnection Details:\033[0m"
echo -e "\033[37m  Server: localhost,1433\033[0m"
echo -e "\033[37m  Database: TradingBE\033[0m"
echo -e "\033[37m  Username: sa\033[0m"
echo -e "\033[37m  Password: $SQL_PASSWORD\033[0m"
echo ""
echo -e "\033[33mConnection String:\033[0m"
echo -e "\033[37m  Server=localhost,1433;Database=TradingBE;User Id=sa;Password=$SQL_PASSWORD;TrustServerCertificate=true;Encrypt=false;\033[0m"
echo ""
echo -e "\033[33mTo connect from your application, use:\033[0m"
echo -e "\033[37m  Update your User Secrets with the database credentials\033[0m"
echo ""
echo -e "\033[33mTo stop the database:\033[0m"
echo -e "\033[37m  docker-compose down\033[0m"
echo ""
echo -e "\033[33mTo view database logs:\033[0m"
echo -e "\033[37m  docker logs traderview-sqlserver\033[0m"
echo ""
