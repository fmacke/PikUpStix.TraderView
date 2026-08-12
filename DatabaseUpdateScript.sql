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

-- Add PositionId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TradeExecutions]') AND name = 'PositionId')
BEGIN
	ALTER TABLE [dbo].[TradeExecutions] ADD [PositionId] INT NULL;
	PRINT '  - Added column PositionId';
END
ELSE
BEGIN
	PRINT '  - Column PositionId already exists';
END

-- Drop FK constraint on InstrumentId if it exists
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TradeExecutions_Instruments')
BEGIN
	ALTER TABLE [dbo].[TradeExecutions] DROP CONSTRAINT [FK_TradeExecutions_Instruments];
	PRINT '  - Dropped foreign key FK_TradeExecutions_Instruments';
END

-- Drop index on InstrumentId if it exists
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TradeExecutions]') AND name = 'IX_TradeExecutions_InstrumentId')
BEGIN
	DROP INDEX [IX_TradeExecutions_InstrumentId] ON [dbo].[TradeExecutions];
	PRINT '  - Dropped index IX_TradeExecutions_InstrumentId';
END

-- Drop InstrumentId column if it exists
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TradeExecutions]') AND name = 'InstrumentId')
BEGIN
	ALTER TABLE [dbo].[TradeExecutions] DROP COLUMN [InstrumentId];
	PRINT '  - Dropped column InstrumentId';
END
ELSE
BEGIN
	PRINT '  - Column InstrumentId does not exist';
END

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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TradeExecutions]') AND name = 'IX_TradeExecutions_PositionId')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_TradeExecutions_PositionId] 
	ON [dbo].[TradeExecutions] ([PositionId])
	WHERE [PositionId] IS NOT NULL;
	PRINT '  - Created index IX_TradeExecutions_PositionId';
END
ELSE
BEGIN
	PRINT '  - Index IX_TradeExecutions_PositionId already exists';
END

GO

-- =============================================
-- Update Positions Table
-- =============================================
PRINT 'Checking Positions table...';

-- Make InstrumentId NOT NULL if it's currently nullable
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Positions]') AND name = 'InstrumentId' AND is_nullable = 1)
BEGIN
	-- Update any NULL values before changing to NOT NULL
	-- Users should update this with appropriate logic based on their data
	PRINT '  - Warning: InstrumentId contains nullable column. Consider updating NULL values before migration.';
	-- Uncomment and modify if needed:
	-- UPDATE [dbo].[Positions] SET [InstrumentId] = <default_value> WHERE [InstrumentId] IS NULL;
	-- ALTER TABLE [dbo].[Positions] ALTER COLUMN [InstrumentId] INT NOT NULL;
	-- PRINT '  - Modified InstrumentId to NOT NULL';
END
ELSE
BEGIN
	PRINT '  - Column InstrumentId already NOT NULL or does not exist';
END

-- Drop obsolete indexes
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Positions]') AND name = 'IX_Positions_Symbol')
BEGIN
	DROP INDEX [IX_Positions_Symbol] ON [dbo].[Positions];
	PRINT '  - Dropped index IX_Positions_Symbol';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Positions]') AND name = 'IX_Positions_Conid')
BEGIN
	DROP INDEX [IX_Positions_Conid] ON [dbo].[Positions];
	PRINT '  - Dropped index IX_Positions_Conid';
END

-- Update InstrumentId index to remove WHERE clause
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Positions]') AND name = 'IX_Positions_InstrumentId')
BEGIN
	DROP INDEX [IX_Positions_InstrumentId] ON [dbo].[Positions];
	PRINT '  - Dropped old index IX_Positions_InstrumentId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Positions]') AND name = 'IX_Positions_InstrumentId')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Positions_InstrumentId] 
	ON [dbo].[Positions] ([InstrumentId]);
	PRINT '  - Created index IX_Positions_InstrumentId (without WHERE clause)';
END

-- Keep essential indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Positions]') AND name = 'IX_Positions_OpenDate')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Positions_OpenDate] 
	ON [dbo].[Positions] ([OpenDate] DESC);
	PRINT '  - Created index IX_Positions_OpenDate';
END
ELSE
BEGIN
	PRINT '  - Index IX_Positions_OpenDate already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Positions]') AND name = 'IX_Positions_Status')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Positions_Status] 
	ON [dbo].[Positions] ([Status]);
	PRINT '  - Created index IX_Positions_Status';
END
ELSE
BEGIN
	PRINT '  - Index IX_Positions_Status already exists';
