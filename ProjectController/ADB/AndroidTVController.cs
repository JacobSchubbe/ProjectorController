namespace ProjectController.ADB;

public class AndroidTVController
{
    private readonly ILogger<AndroidTVController> logger;
    internal readonly ADBClient AdbClient;
    internal readonly string Ip = "192.168.0.236:5555";
    private readonly bool verbose = false;
    private readonly bool showCommand = false;

    public AndroidTVController(ILogger<AndroidTVController> logger)
    {
        this.logger = logger;
        AdbClient = new ADBClient(Log, verbose, showCommand);
    }

    private void Log(string message)
    {
        logger.LogDebug(message);
    }
    
    public async Task<bool> Connect(CancellationToken cancellationToken = default)
    {
        Log($"Connecting to {Ip}...");
        _ = AdbClient.DetectConnectionChange(Ip, cancellationToken);
        var result = await AdbClient.Connect(Ip, cancellationToken);
        Log(result ? $"Connected successfully to {Ip}." : $"Connection failed to {Ip}.");
        return result;
    }
    
    public bool IsConnected()
    {
        var status = AdbClient.IsConnected(Ip);
        Log($"AndroidTV Connection status: {status}");
        return status;
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
        { AndroidTVApps.WatchIt, () => ExecuteOpenApp(AndroidTVApps.WatchIt) },
        { AndroidTVApps.Shahid, () => ExecuteOpenApp(AndroidTVApps.Shahid) },
        { AndroidTVApps.DisneyPlus, () => ExecuteOpenApp(AndroidTVApps.DisneyPlus) }
    };

    private void ExecuteOpenApp(AndroidTVApps app)
    {
        var packageActivity = app.GetPackageActivity(); // Use the GetPackageActivity extension method
        var parts = packageActivity.Split('/'); // Split the package and activity strings
        var package = parts[0];
        var activity = parts[1];
        AdbClient.StartApp(package, activity);
    }
}