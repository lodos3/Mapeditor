using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mir2.Core.Models;
using Mir2.Core.Services;
using Mir2.Core.IO;

namespace Mir2.Editor;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Crystal Mir2 Map Editor - Core Demo");
        Console.WriteLine("===================================");

        // Build the host
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<LibraryCatalog>();
                services.AddTransient<MapReader>();
                services.AddTransient<MapWriter>();
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var libraryCatalog = host.Services.GetRequiredService<LibraryCatalog>();
        var mapReader = host.Services.GetRequiredService<MapReader>();
        var mapWriter = host.Services.GetRequiredService<MapWriter>();

        logger.LogInformation("Starting Crystal Mir2 Map Editor");

        try
        {
            // Demo: Scan for libraries
            Console.WriteLine("Scanning for libraries...");
            await libraryCatalog.ScanLibrariesAsync();

            var wemadeMir2Libs = libraryCatalog.GetLibrariesByType(LibraryType.WemadeMir2).ToList();
            var shandaMir2Libs = libraryCatalog.GetLibrariesByType(LibraryType.ShandaMir2).ToList();
            var wemadeMir3Libs = libraryCatalog.GetLibrariesByType(LibraryType.WemadeMir3).ToList();

            Console.WriteLine($"Found {wemadeMir2Libs.Count} Wemade Mir2 libraries");
            Console.WriteLine($"Found {shandaMir2Libs.Count} Shanda Mir2 libraries");
            Console.WriteLine($"Found {wemadeMir3Libs.Count} Wemade Mir3 libraries");

            // Demo: Create and save a test map
            Console.WriteLine("\nCreating test map...");
            var testMap = new MapData(50, 50);
            
            // Set some test data
            testMap.Cells[0, 0] = new CellInfo
            {
                BackIndex = 0,
                BackImage = 1,
                Light = 50
            };

            testMap.Cells[25, 25] = new CellInfo
            {
                BackIndex = 1,
                BackImage = 100,
                MiddleIndex = 2,
                MiddleImage = 200,
                Light = 100 // Fishing zone
            };

            var testMapPath = "test_map.map";
            await mapWriter.WriteAsync(testMap, testMapPath);
            Console.WriteLine($"Test map saved to: {testMapPath}");

            // Demo: Load the test map back
            Console.WriteLine("Loading test map...");
            var loadedMap = await mapReader.ReadAsync(testMapPath);
            
            Console.WriteLine($"Loaded map: {loadedMap.Width}x{loadedMap.Height} ({loadedMap.FormatType})");
            Console.WriteLine($"Cell[0,0]: BackIndex={loadedMap.Cells[0,0].BackIndex}, BackImage={loadedMap.Cells[0,0].BackImage}, Light={loadedMap.Cells[0,0].Light}");
            Console.WriteLine($"Cell[25,25]: BackIndex={loadedMap.Cells[25,25].BackIndex}, MiddleIndex={loadedMap.Cells[25,25].MiddleIndex}, FishingCell={loadedMap.Cells[25,25].FishingCell}");

            // Save configuration
            var configPath = "library_config.json";
            await libraryCatalog.SaveConfigAsync(configPath);
            Console.WriteLine($"Configuration saved to: {configPath}");

            Console.WriteLine("\nCore functionality demonstration completed successfully!");
            logger.LogInformation("Crystal Mir2 Map Editor core demo completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during execution");
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
