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
	CREATE DATABASE [TradingBE]
 CONTAINMENT = NONE
 ON  PRIMARY 
	( NAME = N'TradingBE', FILENAME = N'/var/opt/mssql/data/TradingBE.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
	 LOG ON 
	( NAME = N'TradingBE_log', FILENAME = N'/var/opt/mssql/data/TradingBE_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
	PRINT 'Database TradingBE created successfully.';
END
ELSE
BEGIN
	PRINT 'Database TradingBE already exists.';
END
GO

USE TradingBE;
GO


/****** Object:  Database [TradingBE]    Script Date: 23/08/2026 12:54:43 ******/

ALTER DATABASE [TradingBE] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [TradingBE].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [TradingBE] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [TradingBE] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [TradingBE] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [TradingBE] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [TradingBE] SET ARITHABORT OFF 
GO
ALTER DATABASE [TradingBE] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [TradingBE] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [TradingBE] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [TradingBE] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [TradingBE] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [TradingBE] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [TradingBE] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [TradingBE] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [TradingBE] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [TradingBE] SET  DISABLE_BROKER 
GO
ALTER DATABASE [TradingBE] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [TradingBE] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [TradingBE] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [TradingBE] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [TradingBE] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [TradingBE] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [TradingBE] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [TradingBE] SET RECOVERY FULL 
GO
ALTER DATABASE [TradingBE] SET  MULTI_USER 
GO
ALTER DATABASE [TradingBE] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [TradingBE] SET DB_CHAINING OFF 
GO
ALTER DATABASE [TradingBE] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [TradingBE] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [TradingBE] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [TradingBE] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'TradingBE', N'ON'
GO
ALTER DATABASE [TradingBE] SET QUERY_STORE = ON
GO
ALTER DATABASE [TradingBE] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [TradingBE]
GO
/****** Object:  Table [dbo].[CanSlimCandidateAnnualHistory]    Script Date: 23/08/2026 12:54:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CanSlimCandidateAnnualHistory](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CandidateSnapshotId] [bigint] NOT NULL,
	[CalendarYear] [varchar](10) NOT NULL,
	[FiscalDate] [varchar](10) NULL,
	[Revenue] [decimal](19, 2) NOT NULL,
	[NetIncome] [decimal](19, 2) NOT NULL,
	[EpsDiluted] [decimal](18, 4) NOT NULL,
	[EpsGrowthYoYPercent] [decimal](9, 4) NOT NULL,
 CONSTRAINT [PK_CanSlimCandidateAnnualHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CanSlimCandidateSnapshots]    Script Date: 23/08/2026 12:54:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CanSlimCandidateSnapshots](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Symbol] [varchar](16) NOT NULL,
	[Exchange] [varchar](16) NULL,
	[CompanyName] [nvarchar](200) NULL,
	[Sector] [nvarchar](100) NULL,
	[Industry] [nvarchar](100) NULL,
	[Price] [decimal](18, 4) NOT NULL,
	[Volume] [decimal](18, 2) NOT NULL,
	[MarketCap] [decimal](19, 2) NOT NULL,
	[EvaluationDateUtc] [datetime2](7) NOT NULL,
	[PassesBoth] [bit] NOT NULL,
	[CurrentQuarter_LatestQuarterDate] [varchar](10) NULL,
	[CurrentQuarter_LatestQuarterEps] [decimal](18, 4) NOT NULL,
	[CurrentQuarter_PriorYearQuarterEps] [decimal](18, 4) NOT NULL,
	[CurrentQuarter_EpsGrowthYoYPercent] [decimal](9, 4) NOT NULL,
	[CurrentQuarter_RevenueGrowthYoYPercent] [decimal](9, 4) NOT NULL,
	[CurrentQuarter_IsAccelerating] [bit] NOT NULL,
	[CurrentQuarter_PassesCriteria] [bit] NOT NULL,
	[Annual_EpsCagr3YearPercent] [decimal](9, 4) NOT NULL,
	[Annual_EpsCagr5YearPercent] [decimal](9, 4) NULL,
	[Annual_ReturnOnEquityPercent] [decimal](9, 4) NOT NULL,
	[Annual_HasConsecutiveAnnualGrowth] [bit] NOT NULL,
	[Annual_LatestFiscalYear] [varchar](10) NULL,
	[Annual_LatestFiscalYearEps] [decimal](18, 4) NOT NULL,
	[Annual_PriorYear1Eps] [decimal](18, 4) NOT NULL,
	[Annual_PriorYear2Eps] [decimal](18, 4) NOT NULL,
	[Annual_PriorYear3Eps] [decimal](18, 4) NOT NULL,
	[Annual_OperatingMarginPercent] [decimal](9, 4) NOT NULL,
	[Annual_ReturnOnAssetsPercent] [decimal](9, 4) NOT NULL,
	[Annual_FundamentalGrade] [varchar](5) NOT NULL,
	[Annual_PassesCriteria] [bit] NOT NULL,
	[CreatedAtUtc] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_CanSlimCandidateSnapshots] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EconomicCalendar]    Script Date: 23/08/2026 12:54:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EconomicCalendar](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime2](7) NOT NULL,
	[Country] [nvarchar](50) NULL,
	[Event] [nvarchar](500) NOT NULL,
	[Currency] [nvarchar](10) NULL,
	[Previous] [decimal](18, 4) NULL,
	[Estimate] [decimal](18, 4) NULL,
	[Actual] [decimal](18, 4) NULL,
	[Change] [decimal](18, 4) NULL,
	[Impact] [nvarchar](50) NULL,
	[ChangePercentage] [decimal](18, 4) NULL,
	[Unit] [nvarchar](50) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_EconomicCalendar] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_EconomicCalendar_DateCountryEvent] UNIQUE NONCLUSTERED 
(
	[Date] ASC,
	[Country] ASC,
	[Event] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HistoricalData]    Script Date: 23/08/2026 12:54:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HistoricalData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[OpenPrice] [float] NOT NULL,
	[ClosePrice] [float] NOT NULL,
	[LowPrice] [float] NOT NULL,
	[HighPrice] [float] NOT NULL,
	[Volume] [float] NOT NULL,
	[Settle] [float] NULL,
	[OpenInterest] [float] NULL,
	[InstrumentId] [int] NOT NULL,
 CONSTRAINT [PK_HistoricalData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_HistoricalData_InstrumentDate] UNIQUE NONCLUSTERED 
(
	[InstrumentId] ASC,
	[Date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Instruments]    Script Date: 23/08/2026 12:54:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Instruments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InstrumentName] [nvarchar](255) NOT NULL,
	[Provider] [nvarchar](100) NULL,
	[DataName] [nvarchar](255) NULL,
	[DataSource] [nvarchar](100) NULL,
	[Format] [nvarchar](50) NULL,
	[Frequency] [nvarchar](50) NULL,
	[ContractUnit] [float] NULL,
	[ContractUnitType] [nvarchar](100) NULL,
	[PriceQuotation] [nvarchar](100) NULL,
	[MinimumPriceFluctuation] [float] NULL,
	[Currency] [nvarchar](10) NULL,
	[ListingExchange] [nvarchar](50) NULL,
	[ConId] [nvarchar](50) NULL,
 CONSTRAINT [PK_Instruments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Lists]    Script Date: 23/08/2026 12:54:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Lists](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[Category] [nvarchar](50) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Lists] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notes]    Script Date: 23/08/2026 12:54:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PositionId] [int] NOT NULL,
	[TradeExecutionId] [int] NULL,
	[TradeTypeId] [int] NULL,
	[Comment] [nvarchar](max) NOT NULL,
	[EntryDate] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Notes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Positions]    Script Date: 23/08/2026 12:54:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Positions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OpenDate] [datetime2](7) NOT NULL,
	[CloseDate] [datetime2](7) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[InstrumentId] [int] NOT NULL,
	[LastReportedPrice] [decimal](18, 6) NULL,
	[LastReportedPriceUpdated] [datetime2](7) NULL,
 CONSTRAINT [PK_Positions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TradeExecutions]    Script Date: 23/08/2026 12:54:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TradeExecutions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PositionId] [int] NULL,
	[symbol] [nvarchar](50) NULL,
	[securityID] [nvarchar](50) NULL,
	[tradeID] [bigint] NULL,
	[dateTime] [datetime] NOT NULL,
	[tradeDate] [datetime] NOT NULL,
	[quantity] [decimal](18, 6) NULL,
	[tradePrice] [decimal](18, 6) NULL,
	[ibCommission] [decimal](18, 6) NULL,
	[ibCommissionCurrency] [nvarchar](10) NULL,
	[closePrice] [decimal](18, 6) NULL,
	[cost] [decimal](18, 6) NULL,
	[fifoPnlRealized] [decimal](18, 6) NULL,
	[buySell] [nvarchar](10) NULL,
	[transactionID] [bigint] NULL,
	[ibExecID] [nvarchar](100) NULL,
	[brokerageOrderID] [nvarchar](100) NULL,
	[exchOrderId] [nvarchar](100) NULL,
	[extExecID] [nvarchar](100) NULL,
	[orderType] [nvarchar](50) NULL,
	[traderID] [nvarchar](50) NULL,
	[currency] [nvarchar](10) NULL,
	[description] [nvarchar](500) NULL,
	[conid] [nvarchar](50) NULL,
	[taxes] [decimal](18, 6) NULL,
	[assetCategory] [nvarchar](50) NULL,
	[expiry] [nvarchar](50) NULL,
	[transactionType] [nvarchar](50) NULL,
	[exchange] [nvarchar](50) NULL,
	[proceeds] [decimal](18, 6) NULL,
	[netCash] [decimal](18, 6) NULL,
	[mtmPnl] [decimal](18, 6) NULL,
	[origTradePrice] [decimal](18, 6) NULL,
	[origTradeDate] [nvarchar](50) NULL,
	[origTradeID] [nvarchar](50) NULL,
	[origOrderID] [bigint] NULL,
	[origTransactionID] [bigint] NULL,
	[ibOrderID] [bigint] NULL,
	[openDateTime] [nvarchar](50) NULL,
	[initialInvestment] [decimal](18, 6) NULL,
	[accountId] [nvarchar](50) NULL,
	[acctAlias] [nvarchar](50) NULL,
	[model] [nvarchar](50) NULL,
	[fxRateToBase] [decimal](18, 10) NULL,
	[subCategory] [nvarchar](50) NULL,
	[securityIDType] [nvarchar](50) NULL,
	[cusip] [nvarchar](50) NULL,
	[isin] [nvarchar](50) NULL,
	[figi] [nvarchar](50) NULL,
	[listingExchange] [nvarchar](50) NULL,
	[underlyingConid] [nvarchar](50) NULL,
	[underlyingSymbol] [nvarchar](50) NULL,
	[underlyingSecurityID] [nvarchar](50) NULL,
	[underlyingListingExchange] [nvarchar](50) NULL,
	[issuer] [nvarchar](100) NULL,
	[issuerCountryCode] [nvarchar](10) NULL,
	[multiplier] [int] NULL,
	[relatedTradeID] [nvarchar](50) NULL,
	[strike] [decimal](18, 6) NULL,
	[reportDate] [nvarchar](50) NULL,
	[putCall] [nvarchar](10) NULL,
	[principalAdjustFactor] [decimal](18, 10) NULL,
	[settleDateTarget] [nvarchar](50) NULL,
	[tradeMoney] [decimal](18, 6) NULL,
	[openCloseIndicator] [nvarchar](10) NULL,
	[notes] [nvarchar](max) NULL,
	[clearingFirmID] [nvarchar](50) NULL,
	[relatedTransactionID] [nvarchar](50) NULL,
	[rtn] [nvarchar](50) NULL,
	[orderReference] [nvarchar](100) NULL,
	[volatilityOrderLink] [nvarchar](100) NULL,
	[orderTime] [nvarchar](50) NULL,
	[holdingPeriodDateTime] [nvarchar](50) NULL,
	[whenRealized] [nvarchar](50) NULL,
	[whenReopened] [nvarchar](50) NULL,
	[levelOfDetail] [nvarchar](50) NULL,
	[changeInPrice] [decimal](18, 6) NULL,
	[changeInQuantity] [decimal](18, 6) NULL,
	[isAPIOrder] [nvarchar](10) NULL,
	[accruedInt] [decimal](18, 6) NULL,
	[positionActionID] [nvarchar](50) NULL,
	[serialNumber] [nvarchar](50) NULL,
	[deliveryType] [nvarchar](50) NULL,
	[commodityType] [nvarchar](50) NULL,
	[fineness] [decimal](18, 6) NULL,
	[weight] [decimal](18, 6) NULL,
 CONSTRAINT [PK_TradeExecutions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Index [IX_CanSlimCandidateAnnualHistory_SnapshotId]    Script Date: 23/08/2026 12:54:43 ******/
CREATE NONCLUSTERED INDEX [IX_CanSlimCandidateAnnualHistory_SnapshotId] ON [dbo].[CanSlimCandidateAnnualHistory]
(
	[CandidateSnapshotId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CanSlimCandidateSnapshots_PassesBoth]    Script Date: 23/08/2026 12:54:43 ******/
CREATE NONCLUSTERED INDEX [IX_CanSlimCandidateSnapshots_PassesBoth] ON [dbo].[CanSlimCandidateSnapshots]
(
	[PassesBoth] ASC,
	[EvaluationDateUtc] DESC
)
INCLUDE([Symbol],[Price],[Annual_ReturnOnEquityPercent],[CurrentQuarter_EpsGrowthYoYPercent]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_CanSlimCandidateSnapshots_Symbol_Date]    Script Date: 23/08/2026 12:54:43 ******/
CREATE NONCLUSTERED INDEX [IX_CanSlimCandidateSnapshots_Symbol_Date] ON [dbo].[CanSlimCandidateSnapshots]
(
	[Symbol] ASC,
	[EvaluationDateUtc] DESC
)
INCLUDE([PassesBoth],[Price],[Volume],[MarketCap]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_EconomicCalendar_Country]    Script Date: 23/08/2026 12:54:43 ******/
CREATE NONCLUSTERED INDEX [IX_EconomicCalendar_Country] ON [dbo].[EconomicCalendar]
(
	[Country] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_EconomicCalendar_Date]    Script Date: 23/08/2026 12:54:43 ******/
CREATE NONCLUSTERED INDEX [IX_EconomicCalendar_Date] ON [dbo].[EconomicCalendar]
(
	[Date] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HistoricalData_Date]    Script Date: 23/08/2026 12:54:43 ******/
CREATE NONCLUSTERED INDEX [IX_HistoricalData_Date] ON [dbo].[HistoricalData]
(
	[Date] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HistoricalData_InstrumentId]    Script Date: 23/08/2026 12:54:43 ******/
CREATE NONCLUSTERED INDEX [IX_HistoricalData_InstrumentId] ON [dbo].[HistoricalData]
(
	[InstrumentId] ASC,
	[Date] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Lists_Name]    Script Date: 23/08/2026 12:54:43 ******/
CREATE NONCLUSTERED INDEX [IX_Lists_Name] ON [dbo].[Lists]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Notes_PositionId]    Script Date: 23/08/2026 12:54:43 ******/
CREATE NONCLUSTERED INDEX [IX_Notes_PositionId] ON [dbo].[Notes]
(
	[PositionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Positions_InstrumentId]    Script Date: 23/08/2026 12:54:43 ******/
CREATE NONCLUSTERED INDEX [IX_Positions_InstrumentId] ON [dbo].[Positions]
(
	[InstrumentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Positions_OpenDate]    Script Date: 23/08/2026 12:54:43 ******/
CREATE NONCLUSTERED INDEX [IX_Positions_OpenDate] ON [dbo].[Positions]
(
	[OpenDate] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Positions_Status]    Script Date: 23/08/2026 12:54:43 ******/
CREATE NONCLUSTERED INDEX [IX_Positions_Status] ON [dbo].[Positions]
(
	[Status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CanSlimCandidateAnnualHistory] ADD  DEFAULT ((0)) FOR [Revenue]
GO
ALTER TABLE [dbo].[CanSlimCandidateAnnualHistory] ADD  DEFAULT ((0)) FOR [NetIncome]
GO
ALTER TABLE [dbo].[CanSlimCandidateAnnualHistory] ADD  DEFAULT ((0)) FOR [EpsDiluted]
GO
ALTER TABLE [dbo].[CanSlimCandidateAnnualHistory] ADD  DEFAULT ((0)) FOR [EpsGrowthYoYPercent]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT (sysutcdatetime()) FOR [EvaluationDateUtc]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [PassesBoth]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [CurrentQuarter_LatestQuarterEps]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [CurrentQuarter_PriorYearQuarterEps]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [CurrentQuarter_EpsGrowthYoYPercent]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [CurrentQuarter_RevenueGrowthYoYPercent]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [CurrentQuarter_IsAccelerating]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [CurrentQuarter_PassesCriteria]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [Annual_EpsCagr3YearPercent]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [Annual_ReturnOnEquityPercent]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [Annual_HasConsecutiveAnnualGrowth]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [Annual_LatestFiscalYearEps]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [Annual_PriorYear1Eps]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [Annual_PriorYear2Eps]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [Annual_PriorYear3Eps]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [Annual_OperatingMarginPercent]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [Annual_ReturnOnAssetsPercent]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ('N/A') FOR [Annual_FundamentalGrade]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT ((0)) FOR [Annual_PassesCriteria]
GO
ALTER TABLE [dbo].[CanSlimCandidateSnapshots] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAtUtc]
GO
ALTER TABLE [dbo].[EconomicCalendar] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[EconomicCalendar] ADD  DEFAULT (getutcdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Lists] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Lists] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Lists] ADD  DEFAULT (getutcdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Notes] ADD  DEFAULT (getutcdate()) FOR [EntryDate]
GO
ALTER TABLE [dbo].[Notes] ADD  DEFAULT (getutcdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Positions] ADD  DEFAULT ('Open') FOR [Status]
GO
ALTER TABLE [dbo].[CanSlimCandidateAnnualHistory]  WITH CHECK ADD  CONSTRAINT [FK_CanSlimCandidateAnnualHistory_CandidateSnapshot] FOREIGN KEY([CandidateSnapshotId])
REFERENCES [dbo].[CanSlimCandidateSnapshots] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CanSlimCandidateAnnualHistory] CHECK CONSTRAINT [FK_CanSlimCandidateAnnualHistory_CandidateSnapshot]
GO
ALTER TABLE [dbo].[HistoricalData]  WITH CHECK ADD  CONSTRAINT [FK_HistoricalData_Instruments] FOREIGN KEY([InstrumentId])
REFERENCES [dbo].[Instruments] ([Id])
GO
ALTER TABLE [dbo].[HistoricalData] CHECK CONSTRAINT [FK_HistoricalData_Instruments]
GO
ALTER TABLE [dbo].[Notes]  WITH CHECK ADD  CONSTRAINT [FK_Notes_Lists] FOREIGN KEY([TradeTypeId])
REFERENCES [dbo].[Lists] ([Id])
GO
ALTER TABLE [dbo].[Notes] CHECK CONSTRAINT [FK_Notes_Lists]
GO
ALTER TABLE [dbo].[Notes]  WITH CHECK ADD  CONSTRAINT [FK_Notes_Positions] FOREIGN KEY([PositionId])
REFERENCES [dbo].[Positions] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Notes] CHECK CONSTRAINT [FK_Notes_Positions]
GO
ALTER TABLE [dbo].[Positions]  WITH CHECK ADD  CONSTRAINT [FK_Positions_Instruments] FOREIGN KEY([InstrumentId])
REFERENCES [dbo].[Instruments] ([Id])
GO
ALTER TABLE [dbo].[Positions] CHECK CONSTRAINT [FK_Positions_Instruments]
GO
ALTER TABLE [dbo].[TradeExecutions]  WITH CHECK ADD  CONSTRAINT [FK_TradeExecutions_Positions] FOREIGN KEY([PositionId])
REFERENCES [dbo].[Positions] ([Id])
GO
ALTER TABLE [dbo].[TradeExecutions] CHECK CONSTRAINT [FK_TradeExecutions_Positions]
GO
USE [master]
GO
ALTER DATABASE [TradingBE] SET  READ_WRITE 
GO

