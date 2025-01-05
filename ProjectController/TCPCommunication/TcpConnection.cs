using System.Net.Sockets;
using System.Text;
using static ProjectController.TCPCommunication.TCPConsts;

namespace ProjectController.TCPCommunication;

public class TcpConnection
{
    private readonly Socket? socket;
    private readonly Queue<string> commandQueue = new();
    private readonly SemaphoreSlim queueAccessSemaphore = new(1, 1);
    
    private readonly string host = "192.168.0.150";
    private readonly int port = 3629;
    
    public TcpConnection()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        if (!socket.Connected)
        {
            Console.WriteLine($"Connecting to {host}:{port}...");
            socket.Connect(host, port);
            SendCommand(SystemControlDictionary[SystemControl.StartCommunication]);
            Console.WriteLine($"Connected to {host}:{port}.");
        }
        RunCommandQueue(CancellationToken.None);
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
                        if (commandQueue.TryDequeue(out var command))
                        {
                            SendCommand(command);
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
    
    public async Task QueueCommand(string[] commands)
    {
        foreach (var command in commands)
        {
            await queueAccessSemaphore.WaitAsync();
            try
            {
                commandQueue.Enqueue(command);
                Console.WriteLine($"Command enqueued: {command}");
            }
            finally
            {
                queueAccessSemaphore.Release();
            }
        }
    }
    
    void SendCommand(string command)
    {
        if (command != SystemControlDictionary[SystemControl.StartCommunication])
        {
            command = $"{command}\r";
        }

        byte[] commandBytes = Encoding.ASCII.GetBytes(command);
        socket.Send(commandBytes);

        Console.WriteLine($"Sent command: {command}");

        // Receive the response
        byte[] buffer = new byte[1024];
        int bytesRead = socket.Receive(buffer);
        string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

        Console.WriteLine($"Received response: {response}");

        // Uncomment to handle error responses
        /*
        if (response == "Err\r:")
        {
            byte[] errorQueryBytes = Encoding.ASCII.GetBytes(ErrorQuery + "\r");
            socket.Send(errorQueryBytes);
            Console.WriteLine($"Sent command: {ErrorQuery}");
            bytesRead = socket.Receive(buffer);
            response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received error response: {response}");
        }
        */
    }
}