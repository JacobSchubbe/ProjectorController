using System.IO.Ports;
using Microsoft.AspNetCore.SignalR;
using ProjectController.Communication.Serial;

namespace ProjectController.Controllers.HdmiSwitch;

public class HdmiSwitchController
{
    private readonly SerialCommunication serialCommunication;
    private readonly IHubContext<GUIHub> hub;
    private string portName = "/dev/ttyUSB0"; // Replace with the correct COM port for your device
    private int baudRate = 19200;     // The desired baud rate (19200)
    private int dataBits = 8;         // Number of data bits (8)
    private Parity parity = Parity.None;      // Parity setting (None)
    private StopBits stopBits = StopBits.One; // Stop bits setting (1 stop bit)
    private Handshake handshake = Handshake.None; // Flow control (None)
    
    public HdmiSwitchController(SerialCommunication serialCommunication, IHubContext<GUIHub> hub)
    {
        this.serialCommunication = serialCommunication;
        this.hub = hub;
        serialCommunication.Connect(portName, baudRate, dataBits, parity, stopBits, handshake, 0x04).Wait();
    }

    public string SetInputHdmi(int input)
    {
        if (input is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(input), "Input must be between 1 and 4");

        var command = $"sw i 0{input}"; // it says [Enter] so maybe a \r?
        WriteCommand(command);
        return ReadResponseAsString();
    }

    public string ToggleDisplayOn(bool isOn)
    {
        var command = $"sw {(isOn ? "on" : "off")}";
        WriteCommand(command);
        return ReadResponseAsString();
    }

    public string SwitchToNextOrPreviousInput(bool isNext)
    {
        var command = $"sw {(isNext ? "+" : "-")}";
        WriteCommand(command);
        return ReadResponseAsString();
    }

    public string EnableHotPlugDetection(bool isEnabled)
    {
        var command = $"hpd {(isEnabled ? "on" : "off")}";
        WriteCommand(command);
        return ReadResponseAsString();
    }

    public async Task<string> ReadCurrentConfiguration()
    {
        var command = "read";
        WriteCommand(command);
        await hub.Clients.All.SendAsync("handleHdmiInputQuery", 3);
        return ReadResponseAsString();
    }

    public string ResetToFactoryDefaults()
    {
        var command = $"reset";
        WriteCommand(command);
        return ReadResponseAsString();
    }

    public string SetSwitchMode(SwitchMode mode, int? input = null, bool? goToOn = null)
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
        
        return ReadResponseAsString();
    }

    private void WriteCommand(string command)
    {
        serialCommunication.WriteCommand($"{command}\r");
    }

    private string ReadResponseAsString()
    {
        return serialCommunication.ReadResponseAsString();
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