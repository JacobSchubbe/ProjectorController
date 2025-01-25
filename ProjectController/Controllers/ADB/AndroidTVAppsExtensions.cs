namespace ProjectController.Controllers.ADB;

public enum AndroidTVApps
{
    /// <summary>
    /// YouTube app for Android TV
    /// </summary>
    YouTube,

    /// <summary>
    /// Netflix app for Android TV
    /// </summary>
    Netflix,

    /// <summary>
    /// Amazon Prime Video app for Android TV
    /// </summary>
    AmazonPrime,

    /// <summary>
    /// Watch It app for Android TV
    /// </summary>
    WatchIt,

    /// <summary>
    /// Shahid app for Android TV
    /// </summary>
    Shahid,

    /// <summary>
    /// Disney+ for Android TV
    /// </summary>
    DisneyPlus
}

public static class AndroidTVAppsExtensions
{
    /// <summary>
    /// Returns the package and activity string for the specified AndroidTVApps value.
    /// </summary>
    /// <param name="app">The app enum value.</param>
    /// <returns>The app's package and activity string.</returns>
    public static string GetPackageActivity(this AndroidTVApps app)
    {
        return app switch
        {
            AndroidTVApps.YouTube => "com.google.android.youtube.tv/com.google.android.apps.youtube.tv.activity.ShellActivity",
            AndroidTVApps.Netflix => "com.netflix.ninja/.MainActivity",
            AndroidTVApps.AmazonPrime => "com.amazon.amazonvideo.livingroom/com.amazon.ignition.IgnitionActivity",
            AndroidTVApps.WatchIt => "com.watchit.vod/.refactor.splash.ui.SplashActivity",
            AndroidTVApps.Shahid => "net.mbc.shahidTV/.MainActivity",
            AndroidTVApps.DisneyPlus => "com.disney.disneyplus/com.bamtechmedia.dominguez.main.MainActivity",
            _ => throw new ArgumentOutOfRangeException(nameof(app), app, "Unknown app selected.")
        };
    }
}