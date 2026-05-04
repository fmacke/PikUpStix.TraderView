using IBApi;

namespace IKBR_Report_Puller.IKBR
{
    // This provides the "empty" implementation for the 60+ IBKR methods
    public abstract class DefaultWrapper : EWrapper
    {
        // We only care about these for your CAN SLIM scanner
        public virtual void error(int id, int errorCode, string errorMsg) { }
        public virtual void historicalData(int reqId, Bar bar) { }
        public virtual void historicalDataEnd(int reqId, string start, string end) { }
        public virtual void nextValidId(int orderId) { }

        // --- STUBS (Mandatory for the interface but unused for screening) ---

        public void connectionClosed() { }
        public void managedAccounts(string accountsList) { }
        public void tickPrice(int tickerId, int field, double price, TickAttrib attribs) { }
        public void tickSize(int tickerId, int field, int size) { }
        public void tickString(int tickerId, int tickType, string value) { }
        public void tickGeneric(int tickerId, int tickType, double value) { }
        public void tickEFP(int tickerId, int tickType, double basisPoints, string formattedBasisPoints, double impliedFuture, int holdDays, string futureLastTradeDate, double dividendImpact, double dividendsToLastTradeDate) { }
        public void tickSnapshotEnd(int tickerId) { }
        public void orderStatus(int orderId, string status, double filled, double remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice) { }
        public void openOrder(int orderId, Contract contract, Order order, OrderState orderState) { }
        public void openOrderEnd() { }
        public void winError(string str, int lastError) { }
        public void updateAccountValue(string key, string val, string currency, string accountName) { }
        public void updatePortfolio(Contract contract, double position, double marketPrice, double marketValue, double averageCost, double unrealizedPNL, double realizedPNL, string accountName) { }
        public void updateAccountTime(string timestamp) { }
        public void accountDownloadEnd(string accountName) { }
        public void bondContractDetails(int reqId, ContractDetails contractDetails) { }
        public void contractDetails(int reqId, ContractDetails contractDetails) { }
        public void contractDetailsEnd(int reqId) { }
        public void execDetails(int reqId, Contract contract, Execution execution) { }
        public void execDetailsEnd(int reqId) { }
        public void updateMktDepth(int tickerId, int position, int operation, int side, double price, int size) { }
        public void updateMktDepthL2(int tickerId, int position, string marketMaker, int operation, int side, double price, int size, bool isSmartDepth) { }
        public void updateNewsBulletin(int msgId, int msgType, string message, string origExchange) { }
        public void receiveFA(int faDataType, string xml) { }
        public void scannerParameters(string xml) { }
        public void scannerData(int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark, string projection, string legsStr) { }
        public void scannerDataEnd(int reqId) { }
        public void realtimeBar(int reqId, long time, double open, double high, double low, double close, long volume, double wap, int count) { }
        public void currentTime(long time) { }
        public void fundamentalData(int reqId, string data) { }
        public void deltaNeutralValidation(int reqId, DeltaNeutralContract deltaNeutralContract) { }
        public void tickOptionComputation(int tickerId, int field, double impliedVol, double delta, double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice) { }
        public void accountSummary(int reqId, string account, string tag, string value, string currency) { }
        public void accountSummaryEnd(int reqId) { }
        public void verifyMessageAPI(string apiData) { }
        public void verifyCompleted(bool isSuccessful, string errorText) { }
        public void verifyAndAuthMessageAPI(string apiData, string xyzChallenge) { }
        public void verifyAndAuthCompleted(bool isSuccessful, string errorText) { }
        public void displayGroupList(int reqId, string groups) { }
        public void displayGroupUpdated(int reqId, string contractInfo) { }
        public void position(string account, Contract contract, double pos, double avgCost) { }
        public void positionEnd() { }
        public void symbolSamples(int reqId, ContractDescription[] contractDescriptions) { }
        public void mktDepthExchanges(DepthMktDataDescription[] depthMktDataDescriptions) { }
        public void tickNews(int tickerId, long timeStamp, string providerCode, string articleId, string headline, string extraData) { }
        public void smartComponents(int reqId, Dictionary<int, KeyValuePair<string, char>> theMap) { }
        public void tickReqParams(int tickerId, double minTick, string bboExchange, int snapshotPermissions) { }
        public void newsProviders(NewsProvider[] newsProviders) { }
        public void newsArticle(int requestId, int articleType, string articleText) { }
        public void historicalNews(int requestId, string time, string providerCode, string articleId, string headline) { }
        public void historicalNewsEnd(int requestId, bool hasMore) { }
        public void headTimestamp(int reqId, string headTimestamp) { }
        public void histogramData(int reqId, List<KeyValuePair<double, long>> data) { }
        public void rerouteMktDataReq(int reqId, int conId, string exchange) { }
        public void rerouteMktDepthReq(int reqId, int conId, string exchange) { }
        public void marketRule(int marketRuleId, PriceIncrement[] priceIncrements) { }
        public void pnl(int reqId, double dailyPnL, double unrealizedPnL, double realizedPnL) { }
        public void pnlSingle(int reqId, int pos, double dailyPnL, double unrealizedPnL, double realizedPnL, double value) { }
        public void historicalTicks(int reqId, HistoricalTick[] ticks, bool done) { }
        public void historicalTicksBidAsk(int reqId, HistoricalTickBidAsk[] ticks, bool done) { }
        public void historicalTicksLast(int reqId, HistoricalTickLast[] ticks, bool done) { }
        public void tickByTickAllLast(int reqId, int tickType, long time, double price, int size, TickAttribLast tickAttribLast, string exchange, string specialConditions) { }
        public void tickByTickBidAsk(int reqId, long time, double bidPrice, double askPrice, int bidSize, int askSize, TickAttribBidAsk tickAttribBidAsk) { }
        public void tickByTickMidPoint(int reqId, long time, double midPoint) { }
        public void orderBound(long orderId, int apiClientId, int apiOrderId) { }
        public void completedOrder(Contract contract, Order order, OrderState orderState) { }
        public void completedOrdersEnd() { }

