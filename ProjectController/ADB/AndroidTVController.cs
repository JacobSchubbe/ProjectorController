namespace ProjectController.ADB;

public class AndroidTVController
{
    // private readonly ILogger<AndroidTVController> logger;
    private readonly ADBClient _adbClient;
    private readonly string _ip = "192.168.0.236:5555";
    private readonly bool verbose = false;
    private readonly bool showCommand = false;

    public AndroidTVController()
    {
        // this.logger = logger;
        _adbClient = new ADBClient(Log, verbose, showCommand);
        Testing().ConfigureAwait(true).GetAwaiter();
    }

    private void Log(string message)
    {
        Console.WriteLine(message);
    }

    private async Task Testing()
    {
        Connect();
        await Task.Delay(5000);
        PressHome();
        await Task.Delay(5000);
        PressDpadRight();
        await Task.Delay(5000);
        PressDpadDown();
    }
    
    public bool Connect()
    {
        Log($"Connecting to {_ip}...");
        var result = _adbClient.Connect(_ip);
        if (result)
        {
            Log("Connected successfully.");
        }
        else
        {
            Log("Connection failed.");
        }
        return result;
    }

    public bool IsConnected()
    {
        var status = _adbClient.IsConnected(_ip);
        Log($"Connection status: {status}");
        return status;
    }

    public ADBClient GetAdbClient()
    {
        return _adbClient;
    }

    // Navigation Commands
    public void PressHome() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_HOME);
    public void PressTv() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_TV);
    public void PressBack() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_BACK);
    public void PressDpadUp() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_DPAD_UP);
    public void PressDpadDown() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_DPAD_DOWN);
    public void PressDpadLeft() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_DPAD_LEFT);
    public void PressDpadRight() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_DPAD_RIGHT);
    public void PressEnter() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_ENTER);

    // Volume Commands
    public void PressVolumeUp() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_VOLUME_UP);
    public void PressVolumeDown() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_VOLUME_DOWN);
    public void PressVolumeMute() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_VOLUME_MUTE);

    // Power Commands
    public bool? IsPoweredOn() => _adbClient.IsPoweredOn();
    public void PressPower() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_POWER);
    public void PressSleep() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_SLEEP);
    public void PressSoftSleep() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_SOFT_SLEEP);
    public void PressWakeup() => _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_WAKEUP);

    // Channel Commands
    public void PressChannelUp()
    {
        _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_TV);
        _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_CHANNEL_UP);
    }

    public void PressChannelDown()
    {
        _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_TV);
        _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_CHANNEL_DOWN);
    }

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

        _adbClient.SendKeyEventInput(KeyCodes.KEYCODE_TV);

        foreach (var digit in channelNumber)
        {
            _adbClient.SendKeyEventInput(numbersKeyCodes[digit]);
        }
    }

    // App Commands
    public void OpenApp(AndroidTVApps app)
    {
        var packageActivity = app.GetPackageActivity(); // Use the GetPackageActivity extension method
        var parts = packageActivity.Split('/'); // Split the package and activity strings
        var package = parts[0];
        var activity = parts[1];
        _adbClient.StartApp(package, activity);
    }
    public void OpenYouTube() => OpenApp(AndroidTVApps.YouTube);
    public void OpenNetflix() => OpenApp(AndroidTVApps.Netflix);
    public void OpenAmazonPrime() => OpenApp(AndroidTVApps.AmazonPrime);
    public void OpenWatchIt() => OpenApp(AndroidTVApps.WatchIt);
    public void OpenShahid() => OpenApp(AndroidTVApps.Shahid);
}