using System.Text;

namespace ProjectController.TCPCommunication;

public static class TCPConsts
{
    // =========== SYSTEM CONTROL ========================

    public enum SystemControl
    {
        StartCommunication,
        PowerQuery,
        PowerOff,
        PowerOn,
        VolumeMuteOn,
        VolumeMuteOff,
        VolumeMuteQuery,
        VolumeUp,
        VolumeDown,
        VolumeQuery,
        SourceHDMI1,
        SourceHDMI2,
        SourceHDMI3,
        SourceHDMILAN,
        SourceListQuery,
    }
    
    public enum SystemInformation
    {
        ProjectorNameQuery,
        SerialNumberQuery,
        ErrorQuery,
        LampHoursQuery,
        OperationalTimeQuery,
        SignalStatusQuery
    }

    public enum ImageControl
    {
        NaturalColorMode,
        ImageReverseHorizontalOn,
        ImageReverseHorizontalOff,
        ImageReverseHorizontalQuery,
        ImageReverseVerticalOn,
        ImageReverseVerticalOff,
        ImageReverseVerticalQuery,
        BrightnessUp,
        BrightnessDown,
        BrightnessQuery,
        StatusLEDIlluminationOn,
        StatusLEDIlluminationOff,
        StatusLEDIlluminationQuery
    }

    public enum KeyControl
    {
        KeyPower,
        KeyMenu,
        KeyUp,
        KeyDown,
        KeyLeft,
        KeyRight,
        KeyEnter,
        KeyHome,
        KeyVolumeUp,
        KeyVolumeDown,
        KeyAVMuteBlank,
        KeyKeysTone,
        KeyHDMILink,
        KeyPlay,
        KeyStop,
        KeyPause,
        KeyRewind,
        KeyFastForward,
        KeyBackward,
        KeyForward,
        KeyMute,
        KeyLinkMenu
    }

    public static readonly Dictionary<SystemControl, string> SystemControlDictionary = new()
    {
        { SystemControl.StartCommunication, "ESC/VP.net\x10\x03\x00\x00\x00\x00" },
        { SystemControl.PowerQuery, "PWR?" },
        { SystemControl.PowerOff, "PWR OFF" },
        { SystemControl.PowerOn, "PWR ON" },
        { SystemControl.VolumeMuteOn, "MUTE ON" },
        { SystemControl.VolumeMuteOff, "MUTE OFF" },
        { SystemControl.VolumeMuteQuery, "MUTE?" },
        { SystemControl.VolumeUp, "VOL INC" },
        { SystemControl.VolumeDown, "VOL DEC" },
        { SystemControl.VolumeQuery, "VOL?" },
        { SystemControl.SourceHDMI1, "SOURCE 30" },
        { SystemControl.SourceHDMI2, "SOURCE A0" },
        { SystemControl.SourceHDMI3, "SOURCE C0" },
        { SystemControl.SourceHDMILAN, "SOURCE 53" },
        { SystemControl.SourceListQuery, "SOURCELIST?" },
    };
    
    public static readonly Dictionary<SystemInformation, string> SystemInfoCommands = new Dictionary<SystemInformation, string>
    {
        { SystemInformation.ProjectorNameQuery, "NWPNAME?" },
        { SystemInformation.SerialNumberQuery, "SNO?" },
        { SystemInformation.ErrorQuery, "ERR?" },
        { SystemInformation.LampHoursQuery, "LAMP?" },
        { SystemInformation.OperationalTimeQuery, "ONTIME?" },
        { SystemInformation.SignalStatusQuery, "SIGNAL?" }
    };

    public static readonly Dictionary<ImageControl, string> ImageControlCommands = new Dictionary<ImageControl, string>
    {
        { ImageControl.NaturalColorMode, "CMODE 07" },
        { ImageControl.ImageReverseHorizontalOn, "HREVERSE ON" },
        { ImageControl.ImageReverseHorizontalOff, "HREVERSE OFF" },
        { ImageControl.ImageReverseHorizontalQuery, "HREVERSE?" },
        { ImageControl.ImageReverseVerticalOn, "VREVERSE ON" },
        { ImageControl.ImageReverseVerticalOff, "VREVERSE OFF" },
        { ImageControl.ImageReverseVerticalQuery, "VREVERSE?" },
        
        // --- same for CONTRAST, DENSITY, TINT
        { ImageControl.BrightnessUp, "BRIGHT INC" },
        { ImageControl.BrightnessDown, "BRIGHT DEC" },
        { ImageControl.BrightnessQuery, "BRIGHT?" },
        
        { ImageControl.StatusLEDIlluminationOn, "ILLUM 01" },
        { ImageControl.StatusLEDIlluminationOff, "ILLUM 00" },
        { ImageControl.StatusLEDIlluminationQuery, "ILLUM?" }
    };

    public static readonly Dictionary<KeyControl, string> KeyControlCommands = new Dictionary<KeyControl, string>
    {
        { KeyControl.KeyPower, "KEY 01" },
        { KeyControl.KeyMenu, "KEY 03" },
        { KeyControl.KeyUp, "KEY 35" },
        { KeyControl.KeyDown, "KEY 36" },
        { KeyControl.KeyLeft, "KEY 37" },
        { KeyControl.KeyRight, "KEY 38" },
        { KeyControl.KeyEnter, "KEY 16" },
        { KeyControl.KeyHome, "KEY 04" },
        { KeyControl.KeyVolumeUp, "KEY 56" },
        { KeyControl.KeyVolumeDown, "KEY 57" },
        { KeyControl.KeyAVMuteBlank, "KEY 3E" },
        { KeyControl.KeyKeysTone, "KEY C8" },
        { KeyControl.KeyHDMILink, "KEY 8E" },
        { KeyControl.KeyPlay, "KEY D1" },
        { KeyControl.KeyStop, "KEY D2" },
        { KeyControl.KeyPause, "KEY D3" },
        { KeyControl.KeyRewind, "KEY D4" },
        { KeyControl.KeyFastForward, "KEY D5" },
        { KeyControl.KeyBackward, "KEY D6" },
        { KeyControl.KeyForward, "KEY D7" },
        { KeyControl.KeyMute, "KEY D8" },
        { KeyControl.KeyLinkMenu, "KEY D9" }
    };
    
    private enum KeyCommands : byte
    {
        Power = 0x01
    }

    private enum PowerStatus
    {
        StandbyNetworkOff = 0,
        LampOn = 1,
        Warmup = 2,
        CoolDown = 3,
        StandbyNetworkOn = 4,
        AbnormalityStandby = 5,
    }
    private static byte[] PowerStatusToBytes(PowerStatus status) => Encoding.ASCII.GetBytes($"PWR=0{(int)status}\r:");
    private static readonly byte[] ErrorResponse = Encoding.ASCII.GetBytes("Err\r:");

}