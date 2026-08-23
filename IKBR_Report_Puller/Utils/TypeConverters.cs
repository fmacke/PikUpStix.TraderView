using System.Globalization;

namespace TraderView.Application.Utils
{
    /// <summary>
    /// Provides type conversion utilities for database operations
    /// </summary>
    public static class TypeConverters
    {
        public static long? ConvertToLong(string value)
        {
            if (long.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        public static DateTime? ConvertToNullableDate(string dateString)
        {            
            if (DateTime.TryParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return null;
        }
       public static DateTime ConvertStringToDate(string dateString)
        {
            return DateTime.ParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture);
        }
    }
}
