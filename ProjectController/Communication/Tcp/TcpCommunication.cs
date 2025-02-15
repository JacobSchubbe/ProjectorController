using System.Net.Sockets;
using System.Text;
using Serilog;

namespace ProjectController.Communication.Tcp;

public sealed class TcpCommunication : IDisposable
{
    private readonly ILogger<TcpCommunication> logger;
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
    
    public TcpCommunication(ILogger<TcpCommunication> logger)
    {
        this.logger = logger;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Task.Run(async () => await DetectConnectionChange(CancellationToken.None));
    }
    
    private void Log(string message, LogLevel logLevel = LogLevel.Debug)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                logger.LogTrace(message);
                break;
            case LogLevel.Debug:
                logger.LogDebug(message);
                break;
            case LogLevel.Information:
                logger.LogInformation(message);
                break;
            case LogLevel.Warning:
                logger.LogWarning(message);
                break;
            case LogLevel.Error:
                logger.LogError(message);
                break;
            case LogLevel.Critical:
                logger.LogCritical(message);
                break;
            case LogLevel.None:
            default:
                break;
        }
        // logger.Log(logLevel, logLevel is LogLevel.Trace or LogLevel.Error or LogLevel.Critical ? message + $"Stack Trace: {Environment.StackTrace}" : message);
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
        var retryCount = 0;
        while (retryCount < 10)
        {
            try
            {
                await DisposeExistingSocket(); // Dispose of any existing socket
            
                // Create and configure a new socket
                Log($"Creating a new socket for {host}:{port}...", LogLevel.Information);
                host = newHost;
                port = newPort;
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Log("New socket successfully created.", LogLevel.Information);
                await StartSocket(cancellationToken);
                return;
            }
            catch (Exception ex)
            {
                retryCount++;
                Log($"Failed to create socket. Retrying in {retryCount * 1000}ms... Ex: {ex}");
                await Task.Delay(retryCount * 1000, cancellationToken); // Exponential backoff
            }
        }
    }

    private async Task DisposeExistingSocket()
    {
        try
        {
            if (socket?.Connected ?? false)
            {
                Log("Shutting down the existing socket...", LogLevel.Information);
                socket.Shutdown(SocketShutdown.Both); // Shutdown the socket gracefully
                socket.Close(); // Close the socket
            }

            Log("Disposing the old socket...", LogLevel.Information);
            socket?.Dispose(); // Dispose of the old socket
            lastConnectionStatus = false;
            await (disconnectEvent?.Invoke() ?? Task.CompletedTask);
        }
        catch (SocketException ex)
        {
            Log($"SocketException occurred while disposing of the old socket: {ex}", LogLevel.Error);
        }
        catch (Exception ex)
        {
            Log($"Unexpected error occurred while disposing of the existing socket: {ex}", LogLevel.Error);
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
                Log("Canceled connection change detection.");
                return;
            }
            catch (Exception ex)
            {
                Log($"Error in DetectConnectionChange: {ex.Message}");
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
        Log($"Connecting to {host}:{port}...", LogLevel.Information);
        await socket.ConnectAsync(host, port, cancellationToken);
        Log($"Connected to {host}:{port}.", LogLevel.Information); 
        await HandleConnectionChange(socket is { Connected: true }, cancellationToken);
    }

    private void ClearBuffer()
    {
        Log("Clearing buffer...");
        try
        {
            // Ensure the socket is available and connected
            if (!socket.Connected) 
                return;

            // Temporarily set the socket to non-blocking to avoid indefinite blocking
            socket.Blocking = false;

            // Loop to read all available data
            while (socket.Available > 0)
            {
                _ = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            }
            Log("Buffer cleared.");
        }
        catch (Exception ex)
        {
            Log($"Unexpected error during buffer clearing: {ex}", LogLevel.Error);
        }
        finally
        {
            // Restore the socket to blocking state
            socket.Blocking = true;
        }
    }

    public async Task<string> SendCommand(string commandStr, CancellationToken cancellationToken, bool waitForSemaphore = true)
    {
        while (true)
        {
            // Log("Accessing semaphore for send command.", LogLevel.Trace);
            if (waitForSemaphore)
            {
                // Log("Waiting for semaphore for send command.", LogLevel.Trace);
                await GetConnectionSemaphore(cancellationToken);
            }

            var socketSendingSemaphoreReleasedAlready = false;
            try
            {
                var commandBytes = Encoding.ASCII.GetBytes(commandStr);
                await socketSendingSemaphore.WaitAsync(cancellationToken);
                Log($"Sending command: {commandStr}", LogLevel.Information);
                ClearBuffer();
                socket.Send(commandBytes);
                Log($"Sent command: {commandStr}", LogLevel.Information);
                return GetResponse(cancellationToken);
            }
            catch (SocketException ex)
            {
                Log($"SocketException while sending command: {ex.Message}", LogLevel.Error);
                socketSendingSemaphoreReleasedAlready = true;
                socketSendingSemaphore.Release();
                await CreateNewSocket(host, port, cancellationToken);
            }
            finally
            {
                if (!socketSendingSemaphoreReleasedAlready)
                    socketSendingSemaphore.Release();
                if (waitForSemaphore)
                    ReleaseConnectionSemaphore();
            }
            
            await Task.Delay(250, cancellationToken);
        }
    }

    private string GetResponse(CancellationToken cancellationToken)
    {
        var bytesRead = socket.Receive(buffer);
        var rawResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        Log($"Received response: {rawResponse}", LogLevel.Information);
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
            Log($"Error while closing the socket: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            socket.Dispose();
        }
    }}