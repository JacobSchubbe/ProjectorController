using System.Net.Sockets;
using System.Text;
using static ProjectController.TCPCommunication.TCPConsts;

namespace ProjectController.TCPCommunication;

public sealed class TcpConnection : IDisposable
{
    private readonly Socket? socket;
    private readonly Queue<(ProjectorCommands command, Func<ProjectorCommands, string, Task> callback)> ProjectCommandQueue = new();
    private readonly SemaphoreSlim queueAccessSemaphore = new(1, 1);
    private readonly SemaphoreSlim connectionSemaphore = new(1, 1);
    private event Func<bool?, Task>? disconnectEvent;
    private bool lastConnectionStatus;
    
    private readonly string host = "192.168.0.150";
    private readonly int port = 3629;
    private readonly byte[] buffer = new byte[1024];
    private readonly byte ETX = Encoding.ASCII.GetBytes(":")[0];
    
    public TcpConnection()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Task.Run(async () => await DetectConnectionChange(CancellationToken.None));
        
        if (!socket.Connected)
        {
            _ = ConnectToServer(CancellationToken.None);
        }
        RunCommandQueue(CancellationToken.None);
    }
    
    public bool IsConnected => socket?.Connected ?? false;

    public void RegisterOnDisconnect(Func<bool?, Task> callback)
    {
        disconnectEvent += callback;
    }
    
    public void UnregisterOnDisconnect(Func<bool?, Task> callback)
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
                Console.WriteLine("Canceled connection change detection.");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DetectConnectionChange: {ex.Message}");
            }
            await Task.Delay(100, cancellationToken);
        }
    }
    private async Task ConnectToServer(CancellationToken cancellationToken)
    {
        await connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            Console.WriteLine($"Connecting to {host}:{port}...");
            await socket.ConnectAsync(host, port, cancellationToken);
            // ClearBuffer();
            SendCommand(ProjectorCommands.SystemControlStartCommunication);
            Console.WriteLine($"Connected to {host}:{port}.");
        }
        finally
        {
            connectionSemaphore.Release();
        }
    }

    private async Task CheckConnection(CancellationToken cancellationToken)
    {
        await connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (socket.Connected)
                return;
            await ConnectToServer(cancellationToken);
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
                    await queueAccessSemaphore.WaitAsync(token);
                    try
                    {
                        if (ProjectCommandQueue.TryDequeue(out var commandKvp))
                        {
                            await CheckConnection(token);
                            var response = SendCommand(commandKvp.command);
                            await commandKvp.callback(commandKvp.command, response);
                        }
                    }
                    finally
                    {
                        queueAccessSemaphore.Release();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception while trying to send a command: {e.Message}");
                }
            
                await Task.Delay(TimeSpan.FromMilliseconds(25), token);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Canceled all commands.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"EXCEPTION: {e.Message}");
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
                Console.WriteLine($"Command enqueued: {command}");
            }
            finally
            {
                queueAccessSemaphore.Release();
            }
        }
    }
    
    string SendCommand(ProjectorCommands command)
    {
        string commandStr;
        if (command != ProjectorCommands.SystemControlStartCommunication)
        {
            commandStr = $"{ProjectorCommandsDictionary[command]}\r";
        }
        else
        {
            commandStr = $"{ProjectorCommandsDictionary[command]}";
        }

        byte[] commandBytes = Encoding.ASCII.GetBytes(commandStr);
        socket.Send(commandBytes);

        Console.WriteLine($"Sent command: {commandStr}");

        int bytesRead = socket.Receive(buffer);
        string rawResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        // string rawResponse = ReceiveData();
        
        Console.WriteLine($"Received response: {rawResponse}");
        var status = StringToPowerStatus(rawResponse);
        if (status == null) 
            return rawResponse;
        Console.WriteLine($"Received response: {status.ToString()}");
        return status.ToString();
    }
    
    public void Dispose()
    {
        socket?.Dispose();
    }
}