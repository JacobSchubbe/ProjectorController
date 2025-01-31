using Microsoft.AspNetCore.Connections;

namespace ProjectController.QueueManagement;

public class CommandRunner<TCommands> where TCommands : Enum
{
    private readonly ILogger<CommandRunner<TCommands>> logger;
    private readonly Queue<(TCommands command, Func<TCommands, string, Task> callback)> commandQueue = new();

    private CancellationTokenSource? runningCancellationTokenSource;
    private Task commandQueueTask = Task.CompletedTask;
    private Func<TCommands, Task<string>>? sendCommand;
    private readonly SemaphoreSlim queueAccessSemaphore = new(1, 1);
    public event Func<CancellationToken, Task>? PreCommandEvent;

    public CommandRunner(ILogger<CommandRunner<TCommands>> logger)
    {
        this.logger = logger;
    }
    
    public async Task Start(Func<TCommands, Task<string>>? sendCommand)
    {
        this.sendCommand = sendCommand;
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
        finally
        {
            sendCommand = null;
        }
    }
    
    private async Task RunCommandQueue(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
                
                if (commandQueue.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500), token);
                    continue;
                }
                
                try
                {
                    await (PreCommandEvent?.Invoke(token) ?? Task.CompletedTask);
                    logger.LogDebug("Try to access queue...");
                    await queueAccessSemaphore.WaitAsync(token);
                    logger.LogDebug("Accessed queue...");
                    (TCommands command, Func<TCommands, string, Task> callback) commandKvp;
                    bool dequeued;
                    
                    try
                    {
                        logger.LogDebug("Checking for command to dequeue...");
                        dequeued = commandQueue.TryDequeue(out commandKvp);
                        logger.LogDebug("Remaining queue: {commands}", string.Join(", ", commandQueue.Select(x => x.command.ToString())));
                    }
                    finally
                    {
                        logger.LogDebug("Finished with queue...");
                        queueAccessSemaphore.Release();
                    }

                    if (dequeued)
                    {
                        var response = sendCommand != null ? await sendCommand.Invoke(commandKvp.command) : string.Empty;
                        await commandKvp.callback(commandKvp.command, response);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError("Exception while trying to send a command. Type: {type}, Message: {message}", e.GetType().FullName, e.Message);
                }
            
                await Task.Delay(TimeSpan.FromMilliseconds(100), token);
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
    
    public async Task EnqueueCommand(TCommands[] commands, Func<TCommands, string, Task> callback, bool allowDuplicates = false)
    {
        foreach (var command in commands)
        {
            await queueAccessSemaphore.WaitAsync();
            try
            {
                if (allowDuplicates)
                {
                    commandQueue.Enqueue((command, callback));
                    logger.LogInformation($"Command enqueued: {command}");
                }
                else
                {
                    if (commandQueue.ToList().All(x => !x.command.Equals(command)))
                    {
                        commandQueue.Enqueue((command, callback));
                        logger.LogInformation($"Command enqueued: {command}");
                    }
                    else
                    {
                        logger.LogTrace($"Command already in queue: {command}");
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
        }
        finally
        {
            queueAccessSemaphore.Release();
        }
        logger.LogDebug("Canceled all commands.");
    }
}