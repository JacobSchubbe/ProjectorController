using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace ProjectController;

public class ConfigureSerilog
{
    public static void Configure(WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration) // Reads logging level from environment variables
            .MinimumLevel.Debug() // Ensure debug-level logs are captured
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Capture only Warning and above for Microsoft libraries
            .WriteTo.Console() // Logs to the console
            .WriteTo.File(
                new JsonFormatter(),
                path: $"/app/logs/app-log-{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.json", // Adds date and time to file name
                fileSizeLimitBytes: 300 * 1024 * 1024, // 300 MB limit
                rollOnFileSizeLimit: true, // Create a new file when size limit is reached
                retainedFileCountLimit: 5 // Optional: Keep only the latest 5 files
            )
            .Enrich.With(new CallStackEnricher()) // Add the CallStackEnricher
            .CreateLogger();
        
        builder.Host.UseSerilog();
    }
}

public class CallStackEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Get the current stack trace
        var callStack = Environment.StackTrace;

        // Add the stack trace as a property to the log event
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CallStack", callStack));
    }
}