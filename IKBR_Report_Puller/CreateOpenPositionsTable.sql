USE [TradingBE]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[OpenPositions](
	[whenGenerated] [datetime] NOT NULL,
	[accountId] [varchar](50) NULL,
	[acctAlias] [varchar](50) NULL,
	[model] [varchar](50) NULL,
	[currency] [varchar](3) NULL,
	[fxRateToBase] [decimal](19, 8) NULL,
	[assetCategory] [varchar](10) NULL,
	[subCategory] [varchar](50) NULL,
	[symbol] [varchar](20) NULL,
	[description] [varchar](255) NULL,
	[conid] [bigint] NULL,
	[securityID] [varchar](50) NULL,
	[securityIDType] [varchar](20) NULL,
	[cusip] [varchar](9) NULL,
	[isin] [varchar](12) NULL,
	[figi] [varchar](12) NULL,
	[listingExchange] [varchar](50) NULL,
	[underlyingConid] [varchar](50) NULL,
	[underlyingSymbol] [varchar](20) NULL,
	[underlyingSecurityID] [varchar](50) NULL,
	[underlyingListingExchange] [varchar](50) NULL,
	[issuer] [varchar](100) NULL,
	[issuerCountryCode] [varchar](2) NULL,
	[multiplier] [int] NULL,
	[strike] [decimal](19, 4) NULL,
	[expiry] [varchar](8) NULL,
	[putCall] [varchar](1) NULL,
	[principalAdjustFactor] [decimal](19, 8) NULL,
	[reportDate] [date] NULL,
	[position] [decimal](19, 4) NULL,
	[markPrice] [decimal](19, 4) NULL,
	[positionValue] [decimal](19, 4) NULL,
	[openPrice] [decimal](19, 8) NULL,
	[costBasisPrice] [decimal](19, 8) NULL,
	[costBasisMoney] [decimal](19, 4) NULL,
	[percentOfNAV] [decimal](19, 4) NULL,
	[fifoPnlUnrealized] [decimal](19, 4) NULL,
	[side] [varchar](10) NULL,
	[levelOfDetail] [varchar](50) NULL,
	[openDateTime] [varchar](17) NULL,
	[holdingPeriodDateTime] [varchar](17) NULL,
	[vestingDate] [date] NULL,
	[code] [varchar](50) NULL,
	[originatingOrderID] [bigint] NULL,
	[originatingTransactionID] [bigint] NULL,
	[accruedInt] [decimal](19, 4) NULL,
	[serialNumber] [varchar](50) NULL,
	[deliveryType] [varchar](50) NULL,
	[commodityType] [varchar](50) NULL,
	[fineness] [decimal](19, 4) NULL,
	[weight] [decimal](19, 4) NULL
) ON [PRIMARY]
GO
