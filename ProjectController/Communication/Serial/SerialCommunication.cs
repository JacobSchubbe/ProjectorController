using System.IO.Ports;

namespace ProjectController.Communication.Serial;

public class SerialCommunication
{
    private SerialPort? serialPort;
    private byte endOfLine;
    
    public Task Connect(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits, Handshake handshake, byte endOfLine)
    {
        try
        {
            if (serialPort == null)
                return Task.CompletedTask;
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            serialPort.Handshake = handshake; // Flow control
            serialPort.ReadTimeout = 1000;    // Timeout in milliseconds
            serialPort.WriteTimeout = 1000;
            this.endOfLine = endOfLine;

            serialPort.Open();
            Console.WriteLine($"Opened serial port {portName} at {baudRate} baud, {dataBits} data bits, parity: {parity}, stop bits: {stopBits}, flow control: {handshake}");
        }
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine($"Access to the port is denied: {e.Message}");
        }
        catch (IOException e)
        {
            Console.WriteLine($"I/O error: {e.Message}");
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine($"Invalid operation: {e.Message}");
        }
        catch (TimeoutException e)
        {
            Console.WriteLine("The operation timed out.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while open serial port: {e.Message}");
        }
        
        return Task.CompletedTask;
    }

    public void WriteCommand(byte[] command)
    {
        if (serialPort == null)
        {
            Console.WriteLine($"Serial port is not open. Unable to send command {command}.");
            return;
        }
        
        serialPort.Write(command, 0, command.Length); // Send the raw bytes
        Console.WriteLine($"Sent command: {command}");
    }

    private byte[]? ReadResponse()
    {
        if (serialPort == null)
        {
            Console.WriteLine($"Serial port is not open. Unable to read response.");
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
                Console.WriteLine("Response timed out.");
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while reading response: {e.Message}");
                break;
            }
        }
        Console.WriteLine($"Received response (in bytes): {BitConverter.ToString(response.ToArray())}");
        return response.ToArray();
    }
}