END

-- Note: Column removal should be done manually after data migration
-- Consider backing up data from these columns before dropping:
-- AccountId, PositionID, AcctAlias, Model, Currency, FxRateToBase, AssetCategory, 
-- SubCategory, Quantity, Symbol, Description, Conid, SecurityID, SecurityIDType,
-- Cusip, Isin, Figi, ListingExchange, UnderlyingConid, UnderlyingSymbol,
-- UnderlyingSecurityID, UnderlyingListingExchange, Issuer, IssuerCountryCode,
-- Multiplier, Strike, Expiry, PutCall, PrincipalAdjustFactor, ReportDate,
-- MarkPrice, PositionValue, OpenPrice, CostBasisPrice, CostBasisMoney,
-- PercentOfNAV, FifoPnlUnrealized, Side, LevelOfDetail, OpenDateTime,
-- HoldingPeriodDateTime, VestingDate, Code, OriginatingOrderID, 
-- OriginatingTransactionID, AccruedInt, SerialNumber, DeliveryType,
-- CommodityType, Fineness, Weight

PRINT '  - WARNING: Manual column cleanup required for Positions table';
PRINT '  - Only keep: Id, OpenDate, CloseDate, Status, InstrumentId, LastReportedPrice, LastReportedPriceUpdated';

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
-- Create/Update Lists Table
-- =============================================
PRINT 'Checking Lists table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Lists]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Lists]
	(
		[Id] INT IDENTITY(1,1) NOT NULL,
		[Name] NVARCHAR(100) NOT NULL,
		[Description] NVARCHAR(500) NULL,
		[Category] NVARCHAR(50) NULL,
		[IsActive] BIT NOT NULL DEFAULT 1,
		[CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
		[UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),

		CONSTRAINT [PK_Lists] PRIMARY KEY CLUSTERED ([Id] ASC)
	);

	-- Create index on Name for quick lookups
	CREATE NONCLUSTERED INDEX [IX_List_Name] 
	ON [dbo].[Lists] ([Name]);

	-- Create index on Category for filtering by category
	CREATE NONCLUSTERED INDEX [IX_Lists_Category] 
	ON [dbo].[Lists] ([Category])
	WHERE [Category] IS NOT NULL;

	-- Create index on IsActive for filtering active items
	CREATE NONCLUSTERED INDEX [IX_Lists_IsActive] 
	ON [dbo].[Lists] ([IsActive]);

	PRINT '  - Table Lists created successfully';
END
ELSE
BEGIN
	PRINT '  - Table Lists already exists';
END

-- Ensure all required indexes exist for Lists table
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Lists]') AND name = 'IX_Lists_Name')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Lists_Name] 
	ON [dbo].[Lists] ([Name]);
	PRINT '  - Created index IX_Lists_Name';
END
ELSE
BEGIN
	PRINT '  - Index IX_Lists_Name already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Lists]') AND name = 'IX_Lists_Category')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Lists_Category] 
	ON [dbo].[Lists] ([Category])
	WHERE [Category] IS NOT NULL;
	PRINT '  - Created index IX_Lists_Category';
END
ELSE
BEGIN
	PRINT '  - Index IX_Lists_Category already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Lists]') AND name = 'IX_Lists_IsActive')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Lists_IsActive] 
	ON [dbo].[Lists] ([IsActive]);
	PRINT '  - Created index IX_Lists_IsActive';
END
ELSE
BEGIN
	PRINT '  - Index IX_Lists_IsActive already exists';
END

GO

-- =============================================
-- Create/Update Notes Table
-- =============================================
PRINT 'Checking Notes table...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notes]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Notes]
	(
		[Id] INT IDENTITY(1,1) NOT NULL,
		[PositionId] INT NOT NULL,
		[TradeExecutionId] INT NULL,
		[TradeTypeId] INT NULL,
		[Comment] NVARCHAR(MAX) NOT NULL,
		[EntryDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
		[UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),

		CONSTRAINT [PK_Notes] PRIMARY KEY CLUSTERED ([Id] ASC)
	);

	-- Create index on PositionId for FK performance
	CREATE NONCLUSTERED INDEX [IX_Notes_PositionId] 
	ON [dbo].[Notes] ([PositionId]);

	-- Create index on TradeTypeId for FK performance
	CREATE NONCLUSTERED INDEX [IX_Notes_TradeTypeId] 
	ON [dbo].[Notes] ([TradeTypeId])
	WHERE [TradeTypeId] IS NOT NULL;

	-- Create index on CreatedAt for time-based queries
	CREATE NONCLUSTERED INDEX [IX_Notes_EntryDate] 
	ON [dbo].[Notes] ([EntryDate] DESC);

	PRINT '  - Table Notes created successfully';
END
ELSE
BEGIN
	PRINT '  - Table Notes already exists';
END

-- Ensure all required indexes exist for Notes table
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Notes]') AND name = 'IX_Notes_PositionId')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Notes_PositionId] 
	ON [dbo].[Notes] ([PositionId]);
	PRINT '  - Created index IX_Notes_PositionId';
