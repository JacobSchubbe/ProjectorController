// RUN apt-get update && apt-get install -y android-tools-adb

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ProjectController.ADB;

public class ADBClient
{
    private readonly Action<string> logger;
    private readonly bool _verbose;
    private readonly bool _showCommand;
    private readonly List<string> _devices;
    private string? _selectedDevice;
    private Process? _serverProcess;

    public ADBClient(Action<string> logger, bool verbose = false, bool showCommand = false)
    {
        this.logger = logger ?? (message => { });
        _verbose = verbose;
        _showCommand = showCommand;
        _devices = new List<string>();
        Log("ADB client initialized.");
        
        // StartServer();
    }

    private class CommandSubprocess
    {
        private Process? process;
        private bool blocking;
        
        public CommandSubprocess(Process? process, bool blocking)
        {
            this.blocking = blocking;
            this.process = process;
        }

        public static implicit operator Process?(CommandSubprocess? command)
        {
            if (command is { blocking: false })
            {
                // For non-blocking calls, start reading asynchronous output
                command.process?.BeginOutputReadLine();
                command.process?.BeginErrorReadLine();
                return command.process; // Return the running Process object
            }

            if (command is { blocking: true })
            {
                // Wait for the process to complete and capture the output
                var result = command.process?.StandardOutput.ReadToEnd();
                command.process?.WaitForExit();
                return null;
            }

            return null;
        }
        
        public static implicit operator string(CommandSubprocess? command)
        {
            if (command is { blocking: false })
            {
                // For non-blocking calls, start reading asynchronous output
                command.process?.BeginOutputReadLine();
                command.process?.BeginErrorReadLine();
                return string.Empty; // Return the running Process object
            }

            if (command is { blocking: true })
            {
                // Wait for the process to complete and capture the output
                var result = command.process?.StandardOutput.ReadToEnd();
                command.process?.WaitForExit();
                return result?.Trim() ?? string.Empty;
            }
            
            return string.Empty;
        }
        
    }
    
    private static string GetMachineArchitecture()
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            return "x64";
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            return "arm64";
    
