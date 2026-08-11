-- =============================================
-- SQL Server Database Deletion Script for TradingBE
-- WARNING: This script will permanently DELETE the TradingBE database and ALL its data!
-- Description: Safely drops the TradingBE database after disconnecting all users
-- =============================================

USE master;
GO

PRINT '==============================================';
PRINT 'TradingBE Database Deletion Script';
PRINT 'WARNING: This will DELETE ALL DATA!';
PRINT '==============================================';
PRINT '';

-- Check if databaseExists
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'TradingBE')
BEGIN
	PRINT 'Database TradingBE found. Proceeding with deletion...';
	PRINT '';

	-- Set database to single user mode to disconnect all users
	PRINT 'Step 1: Disconnecting all active connections...';
	ALTER DATABASE TradingBE SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	PRINT '  - All connections closed';
	PRINT '';

	-- Drop the database
	PRINT 'Step 2: Dropping database...';
	DROP DATABASE TradingBE;
	PRINT '  - Database TradingBE has been deleted successfully';
	PRINT '';

	PRINT '==============================================';
	PRINT 'Database deletion completed!';
	PRINT '==============================================';
END
ELSE
BEGIN
	PRINT 'Database TradingBE does not exist. Nothing to delete.';
	PRINT '';
END
GO
