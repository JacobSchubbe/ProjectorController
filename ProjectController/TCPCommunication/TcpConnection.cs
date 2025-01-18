using System.Net.Sockets;
using System.Text;

namespace ProjectController.TCPCommunication;

public sealed class TcpConnection : IDisposable
{
    private readonly ILogger<TcpConnection> logger;
    private readonly SemaphoreSlim connectionSemaphore = new(1, 1);
    private Socket socket;
    
    private event Func<Task>? disconnectEvent;
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
        Task.Run(StartHeartbeatSender);
    }

    public Task Start(string host, int port)
    {
        // await CreateNewSocket(host, port);
        this.host = host;
        this.port = port;

        return Task.CompletedTask;
    }

    private async Task CreateNewSocket(string host, int port)
    {
        await connectionChangeCheckSemaphore.WaitAsync();
        try
        {
            this.host = host;
            this.port = port;

            if (socket?.Connected ?? false)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            socket?.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogError($"Error disposing old socket: {ex.Message}");
        }

        try
        {
            logger.LogInformation("Creating a new socket...");
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error creating new socket: {ex.Message}");
        }
        finally
        {
            connectionChangeCheckSemaphore.Release();
        }
    }

    public async Task Disconnect()
    {
        await socket.DisconnectAsync(true);
    }
    
    public bool IsConnected => socket?.Connected ?? false;

    public void RegisterOnDisconnect(Func<Task> callback)
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
                await CheckConnectionChange(cancellationToken);
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

    private async Task CheckConnectionChange(CancellationToken cancellationToken)
    {
        await connectionChangeCheckSemaphore.WaitAsync(cancellationToken);
        try
        {
            var isConnected = socket is { Connected: true };
            if (isConnected != lastConnectionStatus)
            {
                if (isConnected)
                {
                    ClearBuffer();
                    await (connectEvent?.Invoke() ?? Task.CompletedTask);
                }
                else
                {
                    await (disconnectEvent?.Invoke() ?? Task.CompletedTask);
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
            await CheckConnectionChange(cancellationToken);
            logger.LogInformation($"Connected to {host}:{port}."); 
        }
        finally
        {
            connectionSemaphore.Release();
        }
    }

    public void ClearBuffer()
    {
        try
        {
            // Ensure the socket is available and connected
            if (!socket.Connected) 
                return;

            // Temporarily set the socket to non-blocking to avoid indefinite blocking
            socket.Blocking = false;

            // Loop to read all available data
            while (true)
            {
                int bytesRead = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

                // If no data is read, we assume the buffer is cleared
                if (bytesRead == 0)
                    break;
            }
        }
        catch (SocketException ex)
        {
            // Error code 10035 (WSAEWOULDBLOCK) means there is no data available.
            if (ex.SocketErrorCode != SocketError.WouldBlock)
            {
                throw; // Re-throw other exceptions
            }
        }
        finally
        {
            // Restore the socket to blocking state
            socket.Blocking = true;
        }
    }
    
    public async Task<string> SendCommand(string commandStr)
    {
        while (true)
        {
            try
            {
                var commandBytes = Encoding.ASCII.GetBytes(commandStr);
                socket.Send(commandBytes);
                logger.LogInformation($"Sent command: {commandStr.Replace("\r", "\\r")}");
                var bytesRead = socket.Receive(buffer);
                var rawResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                logger.LogInformation($"Received response: {rawResponse.Replace("\r", "\\r")}");
                return rawResponse;
            }
            catch (SocketException)
            {
                await CreateNewSocket(host, port);
            }

            await Task.Delay(100);
        }
    }
    
    private async void StartHeartbeatSender()
    {
        try
        {
            while (!socket.Connected)
            {
                var heartbeatMessage = Encoding.ASCII.GetBytes("PING\r");
                socket.Send(heartbeatMessage);
                logger.LogDebug("Sent heartbeat to keep connection alive.");
                await Task.Delay(TimeSpan.FromSeconds(10), CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error while sending heartbeat: {ex.Message}");
        }
    }
    
    public void Dispose()
    {
        try
        {
            if (!socket.Connected) return;
            
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
        catch (SocketException ex)
        {
            logger.LogError($"Error while closing the socket: {ex.Message}");
        }
        finally
        {
            socket.Dispose();
        }
    }}