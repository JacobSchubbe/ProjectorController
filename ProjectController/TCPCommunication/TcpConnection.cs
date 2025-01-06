using System.Net.Sockets;
using System.Text;
using static ProjectController.TCPCommunication.TCPConsts;

namespace ProjectController.TCPCommunication;

public class TcpConnection
{
    private readonly Socket? socket;
    private readonly Queue<(SystemControl command, Func<SystemControl, string, Task> callback)> commandQueue = new();
    private readonly SemaphoreSlim queueAccessSemaphore = new(1, 1);
    private readonly SemaphoreSlim connectionSemaphore = new(1, 1);
    
    private readonly string host = "192.168.0.150";
    private readonly int port = 3629;
    private readonly byte[] buffer = new byte[1024];
    private readonly byte ETX = ":"u8.ToArray()[0];
    
    public TcpConnection()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        if (!socket.Connected)
        {
            _ = ConnectToServer(CancellationToken.None);
        }
        RunCommandQueue(CancellationToken.None);
    }

    private async Task ConnectToServer(CancellationToken cancellationToken)
    {
        await connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            Console.WriteLine($"Connecting to {host}:{port}...");
            await socket.ConnectAsync(host, port, cancellationToken);
            // ClearBuffer();
            SendCommand(SystemControl.StartCommunication);
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
                        if (commandQueue.TryDequeue(out var commandKvp))
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

    string ReceiveData()
    {
        StringBuilder receivedData = new StringBuilder();
        int bytesRead;
        bool keepReceiving = true;

        while (keepReceiving)
        {
            // Receive data from the socket
            bytesRead = socket.Receive(buffer);

            // If bytes are received
            if (bytesRead <= 0) continue;
            
            for (var i = 0; i < bytesRead; i++)
            {
                Console.WriteLine($"Byte: {buffer[i]}");
                if (buffer[i] == ETX) // 0x0A is the byte for newline '\n'
                {
                    keepReceiving = false;  // Stop receiving once the byte is encountered
                    break;
                }

                // Append received byte to the received data
                receivedData.Append((char)buffer[i]);
            }
        }

        return receivedData.ToString();
    }
}