        throw new NotSupportedException("Unknown architecture.");
    }
    
    private CommandSubprocess ExecuteCommand(string command, bool blocking = true, bool includeSelectedSerial = true)
    {
        var adbBasePath = GetMachineArchitecture() switch
        {
            "x64" => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ADB", "Resources", "Windows", "adb.exe"),
            _ => "adb"
        };

        if (!File.Exists(adbBasePath))
        {
            throw new FileNotFoundException("ADB executable not found at path: " + adbBasePath);
        }
        
        var adbCommand = "";
        if (!string.IsNullOrEmpty(_selectedDevice) && includeSelectedSerial)
        {
            adbCommand += $"-s {_selectedDevice} ";
        }
        adbCommand += command;

        if (_verbose && _showCommand)
        {
            Log($"Executing command: {adbCommand}");
        }

        var process = new Process { StartInfo = new ProcessStartInfo
            {
                FileName = adbBasePath,
                Arguments = adbCommand,
                RedirectStandardOutput = true,
                RedirectStandardError = blocking,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true // Allow monitoring the process (like events)
        };
        process.OutputDataReceived += (sender, args) => Log(args.Data ?? "No output data received");
        process.ErrorDataReceived += (sender, args) => Log(args.Data ?? "No error data received");
        
        process.Start();
        return new CommandSubprocess(process, blocking);
    }

    private void Log(string message)
    {
        if (_verbose)
        {
            Console.WriteLine(message);
        }
    }

    public bool StartServer()
    {
        Log("Starting ADB server...");
        _serverProcess = ExecuteCommand("start-server", blocking: true, includeSelectedSerial: false);
        // Thread.Sleep(5000); // Give time for the server to start.

        if (_serverProcess != null)
        {
            Log("ADB server started successfully.");
            return true;
        }
        else
        {
            Log("Failed to start ADB server.");
            return false;
        }
    }

    public bool KillServer()
    {
        Log("Stopping ADB server...");
        ExecuteCommand("kill-server", includeSelectedSerial: false);

        if (_serverProcess != null)
        {
            _serverProcess.Kill();
            _serverProcess = null;
            Clean();
            Log("ADB server stopped successfully.");
            return true;
        }

        Log("No running ADB server process found.");
        return false;
    }

    private void Clean()
    {
        Log("Cleaning state...");
        _devices.Clear();
        _selectedDevice = null;
    }

    public bool Connect(string ip)
    {
        Log($"Connecting to {ip}...");
        ExecuteCommand($"disconnect {ip}", includeSelectedSerial: false);
        string result = ExecuteCommand($"connect {ip}", includeSelectedSerial: false);
        if (result.Contains("connected"))
        {
            GetDevices();
            _selectedDevice = _devices[_devices.Count - 1];
            Log($"Device {_selectedDevice} connected successfully.");
            return true;
        }
        else
        {
            Log($"Failed to connect to {ip}.");
            return false;
        }
    }

    public bool IsConnected(string ip)
    {
        foreach (var device in _devices)
        {
            if (device.StartsWith(ip))
            {
                Log($"Device {ip} is connected.");
                return true;
            }
        }
        Log($"Device {ip} is not connected.");
        return false;
    }

    public bool Disconnect()
    {
        Log("Disconnecting device...");
        var result = ExecuteCommand("disconnect");
        if (((string)result).Contains("disconnected"))
        {
            GetDevices();
            _selectedDevice = null;
            Log("Device disconnected successfully.");
            return true;
        }
        else
        {
            Log("Error disconnecting device.");
            return false;
        }
    }

    public List<string> GetDevices()
    {
        Log("Getting connected devices...");
        _devices.Clear();
        var result = ExecuteCommand("devices -l", includeSelectedSerial: false);

        string[] lines = ((string)result).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 1) // Skip the `List of devices` header.
        {
            for (var i = 1; i < lines.Length; i++)
            {
                var serial = lines[i].Split(' ')[0];
                _devices.Add(serial);
                Log($"Found device: {serial}");
            }
        }

        return _devices;
    }

    public bool SelectDevice(string deviceSerial)
    {
        if (_devices.Contains(deviceSerial))
        {
            _selectedDevice = deviceSerial;
            Log($"Selected device: {_selectedDevice}");
            return true;
        }
        else
        {
            Log($"Device {deviceSerial} not found.");
            return false;
        }
    }

    public string? GetSelectedDevice()
    {
        Log($"Selected device: {_selectedDevice ?? "None"}");
        return _selectedDevice;
    }

    public Dictionary<string, string>? GetDeviceInfo()
    {
        if (_selectedDevice == null) return null;

        Log("Getting device info...");
        var deviceInfo = new Dictionary<string, string>();
        var results = ((string)ExecuteCommand("shell getprop")).Split(Environment.NewLine);

        foreach (var data in results)
        {
            var match = Regex.Match(data, @"\[([^\]]+)\]: \[([^\]]+)\]");
            if (match.Success)
            {
                var prop = match.Groups[1].Value;
                var value = match.Groups[2].Value;
                deviceInfo[prop] = value;
                Log($"{prop}: {value}");
            }
        }

        return deviceInfo;
    }

    public string? GetIpAddress(string interfaceName = "wlan0")
    {
        if (_selectedDevice == null) return null;

        Log($"Getting IP for interface {interfaceName}");
        var result = ExecuteCommand($"shell ifconfig {interfaceName}");
        var match = Regex.Match(result, @"inet addr:([^\s]+)");
        return match.Success ? match.Groups[1].Value : null;
    }

    public bool Install(string apkPath, bool replace = true)
    {
        if (_selectedDevice == null) return false;

        var command = "install ";
        if (replace)
        {
            command += "-r ";
        }
        command += apkPath;

        Log($"Installing APK: {apkPath}...");
        var result = ExecuteCommand(command);

        if (((string)result).Contains("Success"))
        {
            Log("APK installed successfully.");
            return true;
        }

        Log("APK installation failed.");
        return false;
    }

    public bool Uninstall(string packageName, bool keepData = false)
    {
        if (_selectedDevice == null) return false;

        var command = "uninstall ";
        if (keepData)
        {
            command += "-k ";
        }
        command += packageName;

        Log($"Uninstalling package: {packageName}...");
        var result = ExecuteCommand(command);

        if (((string)result).Contains("Success"))
        {
            Log("Package uninstalled successfully.");
            return true;
        }

        Log("Package uninstallation failed.");
        return false;
    }

    public bool StartApp(string packageName, string activity, bool wait = true, bool stop = true)
    {
        if (_selectedDevice == null) return false;

        var command = "am start ";
        if (wait) command += "-W ";
        if (stop) command += "-S ";

        command += $"{packageName}/{activity}";
        Log($"Starting app: {packageName}...");

        var result = ExecuteCommand($"shell {command}");
        if (((string)result).Contains("Error"))
        {
            Log("Failed to start the app.");
            return false;
        }

        Log("App started successfully.");
        return true;
    }

    public bool StopApp(string packageName)
    {
        if (_selectedDevice == null) return false;

        Log($"Stopping app: {packageName}...");
        var result = ExecuteCommand($"shell am force-stop {packageName}");
        return ((string)result).Contains("Done");
    }

    public bool Reboot(string? mode = null)
    {
        if (_selectedDevice == null) return false;

        var command = "reboot ";
        if (!string.IsNullOrEmpty(mode))
        {
            command += mode;
        }

        Log($"Rebooting device{(mode != null ? " into " + mode : string.Empty)}...");
        var result = ExecuteCommand(command);

        return !((string)result).Contains("error");
    }
    
    public bool? IsPoweredOn()
    {
        if (_selectedDevice == null)
        {
            return null;
        }
        try
        {
            // Example option #1: Use alternative dumpsys power command (results can vary by Android version).
            var results = ExecuteShellCommand("dumpsys power | grep \"Display Power\"");

            // Optionally, handle other outputs like checking for "mWakefulness" or "mHoldingDisplaySuspendBlocker".
            return results.Contains("ON");
        }
        catch
        {
            return null;
        }
    }
    
    public string ExecuteShellCommand(string command)
    {
        if (_selectedDevice == null)
        {
            throw new InvalidOperationException("No device is selected.");
        }
        return ExecuteCommand($"shell {command}");
    }
    
    public Task SendKeyEventInput(KeyCodes keycode, bool longPress = false)
    {
        if (_selectedDevice == null)
        {
            throw new InvalidOperationException("No device is selected.");
        }

        // Construct the input keyevent command
        var command = $"input keyevent {((int)keycode).ToString()}";
        if (longPress)
        {
            command += " --longpress";
        }

        ExecuteShellCommand(command);
        return Task.CompletedTask;
    }
    
    public void SendTextInput(string text, bool encodeSpaces = true)
    {
        if (_selectedDevice == null)
        {
            throw new InvalidOperationException("No device is selected.");
        }

        // Preprocess the text to handle spaces
        var processedText = encodeSpaces ? text.Replace(" ", "%s") : text;

        // Execute input text command
        ExecuteShellCommand($"input text {processedText}");
    }
}