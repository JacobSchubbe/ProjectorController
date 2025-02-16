using ProjectController.QueueManagement;
using static ProjectController.Controllers.Projector.ProjectorConstants;

namespace ProjectController.Controllers.Projector;

public sealed class ProjectorCommand : ICommand<ProjectorCommandsEnum>
{
    public ProjectorCommandsEnum CommandType { get; }
    public Func<ICommand<ProjectorCommandsEnum>, Task<string>> SendCommand { get; }
    public Func<ICommand<ProjectorCommandsEnum>, string, Task> Callback { get; }
    
    public ProjectorCommand(ProjectorCommandsEnum commandType, Func<ICommand<ProjectorCommandsEnum>, Task<string>> sendCommand, Func<ICommand<ProjectorCommandsEnum>, string, Task> callback)
    {
        CommandType = commandType;
        SendCommand = sendCommand;
        Callback = callback;
    }
}