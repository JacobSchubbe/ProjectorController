using ProjectController.TCPCommunication;

var builder = WebApplication.CreateBuilder(args);

// Register the singleton
// builder.Services.AddSingleton<SingletonService>();

// Add SignalR services
builder.Services.AddSignalR();
builder.Logging.AddConsole();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors(); // Add this before UseEndpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<GUIHub>("/GUIHub");
});

var tcp = new TcpConnection();
tcp.RunCommand();

// Run the application indefinitely
app.Run();