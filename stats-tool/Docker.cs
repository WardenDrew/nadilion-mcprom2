namespace Nadilion.McProm2StatsTool;

internal static class Docker
{
    private static string dockerCommand(string arguments)
        => Command.Run("docker", arguments);
    
    public static string Exec(string container, string command)
        => dockerCommand($"exec {container} {command}");

    public static string Logs(string container, int limit)
        => dockerCommand($"logs -n {limit} {container}");

    public static string Stats(string container)
        => dockerCommand($"container stats {container} --no-stream");
}