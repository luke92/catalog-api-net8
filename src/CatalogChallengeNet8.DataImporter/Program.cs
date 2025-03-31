
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

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console() 
            .CreateBootstrapLogger();

        Log.Information("Starting Data Importer application...");

        try
        {
            if (args.Length < 1)
            {
                Log.Error("Error: Please provide the CSV file path as an argument.");
                Console.WriteLine("Usage: dotnet run <path_to_your_csv_file>");
                return;
            }
            var filePath = args[0];

            if (!File.Exists(filePath))
            {
                Log.Error("Error: File does not exist at the specified path: {FilePath}", filePath);
                return;
            }

            // --- Configure Host ---
            var host = Host.CreateDefaultBuilder(args)
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

            await RunImportAsync(host.Services, filePath);

        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly.");
        }
        finally
        {
            Log.Information("Shutting down Data Importer application.");
            await Log.CloseAndFlushAsync();
        }
    }

    static async Task RunImportAsync(IServiceProvider services, string filePath)
    {
        // Create a scope for scoped services like DbContext and Repositories
        using (var scope = services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            try
            {
                var orchestrator = serviceProvider.GetRequiredService<DataImportOrchestrator>();
                await orchestrator.ImportDataAsync(filePath);
                Log.Information("Import process finished.");
            }
            catch (Exception ex)
            {
                // Log errors originating from the orchestrator or DI resolution
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred during the data import process.");
            }
        }
    }
}