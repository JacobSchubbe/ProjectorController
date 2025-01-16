using System.Net.Sockets;
using System.Text;

namespace ProjectController.TCPCommunication;

public sealed class TcpConnection : IDisposable
{
    private readonly ILogger<TcpConnection> logger;
    private readonly Socket socket;
    private readonly SemaphoreSlim connectionSemaphore = new(1, 1);
    
    private event Func<bool, Task>? disconnectEvent;
    private event Func<Task>? connectEvent;
    private bool lastConnectionStatus;
    private readonly SemaphoreSlim connectionChangeCheckSemaphore = new(1, 1);
    
    private string host = string.Empty;
    private int port;
    private readonly byte[] buffer = new byte[1024];
    private readonly byte ETX = Encoding.ASCII.GetBytes(":")[0];
    
    public TcpConnection(ILogger<TcpConnection> logger)
    {
        this.logger = logger;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Task.Run(async () => await DetectConnectionChange(CancellationToken.None));
    }

    public Task Start(string host, int port)
    {
        this.host = host;
        this.port = port;
        return Task.CompletedTask;
    }

    public async Task Disconnect()
    {
        await socket.DisconnectAsync(true);
    }
    
    public bool IsConnected => socket?.Connected ?? false;

    public void RegisterOnDisconnect(Func<bool, Task> callback)
    {
        disconnectEvent += callback;
    }
    
    public void RegisterOnConnect(Func<Task> callback)
    {
        connectEvent += callback;
    }
    
    private async Task DetectConnectionChange(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await CheckConnectionChange();
            }
            catch (OperationCanceledException)
            {
                logger.LogDebug("Canceled connection change detection.");
                return;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in DetectConnectionChange: {ex.Message}");
            }
            await Task.Delay(100, cancellationToken);
        }
    }

    private async Task CheckConnectionChange()
    {
        await connectionChangeCheckSemaphore.WaitAsync();
        try
        {
            var isConnected = socket is { Connected: true };
            if (isConnected != lastConnectionStatus)
            {
                if (isConnected)
                {
                    await (connectEvent?.Invoke() ?? Task.CompletedTask);
                }
                else
                {
                    await (disconnectEvent?.Invoke(isConnected) ?? Task.CompletedTask);
                }
                lastConnectionStatus = isConnected;
            }
        }
        finally
        {
            connectionChangeCheckSemaphore.Release();
        }
    }
    
    public async Task CheckConnection(CancellationToken cancellationToken)
    {
        await connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (socket.Connected)
                return;
            
            logger.LogInformation($"Connecting to {host}:{port}...");
            await socket.ConnectAsync(host, port, cancellationToken);
            await CheckConnectionChange();
            logger.LogInformation($"Connected to {host}:{port}."); 
        }
        finally
        {
            connectionSemaphore.Release();
        }
    }

    private void ClearBuffer()
    {
        socket?.Receive(buffer);
    }
    
    public string SendCommand(string commandStr)
    {
        var commandBytes = Encoding.ASCII.GetBytes(commandStr);
        socket.Send(commandBytes);

        logger.LogInformation($"Sent command: {commandStr}");

        var bytesRead = socket.Receive(buffer);
        var rawResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        logger.LogInformation($"Received response: {rawResponse}");
        return rawResponse;
    }
    
    public void Dispose()
    {
        socket?.Dispose();
    }
}