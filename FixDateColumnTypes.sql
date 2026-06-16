-- =============================================
-- SQL Server Fix Script for Date Type Mismatches
-- Description: Fixes DateTime vs String column type issues
-- =============================================

USE TradingBE;
GO

PRINT '==============================================';
PRINT 'Fixing Date Column Type Mismatches...';
PRINT '==============================================';
PRINT '';

-- =============================================
-- Fix OpenPositions Table
-- =============================================
PRINT 'Checking OpenPositions table for date column mismatches...';

-- Check if openDateTime is DATETIME type (needs to be NVARCHAR)
IF EXISTS (
	SELECT * FROM sys.columns c
	INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
	WHERE c.object_id = OBJECT_ID(N'[dbo].[OpenPositions]') 
	AND c.name = 'openDateTime'
	AND t.name IN ('datetime', 'datetime2')
)
BEGIN
	PRINT '  - openDateTime is currently DATETIME type, changing to NVARCHAR(50)...';

	-- Drop dependent indexes if they exist
	IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OpenPositions_OpenDateTime' AND object_id = OBJECT_ID(N'[dbo].[OpenPositions]'))
	BEGIN
		DROP INDEX IX_OpenPositions_OpenDateTime ON [dbo].[OpenPositions];
		PRINT '    - Dropped index IX_OpenPositions_OpenDateTime';
	END

	-- Alter column type
	ALTER TABLE [dbo].[OpenPositions] 
	ALTER COLUMN [openDateTime] NVARCHAR(50) NULL;

	PRINT '    - Changed openDateTime to NVARCHAR(50)';
END
ELSE
BEGIN
	PRINT '  - openDateTime is already NVARCHAR type';
END

-- Check if holdingPeriodDateTime is DATETIME type (needs to be NVARCHAR)
IF EXISTS (
	SELECT * FROM sys.columns c
	INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
	WHERE c.object_id = OBJECT_ID(N'[dbo].[OpenPositions]') 
	AND c.name = 'holdingPeriodDateTime'
	AND t.name IN ('datetime', 'datetime2')
)
BEGIN
	PRINT '  - holdingPeriodDateTime is currently DATETIME type, changing to NVARCHAR(50)...';

	-- Drop dependent indexes if they exist
	IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OpenPositions_HoldingPeriodDateTime' AND object_id = OBJECT_ID(N'[dbo].[OpenPositions]'))
	BEGIN
		DROP INDEX IX_OpenPositions_HoldingPeriodDateTime ON [dbo].[OpenPositions];
		PRINT '    - Dropped index IX_OpenPositions_HoldingPeriodDateTime';
	END

	-- Alter column type
	ALTER TABLE [dbo].[OpenPositions] 
	ALTER COLUMN [holdingPeriodDateTime] NVARCHAR(50) NULL;

	PRINT '    - Changed holdingPeriodDateTime to NVARCHAR(50)';
END
ELSE
BEGIN
	PRINT '  - holdingPeriodDateTime is already NVARCHAR type';
END

GO

-- =============================================
-- Verify Column Types
-- =============================================
PRINT '';
PRINT 'Verifying column types in OpenPositions table...';
PRINT '';

SELECT 
	c.name AS ColumnName,
	t.name AS DataType,
	c.max_length AS MaxLength,
	c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'[dbo].[OpenPositions]')
AND c.name IN ('whenGenerated', 'reportDate', 'vestingDate', 'openDateTime', 'holdingPeriodDateTime', 'expiry')
ORDER BY c.column_id;

GO

-- =============================================
-- Check TradeExecutions table date columns
-- =============================================
PRINT '';
PRINT 'Verifying date columns in TradeExecutions table...';
PRINT '(All date columns in TradeExecutions should be NVARCHAR)';
PRINT '';

SELECT 
	c.name AS ColumnName,
	t.name AS DataType,
	c.max_length AS MaxLength
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'[dbo].[TradeExecutions]')
AND c.name IN ('dateTime', 'tradeDate', 'origTradeDate', 'openDateTime', 'holdingPeriodDateTime', 
			   'whenRealized', 'whenReopened', 'orderTime', 'settleDateTarget', 'expiry', 'reportDate')
ORDER BY c.name;

GO

PRINT '';
PRINT '==============================================';
PRINT 'Date Column Type Fix Completed!';
PRINT '==============================================';
PRINT '';
PRINT 'Expected column types:';
PRINT '  OpenPositions.openDateTime = NVARCHAR(50) (string in C#)';
PRINT '  OpenPositions.holdingPeriodDateTime = NVARCHAR(50) (string in C#)';
PRINT '  OpenPositions.reportDate = DATETIME2 (DateTime? in C#)';
PRINT '  OpenPositions.vestingDate = DATETIME2 (DateTime? in C#)';
PRINT '  OpenPositions.whenGenerated = DATETIME2 (DateTime in C#)';
PRINT '';
PRINT 'Please try running your application again.';
PRINT '==============================================';
GO
