-- =============================================
-- SQL Server Database Creation Script for TradingBE
-- Generated from IKBR_Report_Puller repository
-- Description: Complete database schema recreation script
-- =============================================

USE master;
GO

-- Drop database if exists (CAUTION: This will delete all data!)
-- Uncomment the following lines to drop and recreate the database
/*
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'TradingBE')
BEGIN
	ALTER DATABASE TradingBE SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	DROP DATABASE TradingBE;
END
GO
*/

-- Create the database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'TradingBE')
BEGIN
	CREATE DATABASE TradingBE;
	PRINT 'Database TradingBE created successfully.';
END
ELSE
BEGIN
	PRINT 'Database TradingBE already exists.';
END
GO

USE TradingBE;
GO

-- =============================================
-- Table 1: Instruments
-- Description: Stores trading instrument information
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Instruments]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Instruments]
	(
		[Id] INT IDENTITY(1,1) NOT NULL,
		[InstrumentName] NVARCHAR(255) NOT NULL,
		[Provider] NVARCHAR(100) NULL,
		[DataName] NVARCHAR(255) NULL,
		[DataSource] NVARCHAR(100) NULL,
		[Format] NVARCHAR(50) NULL,
		[Frequency] NVARCHAR(50) NULL,
		[ContractUnit] FLOAT NULL,
		[ContractUnitType] NVARCHAR(100) NULL,
		[PriceQuotation] NVARCHAR(100) NULL,
		[MinimumPriceFluctuation] FLOAT NULL,
		[Currency] NVARCHAR(10) NULL,
		[ListingExchange] NVARCHAR(50) NULL,
		[ConId] NVARCHAR(50) NULL,

		CONSTRAINT [PK_Instruments] PRIMARY KEY CLUSTERED ([Id] ASC)
	);

	-- Create unique index on ConId for fast lookup
	CREATE UNIQUE NONCLUSTERED INDEX [IX_Instruments_ConId] 
	ON [dbo].[Instruments] ([ConId]) 
	WHERE [ConId] IS NOT NULL;

	-- Create index on InstrumentName
	CREATE NONCLUSTERED INDEX [IX_Instruments_InstrumentName] 
	ON [dbo].[Instruments] ([InstrumentName]);

	PRINT 'Table Instruments created successfully.';
END
ELSE
BEGIN
	PRINT 'Table Instruments already exists. Checking for missing columns...';

	-- Add ConId column if it doesn't exist
	IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Instruments]') AND name = 'ConId')
	BEGIN
		ALTER TABLE [dbo].[Instruments] ADD [ConId] NVARCHAR(50) NULL;
		PRINT '  - Added column ConId';

		-- Create unique index on ConId after adding the column
		IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Instruments]') AND name = 'IX_Instruments_ConId')
		BEGIN
			CREATE UNIQUE NONCLUSTERED INDEX [IX_Instruments_ConId] 
			ON [dbo].[Instruments] ([ConId]) 
			WHERE [ConId] IS NOT NULL;
			PRINT '  - Created index IX_Instruments_ConId';
		END
	END
	ELSE
	BEGIN
		-- Index might be missing even if column exists
		IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Instruments]') AND name = 'IX_Instruments_ConId')
		BEGIN
			CREATE UNIQUE NONCLUSTERED INDEX [IX_Instruments_ConId] 
			ON [dbo].[Instruments] ([ConId]) 
			WHERE [ConId] IS NOT NULL;
			PRINT '  - Created index IX_Instruments_ConId';
		END
		ELSE
		BEGIN
			PRINT '  - Column ConId and index already exist';
		END
	END

	-- Check and create index on InstrumentName if missing
	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Instruments]') AND name = 'IX_Instruments_InstrumentName')
	BEGIN
		CREATE NONCLUSTERED INDEX [IX_Instruments_InstrumentName] 
		ON [dbo].[Instruments] ([InstrumentName]);
		PRINT '  - Created index IX_Instruments_InstrumentName';
	END
