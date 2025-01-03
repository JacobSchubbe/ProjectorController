import socket
from time import sleep

# communication = bytes([0x45, 0x53, 0x43, 0x2F, 0x56, 0x50, 0x2E, 0x6E, 0x65, 0x74, 0x10, 0x03, 0x00, 0x00, 0x00, 0x00]) # ESC/VP.net
# queryState = bytes([0x50, 0x57, 0x52, 0x3F, 0x0D]) # PWR?
# powerOff = bytes([0x50, 0x57, 0x52, 0x20, 0x4F, 0x46, 0x46, 0x5C, 0x72, 0x5C, 0x6E]) # PWROFF
# naturalColorMode = bytes([0x43, 0x4D, 0x4F, 0x44, 0x45, 0x20, 0x30, 0x37, 0x0D]) # CMODE 07
communication = "ESC/VP.net\x10\x03\x00\x00\x00\x00"
naturalColorMode = "CMODE 07"

powerQuery = "PWR?"
powerOff = "PWR OFF"
powerOn = "PWR ON"

volumeUp = "VOL INC" # only with non-ARC sound
volumeDown = "VOL DEC" # only with non-ARC sound
volumeQuery = "VOL?"

sourceHDMI1 = "SOURCE 30"
sourceHDMI2 = "SOURCE A0"
sourceHDMI3 = "SOURCE C0"
sourceHDMILan = "SOURCE 53"

# same for CONTRAST, DENSITY, TINT
brightnessUp = "BRIGHT INC"
brightnessDown = "BRIGHT DEC"
brightnessQuery = "BRIGHT?"



keyPower = "KEY 01"
keyMenu = "KEY 03"
keyUp = "KEY 35"
keyDown = "KEY 36"
keyLeft = "KEY 37"
keyRight = "KEY 38"
keyEnter = "KEY 16"
keyHome = "KEY 04"

keyVolumeUp = "KEY 56"
keyVolumeDown = "KEY 57"
keyAVMuteBlank = "KEY 3E"
keyKeysTone = "KEY C8"
keyHDMILink = "KEY 8E"

keyPlay = "KEY D1"
keyStop = "KEY D2"
keyPause = "KEY D3"
keyRewind = "KEY D4"
keyFastFoward = "KEY D5"
keyBackward = "KEY D6"
keyForward = "KEY D7"
keyMute = "KEY D8"
keyLinkMenu = "KEY D9"

projectorNameQuery = "NWPNAME?"
lampHoursQuery = "LAMP?"
errorQuery = "ERR?"
sourceListQuery = "SOURCELIST?"

powerOnStatus = b'PWR=03\r:'
powerBootingStatus = b'PWR=02\r:'
powerOffStatus = b'PWR=01\r:'
errorResponse = b'Err\r:'

def send_command(s:socket, str):
    if str != communication:
        str = f'{str}\r'
    bytes = str.encode('ascii')
    s.sendall(bytes)
    print(f"Sent command: {bytes}")
    response = s.recv(1024)
    print(f"Received response: {response}")

    # if response == b'ERR\r:':
    #     s.sendall(errorQuery.encode('ascii'))
    #     print(f"Sent command: {errorQuery}")
    #     response = s.recv(1024)
    #     print(f"Received error response: {response}")


def main():
    # Configuration for the TCP connection
    host = "192.168.0.150"  # Replace with the target IP address
    port = 3629         # Replace with the target port number

    try:
        # Create a TCP socket
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            print(f"Connecting to {host}:{port}...")

            # Connect to the server
            s.connect((host, port))
            print(f"Connected to {host}:{port}.")

            send_command(s, communication)
            send_command(s, volumeQuery)
            send_command(s, volumeUp)
            send_command(s, volumeQuery)

    except socket.error as e:
        print(f"Error with the TCP connection: {e}")

if __name__ == "__main__":
    main()
