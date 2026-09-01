using Microsoft.EntityFrameworkCore;
using TraderView.Domain.Entities;
using TraderView.Domain.Entities.FMP;

namespace TraderView.Infrastructure.DbContexts
{

    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CanSlimCandidate> CanSlimCandidates { get; set; }

        //public virtual DbSet<CanSlimCandidateAnnualHistory> CanSlimCandidateAnnualHistories { get; set; }

        public virtual DbSet<CanSlimScreenerSnapshot> CanSlimScreenerSnapshots { get; set; }

        public virtual DbSet<EconomicCalendar> EconomicCalendars { get; set; }

        public virtual DbSet<HistoricalDatum> HistoricalData { get; set; }

        public virtual DbSet<Instrument> Instruments { get; set; }

        public virtual DbSet<ListItem> Lists { get; set; }

        public virtual DbSet<Note> Notes { get; set; }

        public virtual DbSet<Position> Positions { get; set; }

        public virtual DbSet<Strategy> Strategies { get; set; }

        public virtual DbSet<StrategyStage> StrategyStages { get; set; }

        public virtual DbSet<StrategyStageStep> StrategyStageSteps { get; set; }

        public virtual DbSet<StrategyStageStepsRule> StrategyStageStepsRules { get; set; }

        public virtual DbSet<TradeExecution> TradeExecutions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
            => optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=TradingBE;Persist Security Info=True;User ID=sa;Password=gogogo123!;Pooling=False;MultipleActiveResultSets=False;Encrypt= True;TrustServerCertificate=True;Application Name=SQL Server Management Studio;Command Timeout=0");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CanSlimCandidate>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_CanSlimCandidateSnapshots");

                entity.HasIndex(e => new { e.PassesBoth, e.EvaluationDateUtc }, "IX_CanSlimCandidateSnapshots_PassesBoth").IsDescending(false, true);

                entity.HasIndex(e => new { e.Symbol, e.EvaluationDateUtc }, "IX_CanSlimCandidateSnapshots_Symbol_Date").IsDescending(false, true);

                entity.Property(e => e.AnnualEpsCagr3YearPercent)
                    .HasColumnType("decimal(9, 4)")
                    .HasColumnName("Annual_EpsCagr3YearPercent");
                entity.Property(e => e.AnnualEpsCagr5YearPercent)
                    .HasColumnType("decimal(9, 4)")
                    .HasColumnName("Annual_EpsCagr5YearPercent");
                entity.Property(e => e.AnnualFundamentalGrade)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .HasDefaultValue("N/A", "DF__CanSlimCa__Annua__6D0D32F4")
                    .HasColumnName("Annual_FundamentalGrade");
                entity.Property(e => e.AnnualHasConsecutiveAnnualGrowth).HasColumnName("Annual_HasConsecutiveAnnualGrowth");
                entity.Property(e => e.AnnualLatestFiscalYear)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("Annual_LatestFiscalYear");
                entity.Property(e => e.AnnualLatestFiscalYearEps)
                    .HasColumnType("decimal(18, 4)")
                    .HasColumnName("Annual_LatestFiscalYearEps");
                entity.Property(e => e.AnnualOperatingMarginPercent)
                    .HasColumnType("decimal(9, 4)")
                    .HasColumnName("Annual_OperatingMarginPercent");
                entity.Property(e => e.AnnualPassesCriteria).HasColumnName("Annual_PassesCriteria");
                entity.Property(e => e.AnnualPriorYear1Eps)
                    .HasColumnType("decimal(18, 4)")
                    .HasColumnName("Annual_PriorYear1Eps");
                entity.Property(e => e.AnnualPriorYear2Eps)
                    .HasColumnType("decimal(18, 4)")
                    .HasColumnName("Annual_PriorYear2Eps");
                entity.Property(e => e.AnnualPriorYear3Eps)
                    .HasColumnType("decimal(18, 4)")
                    .HasColumnName("Annual_PriorYear3Eps");
                entity.Property(e => e.AnnualReturnOnAssetsPercent)
                    .HasColumnType("decimal(9, 4)")
                    .HasColumnName("Annual_ReturnOnAssetsPercent");
                entity.Property(e => e.AnnualReturnOnEquityPercent)
                    .HasColumnType("decimal(9, 4)")
                    .HasColumnName("Annual_ReturnOnEquityPercent");
                entity.Property(e => e.CompanyName).HasMaxLength(200);
                entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(sysutcdatetime())", "DF__CanSlimCa__Creat__6EF57B66");
                entity.Property(e => e.CurrentQuarterEpsGrowthYoYpercent)
                    .HasColumnType("decimal(9, 4)")
                    .HasColumnName("CurrentQuarter_EpsGrowthYoYPercent");
                entity.Property(e => e.CurrentQuarterIsAccelerating).HasColumnName("CurrentQuarter_IsAccelerating");
                entity.Property(e => e.CurrentQuarterLatestQuarterDate)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("CurrentQuarter_LatestQuarterDate");
                entity.Property(e => e.CurrentQuarterLatestQuarterEps)
                    .HasColumnType("decimal(18, 4)")
                    .HasColumnName("CurrentQuarter_LatestQuarterEps");
                entity.Property(e => e.CurrentQuarterPassesCriteria).HasColumnName("CurrentQuarter_PassesCriteria");
                entity.Property(e => e.CurrentQuarterPriorYearQuarterEps)
                    .HasColumnType("decimal(18, 4)")
                    .HasColumnName("CurrentQuarter_PriorYearQuarterEps");
                entity.Property(e => e.CurrentQuarterRevenueGrowthYoYpercent)
                    .HasColumnType("decimal(9, 4)")
                    .HasColumnName("CurrentQuarter_RevenueGrowthYoYPercent");
                entity.Property(e => e.EvaluationDateUtc).HasDefaultValueSql("(sysutcdatetime())", "DF__CanSlimCa__Evalu__5CD6CB2B");
                entity.Property(e => e.Exchange)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.Industry).HasMaxLength(100);
                entity.Property(e => e.MarketCap).HasColumnType("decimal(19, 2)");
                entity.Property(e => e.Price).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.Sector).HasMaxLength(100);
                entity.Property(e => e.Symbol)
                    .HasMaxLength(16)
                    .IsUnicode(false);
                entity.Property(e => e.Volume).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.CanSlimScreenerSnapshot).WithMany(p => p.CanSlimCandidates)
                    .HasForeignKey(d => d.CanSlimScreenerSnapshotId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CanSlimCandidates_CanSlimScreenerSnapshot");
            });

            modelBuilder.Entity<CanSlimCandidateAnnualHistory>(entity =>
            {
                entity.ToTable("CanSlimCandidateAnnualHistory");

                entity.HasIndex(e => e.CandidateId, "IX_CanSlimCandidateAnnualHistory_SnapshotId");

                entity.Property(e => e.CalendarYear)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.EpsDiluted).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.EpsGrowthYoYpercent)
                    .HasColumnType("decimal(9, 4)")
                    .HasColumnName("EpsGrowthYoYPercent");
                entity.Property(e => e.FiscalDate)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.NetIncome).HasColumnType("decimal(19, 2)");
                entity.Property(e => e.Revenue).HasColumnType("decimal(19, 2)");

                entity.HasOne(d => d.Candidate).WithMany(p => p.CanSlimCandidateAnnualHistories)
                    .HasForeignKey(d => d.CandidateId)
                    .HasConstraintName("FK_CanSlimCandidateAnnualHistory_CandidateSnapshot");
            });

            modelBuilder.Entity<CanSlimScreenerSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_CanSlimScreenerSnapshot");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<EconomicCalendar>(entity =>
            {
                entity.ToTable("EconomicCalendar");

                entity.HasIndex(e => e.Country, "IX_EconomicCalendar_Country");

                entity.HasIndex(e => e.Date, "IX_EconomicCalendar_Date").IsDescending();

                entity.HasIndex(e => new { e.Date, e.Country, e.Event }, "UQ_EconomicCalendar_DateCountryEvent").IsUnique();

                entity.Property(e => e.Actual).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.Change).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.ChangePercentage).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.Country).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.Estimate).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.Event).HasMaxLength(500);
                entity.Property(e => e.Impact).HasMaxLength(50);
                entity.Property(e => e.Previous).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.Unit).HasMaxLength(50);
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            });

            modelBuilder.Entity<HistoricalDatum>(entity =>
            {
                entity.HasIndex(e => e.Date, "IX_HistoricalData_Date").IsDescending();

                entity.HasIndex(e => new { e.InstrumentId, e.Date }, "IX_HistoricalData_InstrumentId").IsDescending(false, true);

                entity.HasIndex(e => new { e.InstrumentId, e.Date }, "UQ_HistoricalData_InstrumentDate").IsUnique();

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.HasOne(d => d.Instrument).WithMany(p => p.HistoricalData)
                    .HasForeignKey(d => d.InstrumentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HistoricalData_Instruments");
            });

            modelBuilder.Entity<Instrument>(entity =>
            {
                entity.Property(e => e.ConId).HasMaxLength(50);
                entity.Property(e => e.ContractUnitType).HasMaxLength(100);
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.DataName).HasMaxLength(255);
                entity.Property(e => e.DataSource).HasMaxLength(100);
                entity.Property(e => e.Format).HasMaxLength(50);
                entity.Property(e => e.Frequency).HasMaxLength(50);
                entity.Property(e => e.InstrumentName).HasMaxLength(255);
                entity.Property(e => e.ListingExchange).HasMaxLength(50);
                entity.Property(e => e.PriceQuotation).HasMaxLength(100);
                entity.Property(e => e.Provider).HasMaxLength(100);
            });

            modelBuilder.Entity<ListItem>(entity =>
            {
                entity.HasIndex(e => e.Name, "IX_Lists_Name");

                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            });

            modelBuilder.Entity<Note>(entity =>
            {
                entity.HasIndex(e => e.PositionId, "IX_Notes_PositionId");

                entity.Property(e => e.EntryDate).HasDefaultValueSql("(getutcdate())");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

                entity.HasOne(d => d.Position).WithMany(p => p.Notes)
                    .HasForeignKey(d => d.PositionId)
                    .HasConstraintName("FK_Notes_Positions");
            });

            modelBuilder.Entity<Position>(entity =>
            {
                entity.HasIndex(e => e.InstrumentId, "IX_Positions_InstrumentId");

                entity.HasIndex(e => e.OpenDate, "IX_Positions_OpenDate").IsDescending();

                entity.HasIndex(e => e.Status, "IX_Positions_Status");

                entity.Property(e => e.LastReportedPrice).HasColumnType("decimal(18, 6)");
                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Open");

                entity.HasOne(d => d.Instrument).WithMany(p => p.Positions)
                    .HasForeignKey(d => d.InstrumentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Positions_Instruments");
            });

            modelBuilder.Entity<Strategy>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<StrategyStage>(entity =>
            {
                entity.Property(e => e.DataMapping)
                    .HasMaxLength(1000)
                    .IsUnicode(false);
                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsUnicode(false);
                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.Notes)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.HasOne(d => d.Category).WithMany(p => p.StrategyStages)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK_StrategyStages_Lists");

                entity.HasOne(d => d.Strategy).WithMany(p => p.StrategyStages)
                    .HasForeignKey(d => d.StrategyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StrategyStages_Strategies");
            });

            modelBuilder.Entity<StrategyStageStep>(entity =>
            {
                entity.Property(e => e.DataMapping)
                    .HasMaxLength(1000)
                    .IsUnicode(false);
                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsUnicode(false);
                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsUnicode(false);
                entity.Property(e => e.Notes)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.HasOne(d => d.Category).WithMany(p => p.StrategyStageSteps)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK_StrategyStageSteps_Lists");

                entity.HasOne(d => d.Stage).WithMany(p => p.StrategyStageSteps)
                    .HasForeignKey(d => d.StageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StrategyStageSteps_StrategyStages");
            });

            modelBuilder.Entity<StrategyStageStepsRule>(entity =>
            {
                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);
                entity.Property(e => e.NumberValue1).HasColumnType("decimal(18, 5)");
                entity.Property(e => e.NumberValue2).HasColumnType("decimal(18, 5)");
                entity.Property(e => e.NumberValue3).HasColumnType("decimal(18, 5)");

                entity.HasOne(d => d.Step).WithMany(p => p.StrategyStageStepsRules)
                    .HasForeignKey(d => d.StepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StrategyStageStepsRules_StrategyStageSteps");
            });

            modelBuilder.Entity<TradeExecution>(entity =>
            {
                entity.Property(e => e.AccountId)
                    .HasMaxLength(50)
                    .HasColumnName("accountId");
                entity.Property(e => e.AccruedInt)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("accruedInt");
                entity.Property(e => e.AcctAlias)
                    .HasMaxLength(50)
                    .HasColumnName("acctAlias");
                entity.Property(e => e.AssetCategory)
                    .HasMaxLength(50)
                    .HasColumnName("assetCategory");
                entity.Property(e => e.BrokerageOrderId)
                    .HasMaxLength(100)
                    .HasColumnName("brokerageOrderID");
                entity.Property(e => e.BuySell)
                    .HasMaxLength(10)
                    .HasColumnName("buySell");
                entity.Property(e => e.ChangeInPrice)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("changeInPrice");
                entity.Property(e => e.ChangeInQuantity)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("changeInQuantity");
                entity.Property(e => e.ClearingFirmId)
                    .HasMaxLength(50)
                    .HasColumnName("clearingFirmID");
                entity.Property(e => e.ClosePrice)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("closePrice");
                entity.Property(e => e.CommodityType)
                    .HasMaxLength(50)
                    .HasColumnName("commodityType");
                entity.Property(e => e.Conid)
                    .HasMaxLength(50)
                    .HasColumnName("conid");
                entity.Property(e => e.Cost)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("cost");
                entity.Property(e => e.Currency)
                    .HasMaxLength(10)
                    .HasColumnName("currency");
                entity.Property(e => e.Cusip)
                    .HasMaxLength(50)
                    .HasColumnName("cusip");
                entity.Property(e => e.DateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("dateTime");
                entity.Property(e => e.DeliveryType)
                    .HasMaxLength(50)
                    .HasColumnName("deliveryType");
                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");
                entity.Property(e => e.ExchOrderId)
                    .HasMaxLength(100)
                    .HasColumnName("exchOrderId");
                entity.Property(e => e.Exchange)
                    .HasMaxLength(50)
                    .HasColumnName("exchange");
                entity.Property(e => e.Expiry)
                    .HasMaxLength(50)
                    .HasColumnName("expiry");
                entity.Property(e => e.ExtExecId)
                    .HasMaxLength(100)
                    .HasColumnName("extExecID");
                entity.Property(e => e.FifoPnlRealized)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("fifoPnlRealized");
                entity.Property(e => e.Figi)
                    .HasMaxLength(50)
                    .HasColumnName("figi");
                entity.Property(e => e.Fineness)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("fineness");
                entity.Property(e => e.FxRateToBase)
                    .HasColumnType("decimal(18, 10)")
                    .HasColumnName("fxRateToBase");
                entity.Property(e => e.HoldingPeriodDateTime)
                    .HasMaxLength(50)
                    .HasColumnName("holdingPeriodDateTime");
                entity.Property(e => e.IbCommission)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("ibCommission");
                entity.Property(e => e.IbCommissionCurrency)
                    .HasMaxLength(10)
                    .HasColumnName("ibCommissionCurrency");
                entity.Property(e => e.IbExecId)
                    .HasMaxLength(100)
                    .HasColumnName("ibExecID");
                entity.Property(e => e.IbOrderId).HasColumnName("ibOrderID");
                entity.Property(e => e.InitialInvestment)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("initialInvestment");
                entity.Property(e => e.IsApiorder)
                    .HasMaxLength(10)
                    .HasColumnName("isAPIOrder");
                entity.Property(e => e.Isin)
                    .HasMaxLength(50)
                    .HasColumnName("isin");
                entity.Property(e => e.Issuer)
                    .HasMaxLength(100)
                    .HasColumnName("issuer");
                entity.Property(e => e.IssuerCountryCode)
                    .HasMaxLength(10)
                    .HasColumnName("issuerCountryCode");
                entity.Property(e => e.LevelOfDetail)
                    .HasMaxLength(50)
                    .HasColumnName("levelOfDetail");
                entity.Property(e => e.ListingExchange)
                    .HasMaxLength(50)
                    .HasColumnName("listingExchange");
                entity.Property(e => e.Model)
                    .HasMaxLength(50)
                    .HasColumnName("model");
                entity.Property(e => e.MtmPnl)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("mtmPnl");
                entity.Property(e => e.Multiplier).HasColumnName("multiplier");
                entity.Property(e => e.NetCash)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("netCash");
                entity.Property(e => e.Notes).HasColumnName("notes");
                entity.Property(e => e.OpenCloseIndicator)
                    .HasMaxLength(10)
                    .HasColumnName("openCloseIndicator");
                entity.Property(e => e.OpenDateTime)
                    .HasMaxLength(50)
                    .HasColumnName("openDateTime");
                entity.Property(e => e.OrderReference)
                    .HasMaxLength(100)
                    .HasColumnName("orderReference");
                entity.Property(e => e.OrderTime)
                    .HasMaxLength(50)
                    .HasColumnName("orderTime");
                entity.Property(e => e.OrderType)
                    .HasMaxLength(50)
                    .HasColumnName("orderType");
                entity.Property(e => e.OrigOrderId).HasColumnName("origOrderID");
                entity.Property(e => e.OrigTradeDate)
                    .HasMaxLength(50)
                    .HasColumnName("origTradeDate");
                entity.Property(e => e.OrigTradeId)
                    .HasMaxLength(50)
                    .HasColumnName("origTradeID");
                entity.Property(e => e.OrigTradePrice)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("origTradePrice");
                entity.Property(e => e.OrigTransactionId).HasColumnName("origTransactionID");
                entity.Property(e => e.PositionActionId)
                    .HasMaxLength(50)
                    .HasColumnName("positionActionID");
                entity.Property(e => e.PrincipalAdjustFactor)
                    .HasColumnType("decimal(18, 10)")
                    .HasColumnName("principalAdjustFactor");
                entity.Property(e => e.Proceeds)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("proceeds");
                entity.Property(e => e.PutCall)
                    .HasMaxLength(10)
                    .HasColumnName("putCall");
                entity.Property(e => e.Quantity)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("quantity");
                entity.Property(e => e.RelatedTradeId)
                    .HasMaxLength(50)
                    .HasColumnName("relatedTradeID");
                entity.Property(e => e.RelatedTransactionId)
                    .HasMaxLength(50)
                    .HasColumnName("relatedTransactionID");
                entity.Property(e => e.ReportDate)
                    .HasMaxLength(50)
                    .HasColumnName("reportDate");
                entity.Property(e => e.Rtn)
                    .HasMaxLength(50)
                    .HasColumnName("rtn");
                entity.Property(e => e.SecurityId)
                    .HasMaxLength(50)
                    .HasColumnName("securityID");
                entity.Property(e => e.SecurityIdtype)
                    .HasMaxLength(50)
                    .HasColumnName("securityIDType");
                entity.Property(e => e.SerialNumber)
                    .HasMaxLength(50)
                    .HasColumnName("serialNumber");
                entity.Property(e => e.SettleDateTarget)
                    .HasMaxLength(50)
                    .HasColumnName("settleDateTarget");
                entity.Property(e => e.Strike)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("strike");
                entity.Property(e => e.SubCategory)
                    .HasMaxLength(50)
                    .HasColumnName("subCategory");
                entity.Property(e => e.Symbol)
                    .HasMaxLength(50)
                    .HasColumnName("symbol");
                entity.Property(e => e.Taxes)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("taxes");
                entity.Property(e => e.TradeDate)
                    .HasColumnType("datetime")
                    .HasColumnName("tradeDate");
                entity.Property(e => e.TradeId).HasColumnName("tradeID");
                entity.Property(e => e.TradeMoney)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("tradeMoney");
                entity.Property(e => e.TradePrice)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("tradePrice");
                entity.Property(e => e.TraderId)
                    .HasMaxLength(50)
                    .HasColumnName("traderID");
                entity.Property(e => e.TransactionId).HasColumnName("transactionID");
                entity.Property(e => e.TransactionType)
                    .HasMaxLength(50)
                    .HasColumnName("transactionType");
                entity.Property(e => e.UnderlyingConid)
                    .HasMaxLength(50)
                    .HasColumnName("underlyingConid");
                entity.Property(e => e.UnderlyingListingExchange)
                    .HasMaxLength(50)
                    .HasColumnName("underlyingListingExchange");
                entity.Property(e => e.UnderlyingSecurityId)
                    .HasMaxLength(50)
                    .HasColumnName("underlyingSecurityID");
                entity.Property(e => e.UnderlyingSymbol)
                    .HasMaxLength(50)
                    .HasColumnName("underlyingSymbol");
                entity.Property(e => e.VolatilityOrderLink)
                    .HasMaxLength(100)
                    .HasColumnName("volatilityOrderLink");
                entity.Property(e => e.Weight)
                    .HasColumnType("decimal(18, 6)")
                    .HasColumnName("weight");
                entity.Property(e => e.WhenRealized)
                    .HasMaxLength(50)
                    .HasColumnName("whenRealized");
                entity.Property(e => e.WhenReopened)
                    .HasMaxLength(50)
                    .HasColumnName("whenReopened");

                entity.HasOne(d => d.Position).WithMany(p => p.TradeExecutions)
                    .HasForeignKey(d => d.PositionId)
                    .HasConstraintName("FK_TradeExecutions_Positions");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

}