using ProjectController.GUI;using ProjectController.TCPCommunication;

var builder = WebApplication.CreateBuilder(args);

// Register the singleton
// builder.Services.AddSingleton<SingletonService>();

// Add SignalR services
builder.Services.AddSignalR();

var app = builder.Build();

// Map SignalR hub
app.MapHub<GUIHub>("/GUIHub");

var tcp = new TcpConnection();
tcp.RunCommand();

// Run the application indefinitely
// app.Run();