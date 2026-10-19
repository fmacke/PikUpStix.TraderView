# Copilot Instructions

## Project Guidelines
- User prefers refactoring TradingReportService to depend on service abstractions (IPositionService/ITradeExecutionService) and to introduce IHistoricalDataService. Use query services to avoid DI cycles.