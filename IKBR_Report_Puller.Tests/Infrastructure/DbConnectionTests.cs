using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using TraderView.Infrastructure.DbContexts;

namespace IKBR_Report_Puller.Tests.Infrastructure
{
    [TestClass]
    public class DbConnectionTests
    {
        [TestMethod]
        public async Task AppDbContext_CanConnect_ToDatabase()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            string BuildConnectionString()
            {
                var dbUser = config["Database:User"];
                var dbPassword = config["Database:Password"];
                var dbHost = config["Database:Host"];
                var dbName = config["Database:DbName"];
                    return $"Server={dbHost};Database={dbName};User ID={dbUser};Password={dbPassword};TrustServerCertificate=True;";
                }

                var connectionString = BuildConnectionString();

                // If configuration is missing, mark the test inconclusive instead of failing.
                if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("Server=;"))
                {
                    Assert.Inconclusive("Database connection not configured in appsettings. Skipping connection test.");
                }

                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                using var db = new AppDbContext(options);

            // Attempt to open the connection to capture detailed exception information if it fails
            try
            {
                await db.Database.OpenConnectionAsync();
                await db.Database.CloseConnectionAsync();
            }
            catch (Exception ex)
            {
                // Fail the test with the full exception so the root cause is visible in test output
                Assert.Fail($"Failed to open database connection. Exception: {ex}");
            }
        }
    }
}
