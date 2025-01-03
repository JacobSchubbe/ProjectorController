import serial
import time

def main():
    # Configuration for the serial port
    port_name = "COM3"  # Replace with the correct COM port for your device
    baud_rate = 9600    # Adjust according to your device's specifications
    timeout = 1         # Time in seconds to wait for a response (optional)

    try:
        # Open the serial port
        with serial.Serial(port=port_name, baudrate=baud_rate, timeout=timeout) as ser:
            print(f"Opened serial port {port_name} at {baud_rate} baud.")

            # Command to send through the serial port
            command = "YOUR_COMMAND\r\n"  # Replace 'YOUR_COMMAND' with the actual command

            # Send the command
            ser.write(command.encode())
            print(f"Sent command: {command.strip()}")

            # Optional: Read response from the serial device
            time.sleep(1)  # Wait for the device to process the command
            response = ser.read_all().decode().strip()
            print(f"Received response: {response}")

    except serial.SerialException as e:
        print(f"Error opening or using the serial port: {e}")

if __name__ == "__main__":
    main()