END
GO

-- =============================================
-- Table 2: Positions
-- Description: Stores position data from IBKR reports (both open and closed positions)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Positions]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Positions]
	(
		[Id] INT IDENTITY(1,1) NOT NULL,
		[OpenDate] DATETIME2(7) NOT NULL,
		[CloseDate] DATETIME2(7) NULL,
		[Status] NVARCHAR(20) NOT NULL DEFAULT 'Open',
		[InstrumentId] INT NOT NULL,
		[LastReportedPrice] DECIMAL(18, 6) NULL,
		[LastReportedPriceUpdated] DATETIME2(7) NULL,

		CONSTRAINT [PK_Positions] PRIMARY KEY CLUSTERED ([Id] ASC),
		CONSTRAINT [FK_Positions_Instruments] FOREIGN KEY ([InstrumentId]) 
			REFERENCES [dbo].[Instruments] ([Id])
	);

	-- Create index on OpenDate for time-based queries
	CREATE NONCLUSTERED INDEX [IX_Positions_OpenDate] 
	ON [dbo].[Positions] ([OpenDate] DESC);

	-- Create index on Status for filtering open/closed positions
	CREATE NONCLUSTERED INDEX [IX_Positions_Status] 
	ON [dbo].[Positions] ([Status]);

	-- Create index on InstrumentId for foreign key lookups
	CREATE NONCLUSTERED INDEX [IX_Positions_InstrumentId] 
	ON [dbo].[Positions] ([InstrumentId]);

	PRINT 'Table Positions created successfully.';
END
GO

