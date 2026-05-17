-- =============================================
-- Economic Calendar Table Creation Script
-- Description: Creates table to store economic calendar events from Financial Modeling Prep API
-- =============================================

-- Drop table if it exists (uncomment if you want to recreate)
-- DROP TABLE IF EXISTS dbo.EconomicCalendar;

CREATE TABLE dbo.EconomicCalendar
(
	Id INT IDENTITY(1,1) NOT NULL,
	Date DATETIME2(7) NOT NULL,
	Country NVARCHAR(50) NULL,
	Event NVARCHAR(500) NOT NULL,
	Currency NVARCHAR(10) NULL,
	Previous DECIMAL(18,4) NULL,
	Estimate DECIMAL(18,4) NULL,
	Actual DECIMAL(18,4) NULL,
	Change DECIMAL(18,4) NULL,
	Impact NVARCHAR(50) NULL,
	ChangePercentage DECIMAL(18,4) NULL,
	Unit NVARCHAR(50) NULL,
	CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
	UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),

	CONSTRAINT PK_EconomicCalendar PRIMARY KEY CLUSTERED (Id ASC),
	CONSTRAINT UQ_EconomicCalendar_DateCountryEvent UNIQUE (Date, Country, Event)
);

-- Create index for date-based queries
CREATE NONCLUSTERED INDEX IX_EconomicCalendar_Date
ON dbo.EconomicCalendar (Date DESC);

-- Create index for country-based queries
CREATE NONCLUSTERED INDEX IX_EconomicCalendar_Country
ON dbo.EconomicCalendar (Country);

-- Create index for impact-based queries
CREATE NONCLUSTERED INDEX IX_EconomicCalendar_Impact
ON dbo.EconomicCalendar (Impact)
WHERE Impact IS NOT NULL;

GO

-- Grant permissions (adjust as needed for your environment)
-- GRANT SELECT, INSERT, UPDATE ON dbo.EconomicCalendar TO [YourApplicationUser];
-- GO

PRINT 'Economic Calendar table created successfully';
