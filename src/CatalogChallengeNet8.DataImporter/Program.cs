
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using CatalogChallengeNet8.Infrastructure;
using CatalogChallengeNet8.Application.Interfaces;
using CatalogChallengeNet8.Application.Services;
using Microsoft.Extensions.Hosting;
using CatalogChallengeNet8.DataImporter;

class Program
{
    static async Task Main(string[] args)
    {

        // --- Configure Host ---
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders(); // Asegura que solo Serilog maneja los logs
            })
            .ConfigureAppConfiguration((context, configBuilder) =>
            {
                // appsettings.json is read by default
            })
            .UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(context.Configuration) // Read Serilog config from appsettings.json
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console()) // Ensure Console sink is configured (can be refined in appsettings)
            .ConfigureServices((hostContext, services) =>
            {
                // Get configuration
                var configuration = hostContext.Configuration;

                // Register Infrastructure Module (DbContext, Generic Repo)
                services.AddInfrastructure(configuration);

                // Register Application Services
                services.AddScoped<ICsvReaderService, CsvReaderService>();

                // Register the main Orchestrator service
                services.AddScoped<DataImportOrchestrator>();

                // Add configuration options
                services.Configure<ImportSettings>(configuration.GetSection("ImportSettings"));

            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting Data Importer application...");

        try
        {
            if (args.Length < 1)
            {
                logger.LogError("Error: Please provide the CSV file path as an argument.");
                Console.WriteLine("Usage: dotnet run <path_to_your_csv_file>");
                return;
            }
            var filePath = args[0];

            if (!File.Exists(filePath))
            {
                logger.LogError("Error: File does not exist at the specified path: {FilePath}", filePath);
                return;
            }

            await RunImportAsync(host.Services, filePath);

        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Application terminated unexpectedly.");
        }
        finally
        {
            logger.LogInformation("Shutting down Data Importer application.");
            await Log.CloseAndFlushAsync();
        }
    }

    static async Task RunImportAsync(IServiceProvider services, string filePath)
    {
        // Create a scope for scoped services like DbContext and Repositories
        using (var scope = services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                var orchestrator = serviceProvider.GetRequiredService<DataImportOrchestrator>();
                await orchestrator.ImportDataAsync(filePath);
                logger.LogInformation("Import process finished.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the data import process.");
            }
        }
    }
}