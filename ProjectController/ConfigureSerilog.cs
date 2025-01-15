using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace ProjectController;

public class ConfigureSerilog
{
    public static void Configure(WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose() // Ensure debug-level logs are captured
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Capture only Warning and above for Microsoft libraries
            .WriteTo.Console() // Logs to the console
            .WriteTo.File(
                new JsonFormatter(),
                path: $"/app/logs/app-log-{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.json", // Adds date and time to file name
                fileSizeLimitBytes: 300 * 1024 * 1024, // 300 MB limit
                rollOnFileSizeLimit: true, // Create a new file when size limit is reached
                retainedFileCountLimit: 5 // Optional: Keep only the latest 5 files
            )
            .CreateLogger();
        
        builder.Host.UseSerilog();
    }
}