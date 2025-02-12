using System.IO.Ports;
using System.Text;

namespace ProjectController.Communication.Serial;

public class SerialCommunication
{
    private ILogger<SerialCommunication> logger;
    private SerialPort? serialPort;

    private string[] possiblePortNames = Array.Empty<string>();
    private BaudRate baudRate;
    private DataBits dataBits;
    private Parity parity;
    private StopBits stopBits;
    private Handshake handshake;
    private byte[] endOfLine = { 0x0A, 0x0D }; // End of line bytes [0x0A, 0x0D];
    private event Func<Task>? disconnectEvent;
    private event Func<Task>? connectEvent;

    public SerialCommunication(ILogger<SerialCommunication> logger)
    {
        this.logger = logger;
        serialPort ??= new SerialPort();
    }

    public void StartCommunication(string[] possiblePortNames, BaudRate baudRate, DataBits dataBits, Parity parity, StopBits stopBits, Handshake handshake, byte[] endOfLine)
    {
        this.possiblePortNames = possiblePortNames;
        this.baudRate = baudRate;
        this.dataBits = dataBits;
        this.parity = parity;
        this.stopBits = stopBits;
        this.handshake = handshake;
        this.endOfLine = endOfLine;
        _ = CheckConnection(CancellationToken.None);
    }
    
    private Task<bool> TryToConnect(string portName)
    {
        try
        {
            serialPort ??= new SerialPort();
            serialPort.PortName = portName;
            serialPort.BaudRate = (int)baudRate;
            serialPort.DataBits = (int)dataBits;
            serialPort.Parity = parity;
            serialPort.StopBits = stopBits;
            serialPort.Handshake = handshake; // Flow control
            serialPort.ReadTimeout = 4000;    // Timeout in milliseconds
            serialPort.WriteTimeout = 1000;
            logger.LogTrace($"Connecting to port {portName}, baudRate: {baudRate}, dataBits: {dataBits}, parity: {parity}, stopBits: {stopBits}, handshake: {handshake}, endOfLine: {endOfLine}");
            serialPort.Open();
            logger.LogTrace($"Opened serial port {portName} at {baudRate} baud, {dataBits} data bits, parity: {parity}, stop bits: {stopBits}, flow control: {handshake}");
        }
        catch (Exception e) when (e is UnauthorizedAccessException or IOException or InvalidOperationException or TimeoutException)
        {
            // serialPort?.Close();
            serialPort = null;
            switch (e)
            {
                case UnauthorizedAccessException:
                    logger.LogError($"Access to the port is denied: {e.Message}");
                    break;
                case IOException:
                    logger.LogError($"I/O error: {e.Message}");
                    break;
                case InvalidOperationException:
                    logger.LogError($"Invalid operation: {e.Message}");
                    break;
                case TimeoutException:
                    logger.LogError("The operation timed out.");
                    break;
            }
            
            return Task.FromResult(false);
        }
        catch (Exception e)
        {
            logger.LogError($"Error while open serial port: {e.Message}");
        }
        
        return Task.FromResult(true);
    }

    public void WriteCommand(string command)
    {
        WriteCommand(Encoding.ASCII.GetBytes(command));
    }

    
    private async Task CheckConnection(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!IsConnected)
            {
                await (disconnectEvent?.Invoke() ?? Task.CompletedTask);
                foreach (var portName in possiblePortNames)
                {
                    if (await TryToConnect(portName))
                    {
                        await (connectEvent?.Invoke() ?? Task.CompletedTask);
                        break;
                    }
                }
            }
            
            await Task.Delay(500, cancellationToken);
        }
    }
    
    public bool IsConnected => serialPort?.IsOpen ?? false;

    private void WriteCommand(byte[] command)
    {
        if (serialPort is not { IsOpen: true })
        {
            logger.LogError($"Serial port is not open. Unable to send command {command}.");
            return;
        }
        
        serialPort.DiscardInBuffer();
        serialPort.DiscardOutBuffer();
        serialPort.Write(command, 0, command.Length); // Send the raw bytes
        logger.LogTrace("Sent command: {command}", BitConverter.ToString(command));
    }

    public string ReadOneLineOfResponseAsString()
    {
        var response = ReadOneLineOfResponseAsBytes();
        logger.LogTrace("Response: {response}", BitConverter.ToString(response ?? Array.Empty<byte>()));
        return response == null ? string.Empty : Encoding.ASCII.GetString(response);
    }

    private byte[]? ReadOneLineOfResponseAsBytes()
    {
        if (serialPort is not { IsOpen: true })
        {
            logger.LogError($"Serial port is not open. Unable to read response.");
            return null;
        }

        var response = new List<byte>();

        while (true)        
        {
            try
            {
                var readByte = serialPort.ReadByte(); // Read a single byte
                response.Add((byte)readByte);

                if (response.Count >= endOfLine.Length && response.TakeLast(endOfLine.Length).SequenceEqual(endOfLine))
                {
                    break;
                }
            }
            catch (TimeoutException)
            {
                logger.LogError("Response timed out.");
                throw;
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