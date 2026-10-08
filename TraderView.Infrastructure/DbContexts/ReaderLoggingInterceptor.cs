using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TraderView.Infrastructure.DbContexts
{
    public class ReaderLoggingInterceptor : DbCommandInterceptor
    {
        private readonly ILogger<ReaderLoggingInterceptor> _logger;

        public ReaderLoggingInterceptor(ILogger<ReaderLoggingInterceptor> logger)
        {
            _logger = logger;
        }

        public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            try
            {
                LogReaderSchema(command.CommandText, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging reader schema");
            }

            return base.ReaderExecuted(command, eventData, result);
        }

        public override async System.Threading.Tasks.ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                LogReaderSchema(command.CommandText, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging reader schema");
            }

            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        private void LogReaderSchema(string sql, DbDataReader reader)
        {
            if (reader == null || reader.FieldCount == 0) return;

            _logger.LogInformation("Executed SQL: {Sql}", sql);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var type = reader.GetFieldType(i);
                _logger.LogInformation("ColumnOrdinal={Ordinal} Name={Name} Type={Type}", i, name, type.FullName);
            }

            // Optionally log first row's values for diagnosis (safe because of EnableSensitiveDataLogging below)
            if (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var isDbNull = reader.IsDBNull(i);
                    var val = isDbNull ? "<NULL>" : reader.GetValue(i);
                    _logger.LogInformation("Row0 Ordinal={Ordinal} Name={Name} Value={Value}", i, reader.GetName(i), val);
                }
            }
        }
    }
}
