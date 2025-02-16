using ProjectController.QueueManagement;

namespace ProjectController.Controllers.ADB;

public class AndroidTVCommand : ICommand<KeyCodes>
{
    public KeyCodes CommandType { get; }
    public Func<Task<string>> SendCommand { get; }
    public Func<string, Task> Callback { get; }
    public bool IsLongPress { get; }
    
    public AndroidTVCommand(KeyCodes commandType, Func<ICommand<KeyCodes>, Task<string>> sendCommand, Func<ICommand<KeyCodes>, string, Task> callback, bool isLongPress)
    {
        CommandType = commandType;
        SendCommand = () => sendCommand.Invoke(this);
        Callback = response => callback.Invoke(this, response);
        IsLongPress = isLongPress;
    }
}