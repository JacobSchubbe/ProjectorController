using System.Net.Sockets;
using System.Text;
using static ProjectController.TCPCommunication.TCPConsts;

namespace ProjectController.TCPCommunication;

public class TcpConnection
{
    private Socket? socket;

    public TcpConnection()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }
    
    static void SendCommand(Socket socket, string command)
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

    public void RunCommand()
    {
        string host = "192.168.0.150";
        int port = 3629;

        try
        {
            Console.WriteLine($"Connecting to {host}:{port}...");
            
            // Connect to the server
            socket.Connect(host, port);
            Console.WriteLine($"Connected to {host}:{port}.");

            // Send commands
            SendCommand(socket, SystemControlDictionary[SystemControl.StartCommunication]);
            // SendCommand(socket, powerOn);
            SendCommand(socket, SystemControlDictionary[SystemControl.PowerQuery]);
            
            socket.Disconnect(true);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Error with the TCP connection: {e.Message}");
        }
    }
}