-- =============================================
-- Table 3: TradeExecutions
-- Description: Stores all trade execution records from IBKR reports
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TradeExecutions]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[TradeExecutions]
	(
		[Id] INT IDENTITY(1,1) NOT NULL,
		[PositionId] INT NULL,
		[symbol] NVARCHAR(50) NULL,
		[securityID] NVARCHAR(50) NULL,
		[tradeID] BIGINT NULL,
		[dateTime] NVARCHAR(50) NULL,
		[tradeDate] NVARCHAR(50) NULL,
		[quantity] DECIMAL(18, 6) NULL,
		[tradePrice] DECIMAL(18, 6) NULL,
		[ibCommission] DECIMAL(18, 6) NULL,
		[ibCommissionCurrency] NVARCHAR(10) NULL,
		[closePrice] DECIMAL(18, 6) NULL,
		[cost] DECIMAL(18, 6) NULL,
		[fifoPnlRealized] DECIMAL(18, 6) NULL,
		[buySell] NVARCHAR(10) NULL,
		[transactionID] BIGINT NULL,
		[ibExecID] NVARCHAR(100) NULL,
		[brokerageOrderID] NVARCHAR(100) NULL,
		[exchOrderId] NVARCHAR(100) NULL,
		[extExecID] NVARCHAR(100) NULL,
		[orderType] NVARCHAR(50) NULL,
		[traderID] NVARCHAR(50) NULL,
		[currency] NVARCHAR(10) NULL,
		[description] NVARCHAR(500) NULL,
		[conid] NVARCHAR(50) NULL,
		[taxes] DECIMAL(18, 6) NULL,
		[assetCategory] NVARCHAR(50) NULL,
		[expiry] NVARCHAR(50) NULL,
		[transactionType] NVARCHAR(50) NULL,
		[exchange] NVARCHAR(50) NULL,
		[proceeds] DECIMAL(18, 6) NULL,
		[netCash] DECIMAL(18, 6) NULL,
		[mtmPnl] DECIMAL(18, 6) NULL,
		[origTradePrice] DECIMAL(18, 6) NULL,
		[origTradeDate] NVARCHAR(50) NULL,
		[origTradeID] NVARCHAR(50) NULL,
		[origOrderID] BIGINT NULL,
		[origTransactionID] BIGINT NULL,
		[ibOrderID] BIGINT NULL,
		[openDateTime] NVARCHAR(50) NULL,
		[initialInvestment] DECIMAL(18, 6) NULL,
		[accountId] NVARCHAR(50) NULL,
		[acctAlias] NVARCHAR(50) NULL,
		[model] NVARCHAR(50) NULL,
		[fxRateToBase] DECIMAL(18, 10) NULL,
		[subCategory] NVARCHAR(50) NULL,
		[securityIDType] NVARCHAR(50) NULL,
		[cusip] NVARCHAR(50) NULL,
		[isin] NVARCHAR(50) NULL,
		[figi] NVARCHAR(50) NULL,
		[listingExchange] NVARCHAR(50) NULL,
		[underlyingConid] NVARCHAR(50) NULL,
		[underlyingSymbol] NVARCHAR(50) NULL,
		[underlyingSecurityID] NVARCHAR(50) NULL,
		[underlyingListingExchange] NVARCHAR(50) NULL,
		[issuer] NVARCHAR(100) NULL,
		[issuerCountryCode] NVARCHAR(10) NULL,
		[multiplier] INT NULL,
		[relatedTradeID] NVARCHAR(50) NULL,
		[strike] DECIMAL(18, 6) NULL,
		[reportDate] NVARCHAR(50) NULL,
		[putCall] NVARCHAR(10) NULL,
		[principalAdjustFactor] DECIMAL(18, 10) NULL,
		[settleDateTarget] NVARCHAR(50) NULL,
		[tradeMoney] DECIMAL(18, 6) NULL,
		[openCloseIndicator] NVARCHAR(10) NULL,
		[notes] NVARCHAR(MAX) NULL,
		[clearingFirmID] NVARCHAR(50) NULL,
		[relatedTransactionID] NVARCHAR(50) NULL,
		[rtn] NVARCHAR(50) NULL,
		[orderReference] NVARCHAR(100) NULL,
		[volatilityOrderLink] NVARCHAR(100) NULL,
		[orderTime] NVARCHAR(50) NULL,
		[holdingPeriodDateTime] NVARCHAR(50) NULL,
		[whenRealized] NVARCHAR(50) NULL,
		[whenReopened] NVARCHAR(50) NULL,
		[levelOfDetail] NVARCHAR(50) NULL,
		[changeInPrice] DECIMAL(18, 6) NULL,
		[changeInQuantity] DECIMAL(18, 6) NULL,
		[isAPIOrder] NVARCHAR(10) NULL,
		[accruedInt] DECIMAL(18, 6) NULL,
		[positionActionID] NVARCHAR(50) NULL,
		[serialNumber] NVARCHAR(50) NULL,
		[deliveryType] NVARCHAR(50) NULL,
		[commodityType] NVARCHAR(50) NULL,
		[fineness] DECIMAL(18, 6) NULL,
		[weight] DECIMAL(18, 6) NULL,

		CONSTRAINT [PK_TradeExecutions] PRIMARY KEY CLUSTERED ([Id] ASC),
		CONSTRAINT [FK_TradeExecutions_Positions] FOREIGN KEY ([PositionId]) 
			REFERENCES [dbo].[Positions] ([Id])
	);

	-- Create unique index on ibExecID (execution ID is unique identifier)
	CREATE UNIQUE NONCLUSTERED INDEX [IX_TradeExecutions_IbExecID] 
	ON [dbo].[TradeExecutions] ([ibExecID]) 
	WHERE [ibExecID] IS NOT NULL;

	-- Create index on ibOrderID for order-based queries
	CREATE NONCLUSTERED INDEX [IX_TradeExecutions_IbOrderID] 
	ON [dbo].[TradeExecutions] ([ibOrderID]);

	-- Create index on tradeDate for date-based queries
	CREATE NONCLUSTERED INDEX [IX_TradeExecutions_TradeDate] 
	ON [dbo].[TradeExecutions] ([tradeDate]);

	-- Create index on symbol
	CREATE NONCLUSTERED INDEX [IX_TradeExecutions_Symbol] 
	ON [dbo].[TradeExecutions] ([symbol]);

	-- Create index on PositionId for FK performance
	CREATE NONCLUSTERED INDEX [IX_TradeExecutions_PositionId] 
	ON [dbo].[TradeExecutions] ([PositionId])
	WHERE [PositionId] IS NOT NULL;

	PRINT 'Table TradeExecutions created successfully.';