        public virtual void error(Exception e)
        {
            Console.WriteLine($"[IBKR Error Exception] {e.Message}");
        }

        public virtual void error(string str)
        {
            Console.WriteLine($"[IBKR Error] {str}");
        }

        public void commissionReport(CommissionReport commissionReport)
        {
            // Not used for historical data requests
        }

        public void historicalDataUpdate(int reqId, Bar bar)
        {
            // Not used for non-streaming historical data requests
        }

        public void marketDataType(int reqId, int marketDataType)
        {
            // Not used for historical data requests
        }

        public void connectAck()
        {
        }

        public void positionMulti(int requestId, string account, string modelCode, Contract contract, double pos, double avgCost)
        {
            // Not used for historical data requests
        }

        public void positionMultiEnd(int requestId)
        {
            // Not used for historical data requests
        }

        public void accountUpdateMulti(int requestId, string account, string modelCode, string key, string value, string currency)
        {
            // Not used for historical data requests
        }

        public void accountUpdateMultiEnd(int requestId)
        {
            // Not used for historical data requests
        }

        public void securityDefinitionOptionParameter(int reqId, string exchange, int underlyingConId, string tradingClass, string multiplier, HashSet<string> expirations, HashSet<double> strikes)
        {
            // Not used for historical data requests
        }

        public void securityDefinitionOptionParameterEnd(int reqId)
        {
            // Not used for historical data requests
        }

        public void softDollarTiers(int reqId, SoftDollarTier[] tiers)
        {
            // Not used for historical data requests
        }

        public void familyCodes(FamilyCode[] familyCodes)
        {
            // Not used for historical data requests
        }

        public void histogramData(int reqId, HistogramEntry[] data)
        {
            // Not used for historical data requests
        }
    }
}