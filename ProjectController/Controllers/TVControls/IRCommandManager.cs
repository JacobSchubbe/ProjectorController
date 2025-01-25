using System.Diagnostics;

namespace ProjectController.Controllers.TVControls;

public static class IRCommandManager
{
    public static void SendIRCommand(IRCommands irCommand)
    {
        string command = "irsend";
        string arguments = $"SEND_ONCE DenverCableBox {irCommand.ToString()}";

        try
        {
            // Create a ProcessStartInfo object
            var processStartInfo = new ProcessStartInfo
            {
                FileName = command, // Command to execute
                Arguments = arguments, // Arguments to pass
                RedirectStandardOutput = true, // Capture standard output
                RedirectStandardError = true, // Capture standard error
                UseShellExecute = false, // Do not use shell
                CreateNoWindow = true // Run silently (no window)
            };

            // Start the process
            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            // Read the output and error streams
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            // Log the results
            Console.WriteLine($"Output: {output}");
            if (!string.IsNullOrEmpty(error))
            {
                Console.Error.WriteLine($"Error: {error}");
            }

            // Check if the process exited successfully
            if (process.ExitCode == 0)
            {
                Console.WriteLine("Command executed successfully!");
            }
            else
            {
                Console.Error.WriteLine($"Command failed with exit code {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Exception occurred: {ex.Message}");
        }
    }
}