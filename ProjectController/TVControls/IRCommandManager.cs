using System.Diagnostics;

namespace ProjectController.TVControls;

public enum IRCommands
{ 
    BTN_DPAD_UP              ,
    BTN_DPAD_DOWN            ,
    KEY_OK                   ,
    BTN_DPAD_RIGHT           ,
    BTN_DPAD_LEFT            ,
    KEY_NUMERIC_0            ,
    KEY_NUMERIC_1            ,
    KEY_NUMERIC_2            ,
    KEY_NUMERIC_3            ,
    KEY_NUMERIC_4            ,
    KEY_NUMERIC_5            ,
    KEY_NUMERIC_6            ,
    KEY_NUMERIC_7            ,
    KEY_NUMERIC_8            ,
    KEY_NUMERIC_9            ,
    KEY_PAGEUP               ,
    KEY_PAGEDOWN             ,
    KEY_MUTE                 ,
    KEY_SUBTITLE             ,
    KEY_POWER                ,
    KEY_AUDIO                ,
    KEY_EPG                  ,
    KEY_FILE                 ,
    KEY_MENU                 ,
    KEY_EXIT                 ,
    KEY_FAVORITES            ,
    KEY_TV                   ,
    KEY_BACK                 ,
    KEY_INFO                 ,
    KEY_FORWARD              ,
    KEY_REWIND               ,
    KEY_PREVIOUS             ,
    KEY_NEXT                 ,
    KEY_PLAY                 ,
    KEY_PAUSE                ,
    KEY_STOP                 ,
    KEY_RECORD               ,
    KEY_RED                  ,
    KEY_GREEN                ,
    KEY_YELLOW               ,
    KEY_BLUE                 ,
}

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