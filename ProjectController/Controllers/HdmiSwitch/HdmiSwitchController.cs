using System.IO.Ports;
using Microsoft.AspNetCore.SignalR;
using ProjectController.Communication.Serial;

namespace ProjectController.Controllers.HdmiSwitch;

public class HdmiSwitchController
{
    private readonly ILogger<HdmiSwitchController> logger;
    private readonly SerialCommunication serialCommunication;
    private readonly IHubContext<GUIHub> hub;
    private const string portName0 = "/dev/ttyUSB0"; // Replace with the correct COM port for your device
    private const string portName1 = "/dev/ttyUSB1"; // Replace with the correct COM port for your device
    private const BaudRate baudRate = BaudRate.Baud19200; // The desired baud rate (19200)
    private const DataBits dataBits = DataBits.Eight; // Number of data bits (8)
    private const Parity parity = Parity.None; // Parity setting (None)
    private const StopBits stopBits = StopBits.One; // Stop bits setting (1 stop bit)
    private const Handshake handshake = Handshake.None; // Flow control (None)
    private byte[] endOfLineBytes = { 0x0A, 0x0D }; // End of line bytes [0x0A, 0x0D];
    public HdmiSwitchController(ILogger<HdmiSwitchController> logger, SerialCommunication serialCommunication, IHubContext<GUIHub> hub)
    {
        this.logger = logger;
        this.serialCommunication = serialCommunication;
        this.hub = hub;
        serialCommunication.StartCommunication(new[] {portName0, portName1}, baudRate, dataBits, parity, stopBits, handshake, endOfLineBytes);
    }

    public async Task SetInputHdmi(Inputs input)
    {
        var command = $"sw i0{(int)input}";
        WriteCommand(command);
        var response = ReadResponseAsString();
        if (IsResponseValid(response))
        {
            await hub.Clients.All.SendAsync("ReceiveHdmiInputQueryResponse", GetInputFromInputCommand(response));
        }
    }

    public void ToggleDisplayOn(bool isOn)
    {
        var command = $"sw {(isOn ? "on" : "off")}";
        WriteCommand(command);
        var response = ReadResponseAsString();
    }

    public void SwitchToNextOrPreviousInput(bool isNext)
    {
        var command = $"sw {(isNext ? "+" : "-")}";
        WriteCommand(command);
        var response = ReadResponseAsString();
    }

    public void EnableHotPlugDetection(bool isEnabled)
    {
        var command = $"hpd {(isEnabled ? "on" : "off")}";
        WriteCommand(command);
        var response = ReadResponseAsString();
    }

    public async Task ReadCurrentConfiguration()
    {
        var command = "read";
        WriteCommand(command);
        var response = ReadResponseAsString(isReadCommand: true);
        if (IsResponseValid(response))
        {
            await hub.Clients.All.SendAsync("ReceiveHdmiInputQueryResponse", GetInputFromReadResponse(response));
        }
    }

    public void ResetToFactoryDefaults()
    {
        var command = $"reset";
        WriteCommand(command);
        var response = ReadResponseAsString();
    }

    public void SetSwitchMode(SwitchMode mode, int? input = null, bool? goToOn = null)
    {
        switch (mode)
        {
            case SwitchMode.Auto:
                WriteCommand($"swmode i0{input} auto");
                break;
            case SwitchMode.Next:
                WriteCommand("swmode next");
                break;
            case SwitchMode.Default:
                WriteCommand("swmode default");
                break;
            case SwitchMode.GoTo:
                if (!goToOn.HasValue)
                    throw new ArgumentNullException(nameof(goToOn), "GoTo mode requires a value for goToOn");
                WriteCommand($"swmode goto {(goToOn.Value ? "on" : "off")}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
        
        var response = ReadResponseAsString();
    }

    private void WriteCommand(string command)
    {
        serialCommunication.WriteCommand($"{command}\r");
    }

    private List<string> ReadResponseAsString(bool isReadCommand = false)
    {
        var response = new List<string>();

        if (!isReadCommand)
        {
            while (true)
            {
                try
                {
                    // Response e.g.: sw i01 Command OK\n\r
                    logger.LogTrace("Trying to read a single line from the serial port.");
                    response.Add(serialCommunication.ReadOneLineOfResponseAsString());
                }
                catch (TimeoutException)
                {
                    logger.LogInformation("Response to command timed out. Response: {response}", string.Join(" ", response));
                }
                return response;
            }
        }
        
        // Response e.g.: read Command OK\n\r Input:port 2\n\r Output:ON\n\r Mode:Next\n\r Goto:OFF\n\r F/W:V1.1.101\n\r
        for (var i = 1; i <= 6; i++)
        {
            logger.LogTrace("Trying to read line {i} from the serial port.", i);
            response.Add($"{serialCommunication.ReadOneLineOfResponseAsString()}\n");
        }
        logger.LogInformation("Response to read command: {response}", string.Join(" ", response));
        return response;
    }

    private bool IsResponseValid(List<string> response)
    {
        return response[0].Contains("Command OK");
    }

    private Inputs GetInputFromReadResponse(List<string> response)
    {
        if (response.Count < 6) 
            throw new InvalidOperationException($"Read response contained only {response.Count} line(s).");

        var inputLine = response[1].Split(':');
        if (inputLine.Length != 2)
            throw new InvalidOperationException($"Read response contained invalid input line: {response[1]}");
        
        var input = (Inputs)int.Parse(inputLine[1].Split(" ")[1].Trim());
        logger.LogInformation("Input from read response: {$input}", input);
        return input;
    }
    
    private Inputs GetInputFromInputCommand(List<string> response)
    {
        if (response.Count >= 2)
            throw new InvalidOperationException($"Input command response contained more than 1 line: {response.Count} lines");
        
        var input = (Inputs)int.Parse(response[0].Split(' ')[1][1..]);
        logger.LogInformation("Input from input command response: {$input}", input);
        return input;
    }
    
    public enum SwitchMode
    {
        Next, // Switch priority is placed on the next port that has a new source device connected to it. (default) 
        Default, // The switch behaves normally without automatic switching
        Auto, // Places priority on a selected port so that when a source isconnected to the said port, the VS481B automatically switches to it, and the port can not be changed until the source is unplugged
        // or auto switching mode is disabled with the default command. In addition, the Go To function enables the VS481B to switch to the next port with a powered on source device when the current input source device is powered off.
        GoTo
    }
}