using System;
using System.Net.Sockets;
using System.Text;

namespace ProjectController.TCPCommunication;

public class TcpConnection
{
    private Socket? socket;
    
// =========== SYSTEM CONTROL ========================
    private const string startCommunication = "ESC/VP.net\x10\x03\x00\x00\x00\x00";

    private const string powerQuery = "PWR?";
    private const string powerOff = "PWR OFF";
    private const string powerOn = "PWR ON";

    private const string volumeMuteOn = "MUTE ON"; // only with non-ARC sound
    private const string volumeMuteOff = "MUTE OFF"; // only with non-ARC sound
    private const string volumeMuteQuery = "MUTE?";
    private const string volumeUp = "VOL INC"; // only with non-ARC sound
    private const string volumeDown = "VOL DEC"; // only with non-ARC sound
    private const string volumeQuery = "VOL?";

    private const string sourceHDMI1 = "SOURCE 30";
    private const string sourceHDMI2 = "SOURCE A0";
    private const string sourceHDMI3 = "SOURCE C0";
    private const string sourceHDMILan = "SOURCE 53";
    private const string sourceListQuery = "SOURCELIST?";

// ============ SYSTEM INFORMATION ===================
    private const string projectorNameQuery = "NWPNAME?";
    private const string serialNumberQuery = "SNO?";
    private const string errorQuery = "ERR?";
    private const string lampHoursQuery = "LAMP?";
    private const string operationalTimeQuery = "ONTIME?";
    private const string signalStatusQuery = "SIGNAL?"; // 00: No signal, 01: Signal detected (2D), 02: Signal detected (3D), FF: Unsupported signal

// ================ IMAGE ===================
    private const string naturalColorMode = "CMODE 07";

    private const string imageReverseHorizontalOn = "HREVERSE ON";
    private const string imageReverseHorizontalOff = "HREVERSE OFF";
    private const string imageReverseHorizontalQuery = "HREVERSE?";
    private const string imageReverseVerticalOn = "VREVERSE ON";
    private const string imageReverseVerticalOff = "VREVERSE OFF";
    private const string imageReverseVerticalQuery = "VREVERSE?";


// same for CONTRAST, DENSITY, TINT
    private const string brightnessUp = "BRIGHT INC";
    private const string brightnessDown = "BRIGHT DEC";
    private const string brightnessQuery = "BRIGHT?";
    
    private const string statusLEDIlluminationOn = "ILLUM 01";
    private const string statusLEDIlluminationOff = "ILLUM 00";
    private const string statusLEDIlluminationQuery = "ILLUM?";


// ================== KEYS ===================

    private const string keyPower = "KEY 01";
    private const string keyMenu = "KEY 03";
    private const string keyUp = "KEY 35";
    private const string keyDown = "KEY 36";
    private const string keyLeft = "KEY 37";
    private const string keyRight = "KEY 38";
    private const string keyEnter = "KEY 16";
    private const string keyHome = "KEY 04";

    private const string keyVolumeUp = "KEY 56";
    private const string keyVolumeDown = "KEY 57";
    private const string keyAVMuteBlank = "KEY 3E";
    private const string keyKeysTone = "KEY C8";
    private const string keyHDMILink = "KEY 8E";

    private const string keyPlay = "KEY D1";
    private const string keyStop = "KEY D2";
    private const string keyPause = "KEY D3";
    private const string keyRewind = "KEY D4";
    private const string keyFastFoward = "KEY D5";
    private const string keyBackward = "KEY D6";
    private const string keyForward = "KEY D7";
    private const string keyMute = "KEY D8";
    private const string keyLinkMenu = "KEY D9";

    private enum KeyCommands : byte
    {
        Power = 0x01
    }

    private enum PowerStatus
    {
        StandbyNetworkOff = 0,
        LampOn = 1,
        Warmup = 2,
        CoolDown = 3,
        StandbyNetworkOn = 4,
        AbnormalityStandby = 5,
    }
    private static byte[] PowerStatusToBytes(PowerStatus status) => Encoding.ASCII.GetBytes($"PWR=0{(int)status}\r:");
    private static readonly byte[] ErrorResponse = Encoding.ASCII.GetBytes("Err\r:");

    static void SendCommand(Socket socket, string command)
    {
        if (command != startCommunication)
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

            // Create a TCP socket
            socket ??= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            // Connect to the server
            socket.Connect(host, port);
            Console.WriteLine($"Connected to {host}:{port}.");

            // Send commands
            SendCommand(socket, startCommunication);
            // SendCommand(socket, powerOn);
            SendCommand(socket, powerQuery);
            
            socket.Disconnect(true);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Error with the TCP connection: {e.Message}");
        }
    }
}