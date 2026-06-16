-- =============================================
-- Diagnostic Script - Identify Date Column Type Mismatches
-- Description: Shows all date-related columns and their types
-- =============================================

USE TradingBE;
GO

PRINT '==============================================';
PRINT 'Date Column Type Diagnostic Report';
PRINT '==============================================';
PRINT '';

-- =============================================
-- OpenPositions Table - Date Columns
-- =============================================
PRINT '1. OpenPositions Table - Date-related Columns:';
PRINT '----------------------------------------------';

SELECT 
	c.name AS ColumnName,
	t.name AS CurrentDataType,
	CASE 
		WHEN c.max_length = -1 THEN 'MAX'
		WHEN t.name IN ('nvarchar', 'nchar') THEN CAST(c.max_length/2 AS VARCHAR(10))
		ELSE CAST(c.max_length AS VARCHAR(10))
	END AS Size,
	CASE c.is_nullable WHEN 1 THEN 'NULL' ELSE 'NOT NULL' END AS Nullable,
	CASE 
		WHEN c.name = 'whenGenerated' AND t.name = 'datetime2' THEN 'OK'
		WHEN c.name = 'reportDate' AND t.name = 'datetime2' THEN 'OK'
		WHEN c.name = 'vestingDate' AND t.name = 'datetime2' THEN 'OK'
		WHEN c.name = 'openDateTime' AND t.name = 'nvarchar' THEN 'OK'
		WHEN c.name = 'holdingPeriodDateTime' AND t.name = 'nvarchar' THEN 'OK'
		WHEN c.name = 'expiry' AND t.name = 'nvarchar' THEN 'OK'
		ELSE '*** MISMATCH ***'
	END AS Status,
	CASE 
		WHEN c.name = 'whenGenerated' THEN 'Should be DATETIME2 (DateTime)'
		WHEN c.name = 'reportDate' THEN 'Should be DATETIME2 (DateTime?)'
		WHEN c.name = 'vestingDate' THEN 'Should be DATETIME2 (DateTime?)'
		WHEN c.name = 'openDateTime' THEN 'Should be NVARCHAR(50) (string)'
		WHEN c.name = 'holdingPeriodDateTime' THEN 'Should be NVARCHAR(50) (string)'
		WHEN c.name = 'expiry' THEN 'Should be NVARCHAR(50) (string)'
		ELSE ''
	END AS ExpectedType
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'[dbo].[OpenPositions]')
AND c.name IN ('whenGenerated', 'reportDate', 'vestingDate', 'openDateTime', 'holdingPeriodDateTime', 'expiry')
ORDER BY 
	CASE c.name
		WHEN 'whenGenerated' THEN 1
		WHEN 'reportDate' THEN 2
		WHEN 'vestingDate' THEN 3
		WHEN 'openDateTime' THEN 4
		WHEN 'holdingPeriodDateTime' THEN 5
		WHEN 'expiry' THEN 6
	END;

PRINT '';

-- =============================================
-- TradeExecutions Table - Date Columns
-- =============================================
PRINT '2. TradeExecutions Table - Date-related Columns:';
PRINT '------------------------------------------------';

SELECT 
	c.name AS ColumnName,
	t.name AS CurrentDataType,
	CASE 
		WHEN c.max_length = -1 THEN 'MAX'
		WHEN t.name IN ('nvarchar', 'nchar') THEN CAST(c.max_length/2 AS VARCHAR(10))
		ELSE CAST(c.max_length AS VARCHAR(10))
	END AS Size,
	CASE c.is_nullable WHEN 1 THEN 'NULL' ELSE 'NOT NULL' END AS Nullable,
	CASE 
		WHEN t.name = 'nvarchar' THEN 'OK'
		ELSE '*** MISMATCH - Should be NVARCHAR ***'
	END AS Status
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'[dbo].[TradeExecutions]')
AND c.name IN ('dateTime', 'tradeDate', 'origTradeDate', 'openDateTime', 'holdingPeriodDateTime', 
			   'whenRealized', 'whenReopened', 'orderTime', 'settleDateTarget', 'expiry', 'reportDate')
ORDER BY c.name;

PRINT '';

-- =============================================
-- Summary
-- =============================================
PRINT '==============================================';
PRINT 'Summary of Potential Issues:';
PRINT '==============================================';

-- Count mismatches in OpenPositions
DECLARE @OpenPosMismatches INT;
SELECT @OpenPosMismatches = COUNT(*)
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'[dbo].[OpenPositions]')
AND (
	(c.name IN ('openDateTime', 'holdingPeriodDateTime', 'expiry') AND t.name NOT IN ('nvarchar', 'varchar'))
	OR
	(c.name IN ('whenGenerated', 'reportDate', 'vestingDate') AND t.name NOT IN ('datetime', 'datetime2'))
);

-- Count mismatches in TradeExecutions
DECLARE @TradeExecMismatches INT;
SELECT @TradeExecMismatches = COUNT(*)
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'[dbo].[TradeExecutions]')
AND c.name IN ('dateTime', 'tradeDate', 'origTradeDate', 'openDateTime', 'holdingPeriodDateTime', 
			   'whenRealized', 'whenReopened', 'orderTime', 'settleDateTarget', 'expiry', 'reportDate')
AND t.name NOT IN ('nvarchar', 'varchar');

PRINT 'OpenPositions mismatches found: ' + CAST(@OpenPosMismatches AS VARCHAR(10));
PRINT 'TradeExecutions mismatches found: ' + CAST(@TradeExecMismatches AS VARCHAR(10));
PRINT '';

IF @OpenPosMismatches > 0 OR @TradeExecMismatches > 0
BEGIN
	PRINT 'ACTION REQUIRED: Run FixDateColumnTypes.sql to correct the mismatches.';
END
ELSE
BEGIN
	PRINT 'No date column type mismatches found!';
	PRINT 'The error may be caused by something else. Check:';
	PRINT '  1. Application error logs for the exact line number';
	PRINT '  2. Any data conversion in the application code';
	PRINT '  3. Other tables that might have date columns';
END

PRINT '==============================================';
GO
