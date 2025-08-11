namespace Nadilion.McProm2StatsTool;

internal static class Command
{
    public static string Run(string filename, string arguments)
    {
        System.Diagnostics.Process command = new();
        command.StartInfo.UseShellExecute = false;
        command.StartInfo.RedirectStandardOutput = true;
        command.StartInfo.FileName = filename;
        command.StartInfo.Arguments = arguments;

        command.Start();
        string output = command.StandardOutput.ReadToEnd();
        command.WaitForExit();

        return output;
    }
}