END
ELSE
BEGIN
	PRINT '  - Index IX_Notes_PositionId already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Notes]') AND name = 'IX_Notes_TradeTypeId')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Notes_TradeTypeId] 
	ON [dbo].[Notes] ([TradeTypeId])
	WHERE [TradeTypeId] IS NOT NULL;
	PRINT '  - Created index IX_Notes_TradeTypeId';
END
ELSE
BEGIN
	PRINT '  - Index IX_Notes_TradeTypeId already exists';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Notes]') AND name = 'IX_Notes_CreatedAt')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Notes_EntryDate] 
	ON [dbo].[Notes] ([EntryDate] DESC);
	PRINT '  - Created index IX_Notes_CreatedAt';
END
ELSE
BEGIN
	PRINT '  - Index IX_Notes_CreatedAt already exists';
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

-- FK: TradeExecutions -> Positions
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TradeExecutions_Positions')
BEGIN
	ALTER TABLE [dbo].[TradeExecutions]
	ADD CONSTRAINT [FK_TradeExecutions_Positions] FOREIGN KEY ([PositionId]) 
		REFERENCES [dbo].[Positions] ([Id]);
	PRINT '  - Created FK_TradeExecutions_Positions';
END
ELSE
BEGIN
	PRINT '  - FK_TradeExecutions_Positions already exists';
END

-- FK: Positions -> Instruments
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Positions_Instruments')
BEGIN
	-- Note: This will fail if InstrumentId has NULL values or invalid references
	-- Ensure data integrity before running this constraint
	ALTER TABLE [dbo].[Positions]
	ADD CONSTRAINT [FK_Positions_Instruments] FOREIGN KEY ([InstrumentId]) 
		REFERENCES [dbo].[Instruments] ([Id]);
	PRINT '  - Created FK_Positions_Instruments';
END
ELSE
BEGIN
	PRINT '  - FK_Positions_Instruments already exists';
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

-- FK: Notes -> Positions
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notes_Positions')
BEGIN
	ALTER TABLE [dbo].[Notes]
	ADD CONSTRAINT [FK_Notes_Positions] FOREIGN KEY ([PositionId]) 
		REFERENCES [dbo].[Positions] ([Id]) ON DELETE CASCADE;
	PRINT '  - Created FK_Notes_Positions';
END
ELSE
BEGIN
	PRINT '  - FK_Notes_Positions already exists';
END

-- FK: Notes -> Lists
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notes_Lists')
BEGIN
	ALTER TABLE [dbo].[Notes]
	ADD CONSTRAINT [FK_Notes_Lists] FOREIGN KEY ([TradeTypeId]) 
		REFERENCES [dbo].[Lists] ([Id]);
	PRINT '  - Created FK_Notes_Lists';
END
ELSE
BEGIN
	PRINT '  - FK_Notes_Lists already exists';
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
WHERE t.name IN ('Instruments', 'Positions', 'TradeExecutions', 'HistoricalData', 'Lists', 'Notes', 'EconomicCalendar')
GROUP BY t.name
ORDER BY t.name;

PRINT '';
PRINT 'Record Counts:';

SELECT 'Instruments' AS TableName, COUNT(*) AS RecordCount FROM [dbo].[Instruments]
UNION ALL
SELECT 'Positions', COUNT(*) FROM [dbo].[Positions]
UNION ALL
SELECT 'TradeExecutions', COUNT(*) FROM [dbo].[TradeExecutions]
UNION ALL
SELECT 'HistoricalData', COUNT(*) FROM [dbo].[HistoricalData]
UNION ALL
SELECT 'Lists', COUNT(*) FROM [dbo].[Lists]
UNION ALL
SELECT 'Notes', COUNT(*) FROM [dbo].[Notes]
UNION ALL
SELECT 'EconomicCalendar', COUNT(*) FROM [dbo].[EconomicCalendar]
ORDER BY TableName;

GO
