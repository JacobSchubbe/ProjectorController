using System.Text;

namespace ProjectController.Projector;

public static class ProjectorConstants
{
    public const string ProjectorHost = "192.168.0.150";
    public const int ProjectorPort = 3629;
    
    public enum ProjectorCommands
    {
        SystemControlStartCommunication,
        SystemControlPowerQuery,
        SystemControlPowerOff,
        SystemControlPowerOn,
        SystemControlVolumeMuteOn,
        SystemControlVolumeMuteOff,
        SystemControlVolumeMuteQuery,
        SystemControlVolumeUp,
        SystemControlVolumeDown,
        SystemControlVolumeQuery,
        SystemControlSourceHDMI1,
        SystemControlSourceHDMI2,
        SystemControlSourceHDMI3,
        SystemControlSourceHDMILAN,
        SystemControlSourceQuery,
        SystemControlSourceListQuery,
        SystemInformationProjectorNameQuery,
        SystemInformationSerialNumberQuery,
        SystemInformationErrorQuery,
        SystemInformationLampHoursQuery,
        SystemInformationOperationalTimeQuery,
        SystemInformationSignalStatusQuery,
        ImageControlNaturalColorMode,
        ImageControlImageReverseHorizontalOn,
        ImageControlImageReverseHorizontalOff,
        ImageControlImageReverseHorizontalQuery,
        ImageControlImageReverseVerticalOn,
        ImageControlImageReverseVerticalOff,
        ImageControlImageReverseVerticalQuery,
        ImageControlBrightnessUp,
        ImageControlBrightnessDown,
        ImageControlBrightnessQuery,
        ImageControlStatusLEDIlluminationOn,
        ImageControlStatusLEDIlluminationOff,
        ImageControlStatusLEDIlluminationQuery,
        KeyControlPower,
        KeyControlMenu,
        KeyControlUp,
        KeyControlDown,
        KeyControlLeft,
        KeyControlRight,
        KeyControlEnter,
        KeyControlHome,
        KeyControlVolumeUp,
        KeyControlVolumeDown,
        KeyControlAVMuteBlank,
        KeyControlKeysTone,
        KeyControlHDMILink,
        KeyControlPlay,
        KeyControlStop,
        KeyControlPause,
        KeyControlRewind,
        KeyControlFastForward,
        KeyControlBackward,
        KeyControlForward,
        KeyControlMute,
        KeyControlLinkMenu,
        IRHome,
        IRESC,
        IREnter,
        IRPointerUp,
        IRPointerDown,
        IRPointerLeft,
        IRPointerRight,
    }

