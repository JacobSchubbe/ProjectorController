using System.Diagnostics;
using Microsoft.Extensions.FileProviders;
using ProjectController.TCPCommunication;

var builder = WebApplication.CreateBuilder(args);

// Register the singleton
builder.Services.AddSingleton<TcpConnection>();
builder.Services.AddSingleton<GUIHub>();

var customConfig = builder.Configuration.GetSection("CustomConfig");
var debugVersion = bool.Parse(customConfig["DebugVersion"]);
string IPAddress = customConfig["IPAddress"];
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
if (allowedOrigins == null)
    throw new ApplicationException("Allowed origins is null");
// allowedOrigins = allowedOrigins.Concat([$"http://{IPAddress}:8080", $"http://{IPAddress}:8081"]).ToArray();

// Add SignalR services
builder.Services.AddSignalR();
builder.Logging.AddConsole();
Console.WriteLine($"Allowed Origins: {string.Join(",", allowedOrigins)}");
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Ensure port proxy is set up (port forwarding for Vue server)
if (debugVersion)
{
}

// Configure Kestrel server to listen on a custom port (optional)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8081);
});

var app = builder.Build();

app.UseRouting();
app.UseCors(); // Add this before UseEndpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<GUIHub>("/GUIHub");
});

// Run the application indefinitely
if (debugVersion)
{
}

app.Run();

#region Proxy Port (Windows only!)

void ConfigurePortProxy()
{
    var netshCommand = "netsh interface portproxy add v4tov4 listenport=8081 connectaddress=192.168.0.153 connectport=8080";
    ProcessNetshCommand(netshCommand);
}

void RemovePortProxy()
{
    var netshCommand = "netsh interface portproxy delete v4tov4 listenport=8081";
    ProcessNetshCommand(netshCommand);
}

void ProcessNetshCommand(string psCommand)
{
    Console.WriteLine("netsh: " + psCommand);
    try
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = psCommand,
            Verb = "runas", // Elevate to admin
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);

        string output = process?.StandardOutput.ReadToEnd().Trim();
        string error = process?.StandardError.ReadToEnd().Trim();

        process?.WaitForExit();

        if (!string.IsNullOrEmpty(output))
        {
            Console.WriteLine("PowerShell Output: " + output);
        }

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine("PowerShell Error: " + error);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error running PowerShell command: {ex.Message}");
    }
}

#endregion
