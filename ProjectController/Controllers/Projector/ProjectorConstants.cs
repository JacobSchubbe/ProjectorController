using System.Text;

namespace ProjectController.Controllers.Projector;

public static class ProjectorConstants
{
    public const string ProjectorHost = "192.168.0.150";
    public const int ProjectorPort = 3629;
    
    public enum ProjectorCommandsEnum
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

    public static readonly Dictionary<ProjectorCommandsEnum, string> ProjectorCommandsDictionary = new()
    {
        { ProjectorCommandsEnum.SystemControlStartCommunication, "ESC/VP.net\x10\x03\x00\x00\x00\x00" },
        { ProjectorCommandsEnum.SystemControlPowerQuery, "PWR?" },
        { ProjectorCommandsEnum.SystemControlPowerOff, "PWR OFF" },
        { ProjectorCommandsEnum.SystemControlPowerOn, "PWR ON" },
        { ProjectorCommandsEnum.SystemControlVolumeMuteOn, "MUTE ON" },
        { ProjectorCommandsEnum.SystemControlVolumeMuteOff, "MUTE OFF" },
        { ProjectorCommandsEnum.SystemControlVolumeMuteQuery, "MUTE?" },
        { ProjectorCommandsEnum.SystemControlVolumeUp, "VOL INC" },
        { ProjectorCommandsEnum.SystemControlVolumeDown, "VOL DEC" },
        { ProjectorCommandsEnum.SystemControlVolumeQuery, "VOL?" },
        { ProjectorCommandsEnum.SystemControlSourceHDMI1, "SOURCE 30" },
        { ProjectorCommandsEnum.SystemControlSourceHDMI2, "SOURCE A0" },
        { ProjectorCommandsEnum.SystemControlSourceHDMI3, "SOURCE C0" },
        { ProjectorCommandsEnum.SystemControlSourceHDMILAN, "SOURCE 53" },
        { ProjectorCommandsEnum.SystemControlSourceQuery, "SOURCE?" },
        { ProjectorCommandsEnum.SystemControlSourceListQuery, "SOURCELISTA?" },
        { ProjectorCommandsEnum.SystemInformationProjectorNameQuery, "NWPNAME?" },
        { ProjectorCommandsEnum.SystemInformationSerialNumberQuery, "SNO?" },
        { ProjectorCommandsEnum.SystemInformationErrorQuery, "ERR?" },
        { ProjectorCommandsEnum.SystemInformationLampHoursQuery, "LAMP?" },
        { ProjectorCommandsEnum.SystemInformationOperationalTimeQuery, "ONTIME?" },
        { ProjectorCommandsEnum.SystemInformationSignalStatusQuery, "SIGNAL?" },
        { ProjectorCommandsEnum.ImageControlNaturalColorMode, "CMODE 07" },
        { ProjectorCommandsEnum.ImageControlImageReverseHorizontalOn, "HREVERSE ON" },
        { ProjectorCommandsEnum.ImageControlImageReverseHorizontalOff, "HREVERSE OFF" },
        { ProjectorCommandsEnum.ImageControlImageReverseHorizontalQuery, "HREVERSE?" },
        { ProjectorCommandsEnum.ImageControlImageReverseVerticalOn, "VREVERSE ON" },
        { ProjectorCommandsEnum.ImageControlImageReverseVerticalOff, "VREVERSE OFF" },
        { ProjectorCommandsEnum.ImageControlImageReverseVerticalQuery, "VREVERSE?" },
        
        // --- same for CONTRAST, DENSITY, TINT
        { ProjectorCommandsEnum.ImageControlBrightnessUp, "BRIGHT INC" },
        { ProjectorCommandsEnum.ImageControlBrightnessDown, "BRIGHT DEC" },
        { ProjectorCommandsEnum.ImageControlBrightnessQuery, "BRIGHT?" },
        
        { ProjectorCommandsEnum.ImageControlStatusLEDIlluminationOn, "ILLUM 01" },
        { ProjectorCommandsEnum.ImageControlStatusLEDIlluminationOff, "ILLUM 00" },
        { ProjectorCommandsEnum.ImageControlStatusLEDIlluminationQuery, "ILLUM?" },
        { ProjectorCommandsEnum.KeyControlPower, "KEY 01" },
        { ProjectorCommandsEnum.KeyControlMenu, "KEY 03" },
        { ProjectorCommandsEnum.KeyControlUp, "KEY 35" },
        { ProjectorCommandsEnum.KeyControlDown, "KEY 36" },
        { ProjectorCommandsEnum.KeyControlLeft, "KEY 37" },
        { ProjectorCommandsEnum.KeyControlRight, "KEY 38" },
        { ProjectorCommandsEnum.KeyControlEnter, "KEY 16" },
        { ProjectorCommandsEnum.KeyControlHome, "KEY 04" },
        { ProjectorCommandsEnum.KeyControlVolumeUp, "KEY 56" },
        { ProjectorCommandsEnum.KeyControlVolumeDown, "KEY 57" },
        { ProjectorCommandsEnum.KeyControlAVMuteBlank, "KEY 3E" },
        { ProjectorCommandsEnum.KeyControlKeysTone, "KEY C8" },
        { ProjectorCommandsEnum.KeyControlHDMILink, "KEY 8E" },
        { ProjectorCommandsEnum.KeyControlPlay, "KEY D1" },
        { ProjectorCommandsEnum.KeyControlStop, "KEY D2" },
        { ProjectorCommandsEnum.KeyControlPause, "KEY D3" },
        { ProjectorCommandsEnum.KeyControlRewind, "KEY D4" },
        { ProjectorCommandsEnum.KeyControlFastForward, "KEY D5" },
        { ProjectorCommandsEnum.KeyControlBackward, "KEY D6" },
        { ProjectorCommandsEnum.KeyControlForward, "KEY D7" },
        { ProjectorCommandsEnum.KeyControlMute, "KEY D8" },
        { ProjectorCommandsEnum.KeyControlLinkMenu, "KEY D9" },
        
        // --- same for IR
        { ProjectorCommandsEnum.IRHome, "KEY 30" },
        { ProjectorCommandsEnum.IRESC, "KEY 3D" },
        { ProjectorCommandsEnum.IREnter, "KEY 49" },
        { ProjectorCommandsEnum.IRPointerUp, "KEY 58" },
        { ProjectorCommandsEnum.IRPointerDown, "KEY 59" },
        { ProjectorCommandsEnum.IRPointerLeft, "KEY 5A" },
        { ProjectorCommandsEnum.IRPointerRight, "KEY 5B" },
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