END
GO

-- =============================================
-- Table 4: HistoricalData
-- Description: Stores historical price bar data for instruments
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HistoricalData]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[HistoricalData]
	(
		[Id] INT IDENTITY(1,1) NOT NULL,
		[Date] DATETIME NOT NULL,
		[OpenPrice] FLOAT NOT NULL,
		[ClosePrice] FLOAT NOT NULL,
		[LowPrice] FLOAT NOT NULL,
		[HighPrice] FLOAT NOT NULL,
		[Volume] FLOAT NOT NULL,
		[Settle] FLOAT NULL,
		[OpenInterest] FLOAT NULL,
		[InstrumentId] INT NOT NULL,

		CONSTRAINT [PK_HistoricalData] PRIMARY KEY CLUSTERED ([Id] ASC),
		CONSTRAINT [FK_HistoricalData_Instruments] FOREIGN KEY ([InstrumentId]) 
			REFERENCES [dbo].[Instruments] ([Id]),
		CONSTRAINT [UQ_HistoricalData_InstrumentDate] UNIQUE NONCLUSTERED ([InstrumentId], [Date])
	);

	-- Create index on Date for time-series queries
	CREATE NONCLUSTERED INDEX [IX_HistoricalData_Date] 
	ON [dbo].[HistoricalData] ([Date] DESC);

	-- Create index on InstrumentId for FK performance and instrument-based queries
	CREATE NONCLUSTERED INDEX [IX_HistoricalData_InstrumentId] 
	ON [dbo].[HistoricalData] ([InstrumentId], [Date] DESC);

	PRINT 'Table HistoricalData created successfully.';
END
GO

