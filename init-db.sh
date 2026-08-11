#!/bin/bash

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to start..."
sleep 30

# Run the database creation script
echo "Initializing database..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${MSSQL_SA_PASSWORD}" -C -d master -Q "
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'TradingBE')
BEGIN
	CREATE DATABASE TradingBE;
	PRINT 'Database TradingBE created successfully.';
END
"

echo "Database initialization complete!"
