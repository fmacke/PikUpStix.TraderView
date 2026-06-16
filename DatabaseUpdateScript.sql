-- =============================================
-- SQL Server Database Update Script for TradingBE
-- Description: Updates existing database schema without data loss
-- This script adds missing columns and indexes to existing tables
-- =============================================

USE TradingBE;
GO

PRINT '==============================================';
PRINT 'Starting TradingBE Database Update...';
PRINT '==============================================';
PRINT '';

-- =============================================
-- Update Instruments Table
-- =============================================
PRINT 'Checking Instruments table...';

-- Add ConId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Instruments]') AND name = 'ConId')
BEGIN
	ALTER TABLE [dbo].[Instruments] ADD [ConId] NVARCHAR(50) NULL;
	PRINT '  - Added column ConId';
END
ELSE
BEGIN
	PRINT '  - Column ConId already exists';
END

-- Create unique index on ConId if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Instruments]') AND name = 'IX_Instruments_ConId')
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX [IX_Instruments_ConId] 
	ON [dbo].[Instruments] ([ConId]) 
	WHERE [ConId] IS NOT NULL;
	PRINT '  - Created index IX_Instruments_ConId';
END
ELSE
BEGIN
	PRINT '  - Index IX_Instruments_ConId already exists';
END

-- Create index on InstrumentName if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Instruments]') AND name = 'IX_Instruments_InstrumentName')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Instruments_InstrumentName] 
	ON [dbo].[Instruments] ([InstrumentName]);
	PRINT '  - Created index IX_Instruments_InstrumentName';
END
ELSE
BEGIN
	PRINT '  - Index IX_Instruments_InstrumentName already exists';
END

GO

-- =============================================
-- Update TradeExecutions Table
-- =============================================
PRINT 'Checking TradeExecutions table...';

-- Ensure all required indexes exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TradeExecutions]') AND name = 'IX_TradeExecutions_IbExecID')
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX [IX_TradeExecutions_IbExecID] 
	ON [dbo].[TradeExecutions] ([ibExecID]) 
	WHERE [ibExecID] IS NOT NULL;
	PRINT '  - Created index IX_TradeExecutions_IbExecID';
END
ELSE
BEGIN
	PRINT '  - Index IX_TradeExecutions_IbExecID already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TradeExecutions]') AND name = 'IX_TradeExecutions_IbOrderID')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_TradeExecutions_IbOrderID] 
	ON [dbo].[TradeExecutions] ([ibOrderID]);
	PRINT '  - Created index IX_TradeExecutions_IbOrderID';
END
ELSE
BEGIN
	PRINT '  - Index IX_TradeExecutions_IbOrderID already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TradeExecutions]') AND name = 'IX_TradeExecutions_TradeDate')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_TradeExecutions_TradeDate] 
	ON [dbo].[TradeExecutions] ([tradeDate]);
	PRINT '  - Created index IX_TradeExecutions_TradeDate';
END
ELSE
BEGIN
	PRINT '  - Index IX_TradeExecutions_TradeDate already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TradeExecutions]') AND name = 'IX_TradeExecutions_Symbol')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_TradeExecutions_Symbol] 
	ON [dbo].[TradeExecutions] ([symbol]);
	PRINT '  - Created index IX_TradeExecutions_Symbol';
END
ELSE
BEGIN
	PRINT '  - Index IX_TradeExecutions_Symbol already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TradeExecutions]') AND name = 'IX_TradeExecutions_InstrumentId')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_TradeExecutions_InstrumentId] 
	ON [dbo].[TradeExecutions] ([InstrumentId]);
	PRINT '  - Created index IX_TradeExecutions_InstrumentId';
END
ELSE
BEGIN
	PRINT '  - Index IX_TradeExecutions_InstrumentId already exists';
END

GO

-- =============================================
-- Update OpenPositions Table
-- =============================================
PRINT 'Checking OpenPositions table...';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OpenPositions]') AND name = 'IX_OpenPositions_WhenGenerated')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_OpenPositions_WhenGenerated] 
	ON [dbo].[OpenPositions] ([whenGenerated] DESC);
	PRINT '  - Created index IX_OpenPositions_WhenGenerated';
END
ELSE
BEGIN
	PRINT '  - Index IX_OpenPositions_WhenGenerated already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OpenPositions]') AND name = 'IX_OpenPositions_Symbol')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_OpenPositions_Symbol] 
	ON [dbo].[OpenPositions] ([symbol]);
	PRINT '  - Created index IX_OpenPositions_Symbol';
END
ELSE
BEGIN
	PRINT '  - Index IX_OpenPositions_Symbol already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OpenPositions]') AND name = 'IX_OpenPositions_Conid')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_OpenPositions_Conid] 
	ON [dbo].[OpenPositions] ([conid]) 
	WHERE [conid] IS NOT NULL;
	PRINT '  - Created index IX_OpenPositions_Conid';
