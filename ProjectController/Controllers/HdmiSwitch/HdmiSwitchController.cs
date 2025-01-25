using System.IO.Ports;
using ProjectController.Communication.Serial;

namespace ProjectController.Controllers.HdmiSwitch;

public class HdmiSwitchController
{
    private string portName = "COM3"; // Replace with the correct COM port for your device
    private int baudRate = 19200;     // The desired baud rate (19200)
    private int dataBits = 8;         // Number of data bits (8)
    private Parity parity = Parity.None;      // Parity setting (None)
    private StopBits stopBits = StopBits.One; // Stop bits setting (1 stop bit)
    private Handshake handshake = Handshake.None; // Flow control (None)
    
    public HdmiSwitchController(SerialCommunication serialCommunication)
    {
        serialCommunication.Connect(portName, baudRate, dataBits, parity, stopBits, handshake, 0x04).Wait();
    }
}