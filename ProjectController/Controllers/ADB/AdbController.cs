using System.Text.RegularExpressions;

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
        _ = AdbClient.DetectVpnConnectionChange(CancellationToken.None);
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

    private async Task StopAllApps(bool shouldForceStopVpn)
    {
        foreach (var app in Enum.GetValues<AndroidTVApps>())
        {
            Log($"Try to stop app: {app}");
            if (app is AndroidTVApps.Surfshark)
            {
                Log($"Checking {app.ToString()}...");
                if (shouldForceStopVpn)
                    ForceStopApp(app);
            }
            else
            {
                ForceStopApp(app);
            }
            await Task.Delay(100);
        }
    }

    public bool IsVpnConnected()
    {
        return AdbClient.IsVpnConnected();
    }

    public ADBClient GetAdbClient()
    {
        return AdbClient;
    }
    
    public Dictionary<KeyCodes, Func<bool, Task<string>>> KeyCommands => new()
    {
        { KeyCodes.KEYCODE_HOME, async isLongPress => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_HOME, isLongPress) },
        { KeyCodes.KEYCODE_TV, async isLongPress => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_TV, isLongPress) },
        { KeyCodes.KEYCODE_BACK, async isLongPress => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_BACK, isLongPress) },
        { KeyCodes.KEYCODE_DPAD_UP, async isLongPress => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_DPAD_UP, isLongPress) },
        { KeyCodes.KEYCODE_DPAD_DOWN, async isLongPress => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_DPAD_DOWN, isLongPress) },
        { KeyCodes.KEYCODE_DPAD_LEFT, async isLongPress => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_DPAD_LEFT, isLongPress) },
        { KeyCodes.KEYCODE_DPAD_RIGHT, async isLongPress => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_DPAD_RIGHT, isLongPress) },
        { KeyCodes.KEYCODE_ENTER, async isLongPress => await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_ENTER, isLongPress) },

        { KeyCodes.VpnOff, async _ => await SetVpnStatusAndReopenApp(false)},
        { KeyCodes.VpnOn, async _ => await SetVpnStatusAndReopenApp(true)},
        { KeyCodes.VpnStatusQuery, _ => Task.FromResult(AdbClient.IsVpnConnected().ToString()) },
    };

    private AndroidTVApps? GetCurrentForegroundApp()
    {
        Log("Getting the current foreground app...");
        try
        {
            // Updated ADB command to get the top activity (foreground app)
            var adbCommand = "dumpsys activity activities | grep -E 'ResumedActivity'";
            var output = AdbClient.ExecuteShellCommand(adbCommand);
            
            // Improved regex to extract the package name and activity
            var match = Regex.Match(output, @"u0\s+([a-zA-Z0-9\.]+)/[a-zA-Z0-9\.]+");

            if (match.Success)
            {
                var packageName = match.Groups[1].Value;
                Log($"Current foreground app package: {packageName}");

                foreach (AndroidTVApps app in Enum.GetValues(typeof(AndroidTVApps)))
                {
                    if (app.GetPackageActivity().Contains(packageName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return app;
                    }
                }
            }
            else
            {
                Log("No matching foreground app found OR regex parsing failed.");
            }
        }
        catch (Exception ex)
        {
            Log($"An error occurred while getting the foreground app: {ex.Message}");
        }

        return null; // Return null if no foreground app is detected
    }
    private async Task<string> SetVpnStatusAndReopenApp(bool isVpnOn)
    {
        try
        {
            var currentVpnStatus = IsVpnConnected();
            if (currentVpnStatus == isVpnOn)
            {
                Log($"Vpn already has a status of {isVpnOn}. Will not change status nor force close apps.");
                return AdbConstants.AdbSuccess;
            }
            
            // Step 1: Detect the currently open app
            Log("Detecting the currently open app...");
            var currentApp = GetCurrentForegroundApp();
            currentApp = currentApp == AndroidTVApps.Surfshark ? null : currentApp;
            Log(currentApp == null
                ? "No foreground app detected. Defaulting to home screen."
                : $"Detected currently open app: {currentApp}");
            
            // Step 2: Open the Surfshark VPN app
            if (isVpnOn)
            {
                Log("Opening Surfshark VPN...");
                await OpenApp(AndroidTVApps.Surfshark);
                
                // Step 3: Wait for VPN connection
                Log("Waiting for VPN connection...");
                bool vpnConnected = false;
                const int maxRetries = 10; // Wait up to 10 seconds
                for (int i = 0; i < maxRetries; i++)
                {
                    if (IsVpnConnected())
                    {
                        vpnConnected = true;
                        break;
                    }

                    // Wait 1 second before checking again
                    await Task.Delay(1000);
                }

                if (!vpnConnected)
                {
                    Log("Failed to establish VPN connection.");
                    return AdbConstants.AdbFailure;
                }

                Log("VPN connection established successfully.");
            }

            // Step 4: Force stop all apps
            Log("Stopping all apps...");
            await StopAllApps(!isVpnOn);

            // Step 5: Reopen the original app
            if (currentApp != null)
            {
                Log($"Reopening the previously open app: {currentApp}");
                await OpenApp(currentApp.Value); // Use .Value since currentApp is nullable
            }
            else
            {
                Log("No previous app detected to reopen. Going to home screen.");
                await AdbClient.SendKeyEventInput(KeyCodes.KEYCODE_HOME);
            }

            Log($"Switch {(isVpnOn ? "to" : "from")} VPN and reopen app process completed.");
            return AdbConstants.AdbSuccess;
        }
        catch (Exception ex)
        {
            Log($"An error occurred during the process: {ex.Message}");
            return AdbConstants.AdbFailure;
        }
    }    
    
    public async Task OpenApp(AndroidTVApps app)
    {
        await (app switch
        {
            AndroidTVApps.YouTube => ExecuteOpenApp(AndroidTVApps.YouTube),
            AndroidTVApps.Netflix => ExecuteOpenApp(AndroidTVApps.Netflix),
            AndroidTVApps.AmazonPrime => ExecuteOpenApp(AndroidTVApps.AmazonPrime),
            AndroidTVApps.DisneyPlus => ExecuteOpenApp(AndroidTVApps.DisneyPlus),
            AndroidTVApps.Crunchyroll => ExecuteOpenApp(AndroidTVApps.Crunchyroll),
            AndroidTVApps.Spotify => ExecuteOpenApp(AndroidTVApps.Spotify),
            AndroidTVApps.Surfshark => ExecuteOpenApp(AndroidTVApps.Surfshark),
            _ => throw new ArgumentOutOfRangeException(nameof(app), app, $"App {app} is not mapped to any command.")
        });
    }
    
    private Task ExecuteOpenApp(AndroidTVApps app)
    {
        var packageActivity = app.GetPackageActivity();
        var parts = packageActivity.Split('/');
        var package = parts[0];
        var activity = parts[1];
        AdbClient.StartApp(package, activity);
        return Task.CompletedTask;
    }

    private void ForceStopApp(AndroidTVApps app)
    {
        Log($"Force stopping app: {app}");
        var packageActivity = app.GetPackageActivity();
        var parts = packageActivity.Split('/');
        var package = parts[0];
        AdbClient.StopApp(package);
    }
}