using Microsoft.AspNetCore.Connections;

namespace ProjectController.QueueManagement;

public interface ICommand<TCommand> where TCommand : Enum
{
    TCommand CommandType { get; }
    Func<ICommand<TCommand>, Task<string>> SendCommand { get; }
    Func<ICommand<TCommand>, string, Task> Callback { get; }
}

public class CommandRunner<TCommand, TCommandType>
    where TCommandType : Enum
    where TCommand : ICommand<TCommandType>
{
    private readonly ILogger<CommandRunner<TCommand, TCommandType>> logger;
    private readonly Queue<TCommand> commandQueue = new();

    private CancellationTokenSource? runningCancellationTokenSource;
    private Task commandQueueTask = Task.CompletedTask;
    private readonly SemaphoreSlim queueAccessSemaphore = new(1, 1);
    private TaskCompletionSource commandAvailableTcs = new();

    public event Func<CancellationToken, Task>? PreCommandEvent;

    public CommandRunner(ILogger<CommandRunner<TCommand, TCommandType>> logger)
    {
        this.logger = logger;
    }
    
    public async Task Start()
    {
        if (runningCancellationTokenSource == null || runningCancellationTokenSource.IsCancellationRequested)
        {
            runningCancellationTokenSource = new CancellationTokenSource();
            await (PreCommandEvent?.Invoke(runningCancellationTokenSource.Token) ?? Task.CompletedTask);
            commandQueueTask = RunCommandQueue(runningCancellationTokenSource.Token);
        }
    }
    
    public async Task Stop()
    {
        runningCancellationTokenSource?.Cancel();
        try
        {
            await commandQueueTask;
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Command queue stopped.");
        }
    }
    
    private async Task RunCommandQueue(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var commandAvailableTask = commandAvailableTcs.Task;
                var cancellationTask = Task.Delay(Timeout.Infinite, token);
                var result = await Task.WhenAny(commandAvailableTask, cancellationTask);
                if (result == cancellationTask)
                {
                    token.ThrowIfCancellationRequested();
                }
                else if (result == commandAvailableTask && commandAvailableTask.IsCanceled)
                {
                    continue;
                }
                
                try
                {
                    await (PreCommandEvent?.Invoke(token) ?? Task.CompletedTask);
                    logger.LogDebug("Try to access queue...");
                    await queueAccessSemaphore.WaitAsync(token);
                    logger.LogDebug("Accessed queue...");
                    bool dequeued;
                    TCommand? command;
                    
                    try
                    {
                        logger.LogDebug("Checking for command to dequeue...");
                        dequeued = commandQueue.TryDequeue(out command);
                        logger.LogDebug("Remaining queue: {commands}", string.Join(", ", commandQueue.Select(x => x.CommandType.ToString())));
                        if (commandQueue.Count == 0)
                        {
                            commandAvailableTcs = new();
                        }
                    }
                    finally
                    {
                        logger.LogDebug("Finished with queue...");
                        queueAccessSemaphore.Release();
                    }

                    if (dequeued)
                    {
                        if (command == null)
                            throw new InvalidOperationException("Command was null.");
                        
                        var response = await command.SendCommand.Invoke(command);
                        await command.Callback(command, response);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError("Exception while trying to send a command. Type: {type}, Message: {message}, Stack Trace: {stackTrace}", e.GetType().FullName, e.Message, e.StackTrace);
                }
            }
        }
        catch (OperationCanceledException)
        {
            await ClearQueue(token);
        }
        catch (Exception e)
        {
            logger.LogError($"Generic Exception: {e.Message}");
        }
    }
    
    public async Task EnqueueCommand(TCommand[] commands, bool allowDuplicates = false)
    {
        foreach (var command in commands)
        {
            await queueAccessSemaphore.WaitAsync();
            try
            {
                if (allowDuplicates)
                {
                    commandQueue.Enqueue(command);
                    logger.LogInformation($"Command enqueued: {command.CommandType}");
                    commandAvailableTcs.TrySetResult();
                }
                else
                {
                    if (commandQueue.ToList().All(x => !x.CommandType.Equals(command.CommandType)))
                    {
                        commandQueue.Enqueue(command);
                        logger.LogInformation($"Command enqueued: {command.CommandType}");
                        commandAvailableTcs.TrySetResult();
                    }
                    else
                    {
                        logger.LogTrace($"Command already in queue: {command.CommandType}");
                    }
                }
            }
            finally
            {
                queueAccessSemaphore.Release();
            }
        }
    }

    private async Task ClearQueue(CancellationToken token)
    {
        await queueAccessSemaphore.WaitAsync(token);
        try
        {
            commandQueue.Clear();
            commandAvailableTcs.TrySetCanceled();
            commandAvailableTcs = new();
        }
        finally
        {
            queueAccessSemaphore.Release();
        }
        logger.LogDebug("Canceled all commands.");
    }
}