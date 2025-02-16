namespace ProjectController.QueueManagement;

public interface ICommand<TCommand> where TCommand : Enum
{
    TCommand CommandType { get; }
    Func<Task<string>> SendCommand { get; }
    Func<string, Task> Callback { get; }
}