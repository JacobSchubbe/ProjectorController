namespace ProjectController.Controllers.ADB;

public class AdbController
{
    private readonly ILogger<AdbController> logger;
    internal readonly ADBClient AdbClient;
    internal readonly string Ip = "192.168.0.236:5555";

    public AdbController(ILogger<AdbController> logger, ADBClient adbClient)
    {
        this.logger = logger;
        AdbClient = adbClient;
        _ = AdbClient.DetectConnectionChange(Ip, CancellationToken.None);
    }

    private void Log(string message)
    {
        logger.LogDebug(message);
    }
    
    public async Task<bool> Connect(CancellationToken cancellationToken = default)
    {
        return await AdbClient.Connect(Ip, cancellationToken);
    }
    
    public bool IsConnected()
    {
        var status = AdbClient.IsConnected(Ip);
        Log($"AndroidTV Connection status: {status}");
        return status;
    }

    public bool StartVpn()
    {
        var adbCommand = "monkey -p com.surfshark.vpnclient.android -c android.intent.category.LAUNCHER 1";
        var output = AdbClient.ExecuteShellCommand(adbCommand);
        Log(output);
        return true;
    }
    
    public void StopVpn()
    {
        ForceStopApp(AndroidTVApps.Surfshark);
    }

    public void StopAllApps()
    {
        foreach (var app in Enum.GetValues<AndroidTVApps>())
        {
            ForceStopApp(app);
        }
    }
    
    public bool IsVpnConnected()
    {
        var adbCommand = "dumpsys connectivity | grep -i vpn";
        try
        {
            // Execute the ADB command
            var output = AdbClient.ExecuteShellCommand(adbCommand);
            if (output.Contains("VPN CONNECTED") && output.Contains("IS_VPN"))
            {
                Log("A VPN connection is active, managed by Surfshark.");
            }
            else
            {
                Log("No active VPN connection found.");
            }
        }
        catch (Exception ex)
        {
            // Handle any errors
            Log("An error occurred: " + ex.Message);
        }

        return false;
    }

    public ADBClient GetAdbClient()
    {
        return AdbClient;
    }
    
    // Define the dictionary mapping KeyCodes to corresponding methods.
    public Dictionary<KeyCodes, Func<Task>> KeyCommands => new()
    {
        // Navigation Commands
        { KeyCodes.KEYCODE_HOME, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_HOME) },
        { KeyCodes.KEYCODE_TV, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_TV) },
        { KeyCodes.KEYCODE_BACK, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_BACK) },
        { KeyCodes.KEYCODE_DPAD_UP, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_DPAD_UP) },
        { KeyCodes.KEYCODE_DPAD_DOWN, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_DPAD_DOWN) },
        { KeyCodes.KEYCODE_DPAD_LEFT, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_DPAD_LEFT) },
        { KeyCodes.KEYCODE_DPAD_RIGHT, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_DPAD_RIGHT) },
        { KeyCodes.KEYCODE_ENTER, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_ENTER) },

        // Volume Commands
        { KeyCodes.KEYCODE_VOLUME_UP, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_VOLUME_UP) },
        { KeyCodes.KEYCODE_VOLUME_DOWN, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_VOLUME_DOWN) },
        { KeyCodes.KEYCODE_VOLUME_MUTE, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_VOLUME_MUTE) },

        // Power Commands
        { KeyCodes.KEYCODE_POWER, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_POWER) },
        { KeyCodes.KEYCODE_SLEEP, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_SLEEP) },
        { KeyCodes.KEYCODE_SOFT_SLEEP, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_SOFT_SLEEP) },
        { KeyCodes.KEYCODE_WAKEUP, async () => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_WAKEUP) },

        // Channel Commands
        { KeyCodes.KEYCODE_CHANNEL_UP, async () => 
        {
            await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_TV);
            await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_CHANNEL_UP);
        }},
        { KeyCodes.KEYCODE_CHANNEL_DOWN, async () =>
        {
            await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_TV);
            await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_CHANNEL_DOWN);
        }}
    };
    
    // Method for channel numbers since it has special logic.
    public void PressChannelNumber(string channelNumber)
    {
        var numbersKeyCodes = new Dictionary<char, KeyCodes>
        {
            { '0', KeyCodes.KEYCODE_0 },
            { '1', KeyCodes.KEYCODE_1 },
            { '2', KeyCodes.KEYCODE_2 },
            { '3', KeyCodes.KEYCODE_3 },
            { '4', KeyCodes.KEYCODE_4 },
            { '5', KeyCodes.KEYCODE_5 },
            { '6', KeyCodes.KEYCODE_6 },
            { '7', KeyCodes.KEYCODE_7 },
            { '8', KeyCodes.KEYCODE_8 },
            { '9', KeyCodes.KEYCODE_9 }
        };

        AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_TV);

        foreach (var digit in channelNumber)
        {
            if (numbersKeyCodes.TryGetValue(digit, out var digitKeyCode))
            {
                AdbClient.SendKeyEventInput(digitKeyCode);
            }
        }
    }
    
    
    // App Commands
    public void OpenApp(AndroidTVApps app)
    {
        if (AppCommands.TryGetValue(app, out var action))
        {
            action.Invoke();
        }
        else
        {
            Console.WriteLine($"App {app} is not mapped to any command.");
        }
    }
    private Dictionary<AndroidTVApps, Action> AppCommands => new()
    {
        { AndroidTVApps.YouTube, () => ExecuteOpenApp(AndroidTVApps.YouTube) },
        { AndroidTVApps.Netflix, () => ExecuteOpenApp(AndroidTVApps.Netflix) },
        { AndroidTVApps.AmazonPrime, () => ExecuteOpenApp(AndroidTVApps.AmazonPrime) },
        { AndroidTVApps.DisneyPlus, () => ExecuteOpenApp(AndroidTVApps.DisneyPlus) },
        { AndroidTVApps.Crunchyroll, () => ExecuteOpenApp(AndroidTVApps.Crunchyroll) },
        { AndroidTVApps.Spotify, () => ExecuteOpenApp(AndroidTVApps.Spotify) },
        { AndroidTVApps.Surfshark, () => ExecuteOpenApp(AndroidTVApps.Surfshark) },
    };

    private void ExecuteOpenApp(AndroidTVApps app)
    {
        var packageActivity = app.GetPackageActivity();
        var parts = packageActivity.Split('/');
        var package = parts[0];
        var activity = parts[1];
        AdbClient.StartApp(package, activity);
    }

    public void ForceStopApp(AndroidTVApps app)
    {
        var packageActivity = app.GetPackageActivity();
        var parts = packageActivity.Split('/');
        var package = parts[0];
        AdbClient.StopApp(package);
    }
}