using System;
using System.Collections.Generic;
using System.Linq;
using IBApi;
using DomainBar = IKBR_Report_Puller.Domain.Bar;

namespace IKBR_Report_Puller.IKBR
{
    /// <summary>
    /// Converts between IBApi.Bar and Domain.Bar types
    /// </summary>
    public static class BarConverter
    {
        /// <summary>
        /// Converts a single IBApi.Bar to Domain.Bar
        /// </summary>
        /// <param name="ibkrBar">The IBKR API bar object</param>
        /// <param name="instrumentId">The instrument ID to associate with the bar</param>
        /// <returns>A Domain.Bar object</returns>
        public static DomainBar ConvertToDomainBar(Bar ibkrBar, int instrumentId)
        {
            if (ibkrBar == null)
                throw new ArgumentNullException(nameof(ibkrBar));

            return new DomainBar
            {
                Date = ParseIbkrDate(ibkrBar.Time),
                OpenPrice = ibkrBar.Open,
                HighPrice = ibkrBar.High,
                LowPrice = ibkrBar.Low,
                ClosePrice = ibkrBar.Close,
                Volume = ibkrBar.Volume,
                Settle = 0, // IBKR doesn't provide settle price for historical data
                OpenInterest = 0, // IBKR doesn't provide open interest in historical bars
                InstrumentId = instrumentId
            };
        }

        /// <summary>
        /// Converts a list of IBApi.Bar objects to Domain.Bar objects
        /// </summary>
        /// <param name="ibkrBars">The list of IBKR API bar objects</param>
        /// <param name="instrumentId">The instrument ID to associate with all bars</param>
        /// <returns>A list of Domain.Bar objects</returns>
        public static List<DomainBar> ConvertToDomainBars(List<Bar> ibkrBars, int instrumentId)
        {
            if (ibkrBars == null)
                throw new ArgumentNullException(nameof(ibkrBars));

            return ibkrBars
                .Where(bar => bar != null)
                .Select(bar => ConvertToDomainBar(bar, instrumentId))
                .ToList();
        }

        /// <summary>
        /// Parses IBKR's date format to DateTime
        /// IBKR returns dates in format: "yyyyMMdd" or "yyyyMMdd HH:mm:ss"
        /// </summary>
        /// <param name="ibkrDateString">The IBKR date string</param>
        /// <returns>Parsed DateTime object</returns>
        private static DateTime ParseIbkrDate(string ibkrDateString)
        {
            if (string.IsNullOrWhiteSpace(ibkrDateString))
                throw new ArgumentException("IBKR date string cannot be null or empty", nameof(ibkrDateString));

            // Try parsing with time component first (format: "yyyyMMdd HH:mm:ss")
            if (DateTime.TryParseExact(ibkrDateString, "yyyyMMdd HH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime dateTimeResult))
            {
                return dateTimeResult;
            }

            // Try parsing date only (format: "yyyyMMdd")
            if (DateTime.TryParseExact(ibkrDateString, "yyyyMMdd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime dateResult))
            {
                return dateResult;
            }

            // Fallback: try parsing with standard DateTime.Parse
            if (DateTime.TryParse(ibkrDateString, out DateTime fallbackResult))
            {
                return fallbackResult;
            }

            throw new FormatException($"Unable to parse IBKR date string: {ibkrDateString}");
        }
    }
}