    public static readonly Dictionary<ProjectorCommands, string> ProjectorCommandsDictionary = new()
    {
        { ProjectorCommands.SystemControlStartCommunication, "ESC/VP.net\x10\x03\x00\x00\x00\x00" },
        { ProjectorCommands.SystemControlPowerQuery, "PWR?" },
        { ProjectorCommands.SystemControlPowerOff, "PWR OFF" },
        { ProjectorCommands.SystemControlPowerOn, "PWR ON" },
        { ProjectorCommands.SystemControlVolumeMuteOn, "MUTE ON" },
        { ProjectorCommands.SystemControlVolumeMuteOff, "MUTE OFF" },
        { ProjectorCommands.SystemControlVolumeMuteQuery, "MUTE?" },
        { ProjectorCommands.SystemControlVolumeUp, "VOL INC" },
        { ProjectorCommands.SystemControlVolumeDown, "VOL DEC" },
        { ProjectorCommands.SystemControlVolumeQuery, "VOL?" },
        { ProjectorCommands.SystemControlSourceHDMI1, "SOURCE 30" },
        { ProjectorCommands.SystemControlSourceHDMI2, "SOURCE A0" },
        { ProjectorCommands.SystemControlSourceHDMI3, "SOURCE C0" },
        { ProjectorCommands.SystemControlSourceHDMILAN, "SOURCE 53" },
        { ProjectorCommands.SystemControlSourceQuery, "SOURCE?" },
        { ProjectorCommands.SystemControlSourceListQuery, "SOURCELISTA?" },
        { ProjectorCommands.SystemInformationProjectorNameQuery, "NWPNAME?" },
        { ProjectorCommands.SystemInformationSerialNumberQuery, "SNO?" },
        { ProjectorCommands.SystemInformationErrorQuery, "ERR?" },
        { ProjectorCommands.SystemInformationLampHoursQuery, "LAMP?" },
        { ProjectorCommands.SystemInformationOperationalTimeQuery, "ONTIME?" },
        { ProjectorCommands.SystemInformationSignalStatusQuery, "SIGNAL?" },
        { ProjectorCommands.ImageControlNaturalColorMode, "CMODE 07" },
        { ProjectorCommands.ImageControlImageReverseHorizontalOn, "HREVERSE ON" },
        { ProjectorCommands.ImageControlImageReverseHorizontalOff, "HREVERSE OFF" },
        { ProjectorCommands.ImageControlImageReverseHorizontalQuery, "HREVERSE?" },
        { ProjectorCommands.ImageControlImageReverseVerticalOn, "VREVERSE ON" },
        { ProjectorCommands.ImageControlImageReverseVerticalOff, "VREVERSE OFF" },
        { ProjectorCommands.ImageControlImageReverseVerticalQuery, "VREVERSE?" },
        
        // --- same for CONTRAST, DENSITY, TINT
        { ProjectorCommands.ImageControlBrightnessUp, "BRIGHT INC" },
        { ProjectorCommands.ImageControlBrightnessDown, "BRIGHT DEC" },
        { ProjectorCommands.ImageControlBrightnessQuery, "BRIGHT?" },
        
        { ProjectorCommands.ImageControlStatusLEDIlluminationOn, "ILLUM 01" },
        { ProjectorCommands.ImageControlStatusLEDIlluminationOff, "ILLUM 00" },
        { ProjectorCommands.ImageControlStatusLEDIlluminationQuery, "ILLUM?" },
        { ProjectorCommands.KeyControlPower, "KEY 01" },
        { ProjectorCommands.KeyControlMenu, "KEY 03" },
        { ProjectorCommands.KeyControlUp, "KEY 35" },
        { ProjectorCommands.KeyControlDown, "KEY 36" },
        { ProjectorCommands.KeyControlLeft, "KEY 37" },
        { ProjectorCommands.KeyControlRight, "KEY 38" },
        { ProjectorCommands.KeyControlEnter, "KEY 16" },
        { ProjectorCommands.KeyControlHome, "KEY 04" },
        { ProjectorCommands.KeyControlVolumeUp, "KEY 56" },
        { ProjectorCommands.KeyControlVolumeDown, "KEY 57" },
        { ProjectorCommands.KeyControlAVMuteBlank, "KEY 3E" },
        { ProjectorCommands.KeyControlKeysTone, "KEY C8" },
        { ProjectorCommands.KeyControlHDMILink, "KEY 8E" },
        { ProjectorCommands.KeyControlPlay, "KEY D1" },
        { ProjectorCommands.KeyControlStop, "KEY D2" },
        { ProjectorCommands.KeyControlPause, "KEY D3" },
        { ProjectorCommands.KeyControlRewind, "KEY D4" },
        { ProjectorCommands.KeyControlFastForward, "KEY D5" },
        { ProjectorCommands.KeyControlBackward, "KEY D6" },
        { ProjectorCommands.KeyControlForward, "KEY D7" },
        { ProjectorCommands.KeyControlMute, "KEY D8" },
        { ProjectorCommands.KeyControlLinkMenu, "KEY D9" },
        
        // --- same for IR
        { ProjectorCommands.IRHome, "KEY 30" },
        { ProjectorCommands.IRESC, "KEY 3D" },
        { ProjectorCommands.IREnter, "KEY 49" },
        { ProjectorCommands.IRPointerUp, "KEY 58" },
        { ProjectorCommands.IRPointerDown, "KEY 59" },
        { ProjectorCommands.IRPointerLeft, "KEY 5A" },
        { ProjectorCommands.IRPointerRight, "KEY 5B" },
    };
    
    private enum KeyCommands : byte
    {
        Power = 0x01
    }

    public enum PowerStatus
    {
        StandbyNetworkOff = 0,
        LampOn = 1,
        Warmup = 2,
        CoolDown = 3,
        StandbyNetworkOn = 4,
        AbnormalityStandby = 5,
        Unknown = 6
    }
    // public static byte[] PowerStatusToBytes(PowerStatus status) => Encoding.ASCII.GetBytes(PowerStatusToString(status));
    // private static string PowerStatusToString(PowerStatus status) => $"PWR=0{(int)status}\r:";
    public static PowerStatus? StringToPowerStatus(string response)
    {
        if (string.IsNullOrWhiteSpace(response) || !response.StartsWith("PWR=0") || !response.EndsWith("\r:"))
        {
            return null;
        }

        var statusValue = response.Substring(5, 1);
        if (int.TryParse(statusValue, out var intValue))
        {
            return (PowerStatus)intValue;
        }

        throw new ArgumentException("Unable to parse the power status value.", nameof(response));
    }
    
    public static readonly byte[] ErrorResponse = Encoding.ASCII.GetBytes("Err\r:");
    public static readonly string SuccessfulCommandResponse = ":";
}