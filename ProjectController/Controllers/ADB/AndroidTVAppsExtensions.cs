namespace ProjectController.Controllers.ADB;

public enum AndroidTVApps
{
    YouTube,
    Netflix,
    AmazonPrime,
    DisneyPlus,
    Crunchyroll,
    Spotify,
    Surfshark,
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
            AndroidTVApps.DisneyPlus => "com.disney.disneyplus/com.bamtechmedia.dominguez.main.MainActivity",
            AndroidTVApps.Crunchyroll => "com.crunchyroll.crunchyroid/.main.ui.MainActivity",
            AndroidTVApps.Spotify => "com.spotify.tv.android/.SpotifyTVActivity",
            AndroidTVApps.Surfshark => "com.surfshark.vpnclient.android/.StartActivity",
            _ => throw new ArgumentOutOfRangeException(nameof(app), app, "Unknown app selected.")
        };
    }
}