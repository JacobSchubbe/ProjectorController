using System.Net.Sockets;
using System.Text;
using static ProjectController.TCPCommunication.TCPConsts;

namespace ProjectController.TCPCommunication;

public class TcpConnection
{
    private readonly Socket? socket;
    private readonly Queue<(SystemControl command, Func<SystemControl, string, Task> callback)> commandQueue = new();
    private readonly SemaphoreSlim queueAccessSemaphore = new(1, 1);
    
    private readonly string host = "192.168.0.150";
    private readonly int port = 3629;
    private readonly byte[] buffer = new byte[1024];

    public TcpConnection()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        if (!socket.Connected)
        {
            Console.WriteLine($"Connecting to {host}:{port}...");
            socket.Connect(host, port);
            // ClearBuffer();
            SendCommand(SystemControl.StartCommunication);
            Console.WriteLine($"Connected to {host}:{port}.");
        }
        RunCommandQueue(CancellationToken.None);
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
                        if (commandQueue.TryDequeue(out var commandKvp))
                        {
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
    
    public async Task QueueCommand(SystemControl[] commands, Func<SystemControl, string, Task> callback)
    {
        foreach (var command in commands)
        {
            await queueAccessSemaphore.WaitAsync();
            try
            {
                commandQueue.Enqueue((command, callback));
                Console.WriteLine($"Command enqueued: {command}");
            }
            finally
            {
                queueAccessSemaphore.Release();
            }
        }
    }
    
    string SendCommand(SystemControl command)
    {
        string commandStr;
        if (command != SystemControl.StartCommunication)
        {
            commandStr = $"{SystemControlDictionary[command]}\r";
        }
        else
        {
            commandStr = $"{SystemControlDictionary[command]}";
        }

        byte[] commandBytes = Encoding.ASCII.GetBytes(commandStr);
        socket.Send(commandBytes);

        Console.WriteLine($"Sent command: {commandStr}");

        // Receive the response
        int bytesRead = socket.Receive(buffer);
        string rawResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"Received response: {rawResponse}");
        var status = StringToPowerStatus(rawResponse);
        if (status == null) 
            return rawResponse;
        Console.WriteLine($"Received response: {status.ToString()}");
        return status.ToString();
    }
}