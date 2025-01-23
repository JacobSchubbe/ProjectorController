using System.Net.Sockets;
using System.Text;

namespace ProjectController.TCPCommunication;

public sealed class TcpConnection : IDisposable
{
    private readonly ILogger<TcpConnection> logger;
    private Socket socket;
    
    private event Func<Task>? disconnectEvent;
    private event Func<Task>? connectEvent;
    private bool lastConnectionStatus;
    private readonly SemaphoreSlim connectionChangeCheckSemaphore = new(1, 1);
    private readonly SemaphoreSlim socketSendingSemaphore = new(1, 1);
    
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

    private async Task GetConnectionSemaphore(CancellationToken cancellationToken)
    {
        await connectionChangeCheckSemaphore.WaitAsync(cancellationToken);
    }

    private void ReleaseConnectionSemaphore()
    {
        connectionChangeCheckSemaphore.Release();
    }
    
    public Task Initialize(string newHost, int newPort)
    {
        host = newHost;
        port = newPort;

        return Task.CompletedTask;
    }
    
    private async Task CreateNewSocket(string newHost, int newPort, CancellationToken cancellationToken)
    {
        try
        {
            await DisposeExistingSocket(); // Dispose of any existing socket
        
            // Create and configure a new socket
            logger.LogInformation($"Creating a new socket for {host}:{port}...");
            host = newHost;
            port = newPort;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            logger.LogInformation("New socket successfully created.");
            await StartSocket(cancellationToken);
        }
        catch (Exception ex)
        {
            // Log full exception with stack trace for better diagnostics
            logger.LogError(ex, $"Failed to create a new socket for {host}:{port}.");
        }
    }

    private Task DisposeExistingSocket()
    {
        try
        {
            if (socket?.Connected ?? false)
            {
                logger.LogInformation("Shutting down the existing socket...");
                socket.Shutdown(SocketShutdown.Both); // Shutdown the socket gracefully
                socket.Close(); // Close the socket
            }

            logger.LogInformation("Disposing the old socket...");
            socket?.Dispose(); // Dispose of the old socket
            lastConnectionStatus = false;
        }
        catch (SocketException ex)
        {
            logger.LogError(ex, "SocketException occurred while disposing of the old socket.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while disposing of the existing socket.");
        }
        return Task.CompletedTask;
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
        await GetConnectionSemaphore(cancellationToken);
        try
        {
            var isConnected = socket is { Connected: true };
            await HandleConnectionChange(isConnected, cancellationToken);
        }
        finally
        {
            ReleaseConnectionSemaphore();
        }
    }

    private async Task HandleConnectionChange(bool isConnected, CancellationToken cancellationToken)
    {
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

    public async Task CheckConnection(CancellationToken cancellationToken)
    {
        await GetConnectionSemaphore(cancellationToken);
        try
        {
            if (socket.Connected)
                return;
            
            await StartSocket(cancellationToken);
        }
        finally
        {
            ReleaseConnectionSemaphore();
        }            
    }

    private async Task StartSocket(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Connecting to {host}:{port}...");
        await socket.ConnectAsync(host, port, cancellationToken);
        logger.LogInformation($"Connected to {host}:{port}."); 
        await HandleConnectionChange(socket is { Connected: true }, cancellationToken);
    }

    private void ClearBuffer()
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
    
    public async Task<string> SendCommand(string commandStr, CancellationToken cancellationToken)
    {
        while (true)
        {
            await GetConnectionSemaphore(cancellationToken);
            try
            {
                var commandBytes = Encoding.ASCII.GetBytes(commandStr);
                await socketSendingSemaphore.WaitAsync(cancellationToken);
                logger.LogInformation($"Sending command: {commandStr.Replace("\r", "\\r")}");
                ClearBuffer();
                socket.Send(commandBytes);
                logger.LogInformation($"Sent command: {commandStr.Replace("\r", "\\r")}");
                return GetResponse(cancellationToken);
            }
            catch (SocketException ex)
            {
                logger.LogError($"SocketException while sending command: {ex.Message}");
                await CreateNewSocket(host, port, cancellationToken);
            }
            finally
            {
                socketSendingSemaphore.Release();
                ReleaseConnectionSemaphore();
            }
            
            await Task.Delay(250, cancellationToken);
        }
    }

    private string GetResponse(CancellationToken cancellationToken)
    {
        var bytesRead = socket.Receive(buffer);
        var rawResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        logger.LogInformation($"Received response: {rawResponse.Replace("\r", "\\r")}");
        return rawResponse;
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