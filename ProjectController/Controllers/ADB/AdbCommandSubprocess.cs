using System.Diagnostics;

namespace ProjectController.Controllers.ADB;

public class AdbCommandSubprocess
{
    private Process? process;
    private bool blocking;
        
    public AdbCommandSubprocess(Process? process, bool blocking)
    {
        this.blocking = blocking;
        this.process = process;
    }

    public static implicit operator Process?(AdbCommandSubprocess? command)
    {
        if (command is { blocking: false })
        {
            // For non-blocking calls, start reading asynchronous output
            command.process?.BeginOutputReadLine();
            command.process?.BeginErrorReadLine();
            return command.process; // Return the running Process object
        }

        if (command is { blocking: true })
        {
            // Wait for the process to complete and capture the output
            var result = command.process?.StandardOutput.ReadToEnd();
            command.process?.WaitForExit();
            return null;
        }

        return null;
    }
        
    public static implicit operator string(AdbCommandSubprocess? command)
    {
        if (command is { blocking: false })
        {
            // For non-blocking calls, start reading asynchronous output
            command.process?.BeginOutputReadLine();
            command.process?.BeginErrorReadLine();
            return string.Empty; // Return the running Process object
        }

        if (command is { blocking: true })
        {
            // Wait for the process to complete and capture the output
            var result = command.process?.StandardOutput.ReadToEnd();
            command.process?.WaitForExit();
            return result?.Trim() ?? string.Empty;
        }
            
        return string.Empty;
    }
}
