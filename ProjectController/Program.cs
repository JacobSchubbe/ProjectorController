using ProjectController;
using ProjectController.TCPCommunication;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Register the singleton
builder.Services.AddSingleton<TcpConnection>();
builder.Services.AddSingleton<GUIHub>();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
if (allowedOrigins == null)
    throw new ApplicationException("Allowed origins is null");

// Add SignalR services
builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(30); // Server sends keep-alive every 30 seconds
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60); // Client disconnects if no response in 60 seconds
});
builder.Logging.AddConsole();
Log.Debug($"Allowed Origins: {string.Join(",", allowedOrigins)}");
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

// Configure Kestrel server to listen on a custom port (optional)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8081);
});

ConfigureSerilog.Configure(builder);
var app = builder.Build();

app.UseRouting();
app.UseCors(); // Add this before UseEndpoints
app.MapHub<GUIHub>("/GUIHub");

app.Run();

// #region Proxy Port (Windows only!)
//
// void ConfigurePortProxy()
// {
//     var netshCommand = "netsh interface portproxy add v4tov4 listenport=8081 connectaddress=192.168.0.153 connectport=8080";
//     ProcessNetshCommand(netshCommand);
// }
//
// void RemovePortProxy()
// {
//     var netshCommand = "netsh interface portproxy delete v4tov4 listenport=8081";
//     ProcessNetshCommand(netshCommand);
// }
//
// void ProcessNetshCommand(string psCommand)
// {
//     Log.Debug("netsh: " + psCommand);
//     try
//     {
//         var startInfo = new ProcessStartInfo
//         {
//             FileName = "powershell",
//             Arguments = psCommand,
//             Verb = "runas", // Elevate to admin
//             RedirectStandardOutput = true,
//             RedirectStandardError = true,
//             UseShellExecute = false,
//             CreateNoWindow = true
//         };
//
//         using var process = Process.Start(startInfo);
//
//         string output = process?.StandardOutput.ReadToEnd().Trim();
//         string error = process?.StandardError.ReadToEnd().Trim();
//
//         process?.WaitForExit();
//
//         if (!string.IsNullOrEmpty(output))
//         {
//             Log.Debug("PowerShell Output: " + output);
//         }
//
//         if (!string.IsNullOrEmpty(error))
//         {
//             Log.Debug("PowerShell Error: " + error);
//         }
//     }
//     catch (Exception ex)
//     {
//         Log.Debug($"Error running PowerShell command: {ex.Message}");
//     }
// }
//
// #endregion
