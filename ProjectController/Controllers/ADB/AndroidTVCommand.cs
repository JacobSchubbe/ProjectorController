using ProjectController.QueueManagement;

namespace ProjectController.Controllers.ADB;

public class AndroidTVCommand : ICommand<KeyCodes>
{
    public KeyCodes CommandType { get; }
    public Func<Task<string>> SendCommand { get; }
    public Func<string, Task> Callback { get; }
    public string AdditionalParameter { get; }
    
    public AndroidTVCommand(KeyCodes commandType, Func<ICommand<KeyCodes>, Task<string>> sendCommand, Func<ICommand<KeyCodes>, string, Task> callback, string additionalParameter)
    {
        CommandType = commandType;
        SendCommand = () => sendCommand.Invoke(this);
        Callback = response => callback.Invoke(this, response);
        AdditionalParameter = additionalParameter;
    }
}