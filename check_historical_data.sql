-- Check if HistoricalData table has any data
SELECT COUNT(*) as TotalRecords FROM HistoricalData;

-- Check which instruments have historical data
SELECT TOP 10
	h.InstrumentId, 
	i.InstrumentName,
	i.DataName,
	COUNT(*) as RecordCount,
	MIN(h.Date) as EarliestDate,
	MAX(h.Date) as LatestDate
FROM HistoricalData h
LEFT JOIN Instruments i ON h.InstrumentId = i.Id
GROUP BY h.InstrumentId, i.InstrumentName, i.DataName
ORDER BY RecordCount DESC;

-- Check if instrument 1145 (BP) has any data
SELECT COUNT(*) as BP_RecordCount 
FROM HistoricalData 
WHERE InstrumentId = 1145;
