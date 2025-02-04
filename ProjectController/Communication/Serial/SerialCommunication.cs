using System.IO.Ports;
using System.Text;

namespace ProjectController.Communication.Serial;

public class SerialCommunication
{
    private ILogger<SerialCommunication> logger;
    private SerialPort? serialPort;
    private byte endOfLine;
    
    public SerialCommunication(ILogger<SerialCommunication> logger)
    {
        this.logger = logger;
    }
    
    public Task Connect(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits, Handshake handshake, byte endOfLine)
    {
        try
        {
            serialPort ??= new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            serialPort.Handshake = handshake; // Flow control
            serialPort.ReadTimeout = 4000;    // Timeout in milliseconds
            serialPort.WriteTimeout = 1000;
            this.endOfLine = endOfLine;
            logger.LogTrace($"Connecting to port {portName}, baudRate: {baudRate}, dataBits: {dataBits}, parity: {parity}, stopBits: {stopBits}, handshake: {handshake}, endOfLine: {endOfLine}");
            serialPort.Open();
            logger.LogTrace($"Opened serial port {portName} at {baudRate} baud, {dataBits} data bits, parity: {parity}, stop bits: {stopBits}, flow control: {handshake}");
        }
        catch (UnauthorizedAccessException e)
        {
            logger.LogError($"Access to the port is denied: {e.Message}");
        }
        catch (IOException e)
        {
            logger.LogError($"I/O error: {e.Message}");
        }
        catch (InvalidOperationException e)
        {
            logger.LogError($"Invalid operation: {e.Message}");
        }
        catch (TimeoutException)
        {
            logger.LogError("The operation timed out.");
        }
        catch (Exception e)
        {
            logger.LogError($"Error while open serial port: {e.Message}");
        }
        
        return Task.CompletedTask;
    }

    public void WriteCommand(string command)
    {
        WriteCommand(Encoding.ASCII.GetBytes(command));
    }

    // private async Task CheckConnection()
    // {
    //     while (serialPort is { IsOpen: false })
    //     {
    //         await Connect();
    //     }
    // }
    
    public void WriteCommand(byte[] command)
    {
        if (serialPort is not { IsOpen: true })
        {
            logger.LogError($"Serial port is not open. Unable to send command {command}.");
            return;
        }
        
        serialPort.Write(command, 0, command.Length); // Send the raw bytes
        logger.LogTrace($"Sent command: {command}");
    }

    public string ReadResponseAsString()
    {
        var response = ReadResponseAsBytes();
        logger.LogTrace($"Response: {response}");
        return response == null ? string.Empty : Encoding.ASCII.GetString(response);
    }
    
    public byte[]? ReadResponseAsBytes()
    {
        if (serialPort is not { IsOpen: true })
        {
            logger.LogError($"Serial port is not open. Unable to read response.");
            return null;
        }

        var response = new List<byte>();

        while (!response.Contains(endOfLine))
        {
            try
            {
                var readByte = serialPort.ReadByte(); // Read a single byte
                response.Add((byte)readByte);

                if (readByte == endOfLine)
                {
                    break; // End of response
                }
            }
            catch (TimeoutException)
            {
                logger.LogError("Response timed out.");
                break;
            }
            catch (Exception e)
            {
                logger.LogError($"Error while reading response: {e.Message}");
                break;
            }
        }
        logger.LogTrace($"Received response (in bytes): {BitConverter.ToString(response.ToArray())}");
        return response.ToArray();
    }
}