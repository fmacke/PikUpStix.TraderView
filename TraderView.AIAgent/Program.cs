using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using PikUpStix.TraderView.Services;
using PikUpStix.TraderView.Services.MarketData;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Services;
using TraderView.Infrastructure.Data;
using TraderView.Infrastructure.DbContexts;
using TraderView.Infrastructure.Repositories;

namespace TraderView.AIAgent
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        config.AddUserSecrets<Program>();
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Register HttpClient factory to avoid DNS and socket exhaustion issues
                    services.AddHttpClient("IKBR", client =>
                    {
                        client.Timeout = TimeSpan.FromMinutes(5);
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                    })
                    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(14)
                    });

                    // Register DB connection factory from configuration
                    services.AddSingleton<IDbConnectionFactory>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        var connectionString = BuildConnectionString(config);
                        return new SqlConnectionFactory(connectionString);
                    });

                    // Register EF Core DbContext so InstrumentRepository can use AppDbContext when available
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseSqlServer(BuildConnectionString(hostContext.Configuration));
                    });

                    // Register Repositories 
                    services.AddScoped<IPositionRepository>(provider =>
                    {
                        AppDbContext db = provider.GetRequiredService<AppDbContext>();
                        return new PositionRepository(db);
                    });
                    services.AddScoped<IAIReportInstructionRepository>(provider =>
                    {
                        AppDbContext db = provider.GetRequiredService<AppDbContext>();
                        return new AIReportInstructionRepository(db);
                    });
                    services.AddScoped<IAIReportRepository>(provider =>
                    {
                        AppDbContext db = provider.GetRequiredService<AppDbContext>();
                        return new AiReportRepository(db);
                    });

                    // Register Services
                    services.AddScoped<IPositionService, PositionService>();
                    services.AddScoped<IAIReportInstructionService, AIReportInstructionService>();
                    services.AddScoped<IAIReportService, AIReportService>();
                    services.AddScoped<Application>();
                })
                .Build();

            using var scope = host.Services.CreateScope();
            var app = scope.ServiceProvider.GetRequiredService<Application>();
            await app.RunAsync();

        }

        // Helper method to build connection string
        static string BuildConnectionString(IConfiguration config)
        {
            var dbUser = config["Database:User"];
            var dbPassword = config["Database:Password"];
            var dbHost = config["Database:Host"];
            var dbName = config["Database:DbName"];
            return $"Server={dbHost};Database={dbName};User ID={dbUser};Password={dbPassword};TrustServerCertificate=True;";
        }
    }
}