-- =============================================
-- Table 5: List
-- Description: Stores lookup list values for categorization (e.g., trade types)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[List]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[List]
	(
		[Id] INT IDENTITY(1,1) NOT NULL,
		[Name] NVARCHAR(100) NOT NULL,
		[Description] NVARCHAR(500) NULL,
		[Category] NVARCHAR(50) NULL,
		[IsActive] BIT NOT NULL DEFAULT 1,
		[CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
		[UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),

		CONSTRAINT [PK_List] PRIMARY KEY CLUSTERED ([Id] ASC)
	);

	-- Create index on Name for quick lookups
	CREATE NONCLUSTERED INDEX [IX_List_Name] 
	ON [dbo].[List] ([Name]);

	-- Create index on Category for filtering by category
	CREATE NONCLUSTERED INDEX [IX_List_Category] 
	ON [dbo].[List] ([Category])
	WHERE [Category] IS NOT NULL;

	-- Create index on IsActive for filtering active items
	CREATE NONCLUSTERED INDEX [IX_List_IsActive] 
	ON [dbo].[List] ([IsActive]);

	PRINT 'Table List created successfully.';
END
GO

-- =============================================
-- Table 6: Notes
-- Description: Stores notes related to positions and trade types
-- =============================================
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

		CONSTRAINT [PK_Notes] PRIMARY KEY CLUSTERED ([Id] ASC),
		CONSTRAINT [FK_Notes_Positions] FOREIGN KEY ([PositionId]) 
			REFERENCES [dbo].[Positions] ([Id]) ON DELETE CASCADE,
		CONSTRAINT [FK_Notes_List] FOREIGN KEY ([TradeTypeId]) 
			REFERENCES [dbo].[List] ([Id])
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

	PRINT 'Table Notes created successfully.';
END
GO

-- =============================================
-- Table 7: EconomicCalendar
-- Description: Stores economic calendar events from Financial Modeling Prep API
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EconomicCalendar]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[EconomicCalendar]
	(
		[Id] INT IDENTITY(1,1) NOT NULL,
		[Date] DATETIME2(7) NOT NULL,
		[Country] NVARCHAR(50) NULL,
		[Event] NVARCHAR(500) NOT NULL,
		[Currency] NVARCHAR(10) NULL,
		[Previous] DECIMAL(18,4) NULL,
		[Estimate] DECIMAL(18,4) NULL,
		[Actual] DECIMAL(18,4) NULL,
		[Change] DECIMAL(18,4) NULL,
		[Impact] NVARCHAR(50) NULL,
		[ChangePercentage] DECIMAL(18,4) NULL,
		[Unit] NVARCHAR(50) NULL,
		[CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
		[UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),

		CONSTRAINT [PK_EconomicCalendar] PRIMARY KEY CLUSTERED ([Id] ASC),
		CONSTRAINT [UQ_EconomicCalendar_DateCountryEvent] UNIQUE NONCLUSTERED ([Date], [Country], [Event])
	);

	-- Create index for date-based queries
	CREATE NONCLUSTERED INDEX [IX_EconomicCalendar_Date]
	ON [dbo].[EconomicCalendar] ([Date] DESC);

	-- Create index for country-based queries
	CREATE NONCLUSTERED INDEX [IX_EconomicCalendar_Country]
	ON [dbo].[EconomicCalendar] ([Country]);

	-- Create index for impact-based queries
	CREATE NONCLUSTERED INDEX [IX_EconomicCalendar_Impact]
	ON [dbo].[EconomicCalendar] ([Impact])
	WHERE [Impact] IS NOT NULL;

	PRINT 'Table EconomicCalendar created successfully.';
END
GO

-- =============================================
-- Grant Permissions (Adjust as needed for your environment)
-- =============================================
-- Uncomment and modify the following to grant permissions to your application user
/*
-- Grant permissions to application user
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.Instruments TO [YourApplicationUser];
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.Positions TO [YourApplicationUser];
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.TradeExecutions TO [YourApplicationUser];
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.HistoricalData TO [YourApplicationUser];
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.List TO [YourApplicationUser];
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.Notes TO [YourApplicationUser];
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.EconomicCalendar TO [YourApplicationUser];
GO
*/

-- =============================================
-- Database Creation Summary
-- =============================================
PRINT '==============================================';
PRINT 'Database TradingBE setup completed successfully!';
PRINT '==============================================';
PRINT '';
PRINT 'Tables created:';
PRINT '  1. Instruments - Trading instrument master data';
PRINT '  2. Positions - Position data (both open and closed)';
PRINT '  3. TradeExecutions - Trade execution records from IBKR';
PRINT '  4. HistoricalData - Historical price data';
PRINT '  5. List - Lookup list values for categorization';
PRINT '  6. Notes - Notes related to positions and trade types';
PRINT '  7. EconomicCalendar - Economic calendar events';
PRINT '';
PRINT 'Next steps:';
PRINT '  1. Update connection string in appsettings.json';
PRINT '  2. Grant appropriate permissions to application users';
PRINT '  3. Run the application to start importing data';
PRINT '==============================================';
GO

-- =============================================
-- Optional: Add sample data verification queries
-- =============================================
-- Uncomment to run verification queries after data import
/*
-- Check record counts
SELECT 'Instruments' AS TableName, COUNT(*) AS RecordCount FROM dbo.Instruments
UNION ALL
SELECT 'Positions', COUNT(*) FROM dbo.Positions
UNION ALL
SELECT 'TradeExecutions', COUNT(*) FROM dbo.TradeExecutions
UNION ALL
SELECT 'HistoricalData', COUNT(*) FROM dbo.HistoricalData
UNION ALL
SELECT 'List', COUNT(*) FROM dbo.List
UNION ALL
SELECT 'Notes', COUNT(*) FROM dbo.Notes
UNION ALL
SELECT 'EconomicCalendar', COUNT(*) FROM dbo.EconomicCalendar;
GO
*/
