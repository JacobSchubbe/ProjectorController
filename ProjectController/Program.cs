using ProjectController;
using ProjectController.Communication.Serial;
using ProjectController.Communication.Tcp;
using ProjectController.Controllers.ADB;
using ProjectController.Controllers.Projector;
using ProjectController.QueueManagement;
using Serilog;
using static ProjectController.Controllers.Projector.ProjectorConstants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TcpCommunication>();
builder.Services.AddSingleton<SerialCommunication>();
builder.Services.AddSingleton<ADBClient>();
builder.Services.AddSingleton<CommandRunner<ProjectorCommands>>();
builder.Services.AddSingleton<CommandRunner<KeyCodes>>();
builder.Services.AddSingleton<AdbController>();
builder.Services.AddSingleton<ProjectorController>();
builder.Services.AddSingleton<AndroidTVController>();

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