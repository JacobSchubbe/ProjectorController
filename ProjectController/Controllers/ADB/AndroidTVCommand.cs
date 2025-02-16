using ProjectController.QueueManagement;

namespace ProjectController.Controllers.ADB;

public class AndroidTVCommand : ICommand<KeyCodes>
{
    public KeyCodes CommandType { get; }
    public Func<ICommand<KeyCodes>, Task<string>> SendCommand { get; }
    public Func<ICommand<KeyCodes>, string, Task> Callback { get; }
    public bool IsLongPress { get; }
    
    public AndroidTVCommand(KeyCodes commandType, Func<ICommand<KeyCodes>, Task<string>> sendCommand, Func<ICommand<KeyCodes>, string, Task> callback, bool isLongPress = false)
    {
        CommandType = commandType;
        SendCommand = sendCommand;
        Callback = callback;
        IsLongPress = isLongPress;
    }
}