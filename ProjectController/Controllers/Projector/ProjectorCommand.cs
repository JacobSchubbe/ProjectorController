using ProjectController.QueueManagement;
using static ProjectController.Controllers.Projector.ProjectorConstants;

namespace ProjectController.Controllers.Projector;

public sealed class ProjectorCommand : ICommand<ProjectorCommandsEnum>
{
    public ProjectorCommandsEnum CommandType { get; }
    public Func<Task<string>> SendCommand { get; }
    public Func<string, Task> Callback { get; }
    
    public ProjectorCommand(ProjectorCommandsEnum commandType, Func<ICommand<ProjectorCommandsEnum>, Task<string>> sendCommand, Func<ICommand<ProjectorCommandsEnum>, string, Task> callback)
    {
        CommandType = commandType;
        SendCommand = () => sendCommand.Invoke(this);
        Callback = response => callback.Invoke(this, response);
    }
}