END
ELSE
BEGIN
	PRINT '  - Index IX_OpenPositions_Conid already exists';
END

GO

-- =============================================
-- Update HistoricalData Table
-- =============================================
PRINT 'Checking HistoricalData table...';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[HistoricalData]') AND name = 'IX_HistoricalData_Date')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_HistoricalData_Date] 
	ON [dbo].[HistoricalData] ([Date] DESC);
	PRINT '  - Created index IX_HistoricalData_Date';
END
ELSE
BEGIN
	PRINT '  - Index IX_HistoricalData_Date already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[HistoricalData]') AND name = 'IX_HistoricalData_InstrumentId')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_HistoricalData_InstrumentId] 
	ON [dbo].[HistoricalData] ([InstrumentId], [Date] DESC);
	PRINT '  - Created index IX_HistoricalData_InstrumentId';
END
ELSE
BEGIN
	PRINT '  - Index IX_HistoricalData_InstrumentId already exists';
END

GO

-- =============================================
-- Update EconomicCalendar Table
-- =============================================
PRINT 'Checking EconomicCalendar table...';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[EconomicCalendar]') AND name = 'IX_EconomicCalendar_Date')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_EconomicCalendar_Date]
	ON [dbo].[EconomicCalendar] ([Date] DESC);
	PRINT '  - Created index IX_EconomicCalendar_Date';
END
ELSE
BEGIN
	PRINT '  - Index IX_EconomicCalendar_Date already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[EconomicCalendar]') AND name = 'IX_EconomicCalendar_Country')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_EconomicCalendar_Country]
	ON [dbo].[EconomicCalendar] ([Country]);
	PRINT '  - Created index IX_EconomicCalendar_Country';
END
ELSE
BEGIN
	PRINT '  - Index IX_EconomicCalendar_Country already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[EconomicCalendar]') AND name = 'IX_EconomicCalendar_Impact')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_EconomicCalendar_Impact]
	ON [dbo].[EconomicCalendar] ([Impact])
	WHERE [Impact] IS NOT NULL;
	PRINT '  - Created index IX_EconomicCalendar_Impact';
END
ELSE
BEGIN
	PRINT '  - Index IX_EconomicCalendar_Impact already exists';
END

GO

-- =============================================
-- Verify Foreign Keys
-- =============================================
PRINT '';
PRINT 'Checking foreign keys...';

-- FK: TradeExecutions -> Instruments
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TradeExecutions_Instruments')
BEGIN
	ALTER TABLE [dbo].[TradeExecutions]
	ADD CONSTRAINT [FK_TradeExecutions_Instruments] FOREIGN KEY ([InstrumentId]) 
		REFERENCES [dbo].[Instruments] ([Id]);
	PRINT '  - Created FK_TradeExecutions_Instruments';
END
ELSE
BEGIN
	PRINT '  - FK_TradeExecutions_Instruments already exists';
END

-- FK: HistoricalData -> Instruments
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_HistoricalData_Instruments')
BEGIN
	ALTER TABLE [dbo].[HistoricalData]
	ADD CONSTRAINT [FK_HistoricalData_Instruments] FOREIGN KEY ([InstrumentId]) 
		REFERENCES [dbo].[Instruments] ([Id]);
	PRINT '  - Created FK_HistoricalData_Instruments';
END
ELSE
BEGIN
	PRINT '  - FK_HistoricalData_Instruments already exists';
END

GO

-- =============================================
-- Summary Report
-- =============================================
PRINT '';
PRINT '==============================================';
PRINT 'Database Update Completed Successfully!';
PRINT '==============================================';
PRINT '';
PRINT 'Database: TradingBE';
PRINT '';
PRINT 'Table Summary:';

SELECT 
	t.name AS TableName,
	COUNT(c.column_id) AS ColumnCount,
	COUNT(DISTINCT i.index_id) AS IndexCount
FROM sys.tables t
LEFT JOIN sys.columns c ON t.object_id = c.object_id
LEFT JOIN sys.indexes i ON t.object_id = i.object_id AND i.type > 0
WHERE t.name IN ('Instruments', 'TradeExecutions', 'OpenPositions', 'HistoricalData', 'EconomicCalendar')
GROUP BY t.name
ORDER BY t.name;

PRINT '';
PRINT 'Record Counts:';

SELECT 'Instruments' AS TableName, COUNT(*) AS RecordCount FROM [dbo].[Instruments]
UNION ALL
SELECT 'TradeExecutions', COUNT(*) FROM [dbo].[TradeExecutions]
UNION ALL
SELECT 'OpenPositions', COUNT(*) FROM [dbo].[OpenPositions]
UNION ALL
SELECT 'HistoricalData', COUNT(*) FROM [dbo].[HistoricalData]
UNION ALL
SELECT 'EconomicCalendar', COUNT(*) FROM [dbo].[EconomicCalendar]
ORDER BY TableName;

GO
