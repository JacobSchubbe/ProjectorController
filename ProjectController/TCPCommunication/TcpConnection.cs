using System.Net.Sockets;
using System.Text;
using static ProjectController.TCPCommunication.TCPConsts;

namespace ProjectController.TCPCommunication;

public sealed class TcpConnection : IDisposable
{
    private readonly ILogger<TcpConnection> logger;
    private readonly Socket socket;
    private readonly Queue<(ProjectorCommands command, Func<ProjectorCommands, string, Task> callback)> ProjectCommandQueue = new();
    private readonly SemaphoreSlim queueAccessSemaphore = new(1, 1);
    private readonly SemaphoreSlim connectionSemaphore = new(1, 1);
    private event Func<bool, Task>? disconnectEvent;
    private bool lastConnectionStatus;
    
    private readonly string host = "192.168.0.150";
    private readonly int port = 3629;
    private readonly byte[] buffer = new byte[1024];
    private readonly byte ETX = Encoding.ASCII.GetBytes(":")[0];
    
    public TcpConnection(ILogger<TcpConnection> logger)
    {
        this.logger = logger;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Task.Run(async () => await DetectConnectionChange(CancellationToken.None));

        _ = CheckConnection(CancellationToken.None);
        RunCommandQueue(CancellationToken.None);
    }
    
    public bool IsConnected => socket?.Connected ?? false;

    public void RegisterOnDisconnect(Func<bool, Task> callback)
    {
        disconnectEvent += callback;
    }
    
    public void UnregisterOnDisconnect(Func<bool, Task> callback)
    {
        disconnectEvent -= callback;
    }
    
    private async Task DetectConnectionChange(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var isConnected = socket is { Connected: true };
                if (isConnected != lastConnectionStatus)
                {
                    lastConnectionStatus = isConnected;
                    if (disconnectEvent != null)
                    {
                        await disconnectEvent.Invoke(isConnected);
                    }
                }
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
    
    private async Task CheckConnection(CancellationToken cancellationToken)
    {
        await connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (socket.Connected)
                return;
            
            logger.LogInformation($"Connecting to {host}:{port}...");
            await socket.ConnectAsync(host, port, cancellationToken);
            SendCommand(ProjectorCommands.SystemControlStartCommunication);
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

    private async void RunCommandQueue(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    logger.LogDebug("Try to access queue...");
                    await queueAccessSemaphore.WaitAsync(token);
                    logger.LogDebug("Accessed queue...");
                    (ProjectorCommands command, Func<ProjectorCommands, string, Task> callback) commandKvp;
                    var dequeued = false;
                    
                    try
                    {
                        logger.LogDebug("Checking for command to dequeue...");
                        dequeued = ProjectCommandQueue.TryDequeue(out commandKvp);
                    }
                    finally
                    {
                        logger.LogDebug("Finished with queue...");
                        queueAccessSemaphore.Release();
                    }

                    if (dequeued)
                    {
                        await CheckConnection(token);
                        var response = SendCommand(commandKvp.command);
                        await commandKvp.callback(commandKvp.command, response);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"Exception while trying to send a command: {e.Message}");
                }
            
                await Task.Delay(TimeSpan.FromMilliseconds(100), token);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Canceled all commands.");
        }
        catch (Exception e)
        {
            logger.LogError($"Generic Exception: {e.Message}");
        }
    }
    
    public async Task QueueCommand(ProjectorCommands[] commands, Func<ProjectorCommands, string, Task> callback)
    {
        foreach (var command in commands)
        {
            await queueAccessSemaphore.WaitAsync();
            try
            {
                ProjectCommandQueue.Enqueue((command, callback));
                logger.LogInformation($"Command enqueued: {command}");
            }
            finally
            {
                queueAccessSemaphore.Release();
            }
        }
    }
    
    string SendCommand(ProjectorCommands command)
    {
        var commandStr = command != ProjectorCommands.SystemControlStartCommunication ? 
            $"{ProjectorCommandsDictionary[command]}\r" : $"{ProjectorCommandsDictionary[command]}";

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