using Nadilion.McProm2StatsTool;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;

const string INFLUX_URI = @"https://db.stats.nadilion.com";
const string INFLUX_ORG = "nadilion";
const string INFLUX_BUCKET = "mcprom2";
string? influxDbToken = Environment.GetEnvironmentVariable("INFLUX_TOKEN");

if (String.IsNullOrWhiteSpace(influxDbToken)) {
    Console.WriteLine("FAILURE: Missing INFLUX_TOKEN from Environment");
    return;
}

using var influx = new InfluxDBClient(INFLUX_URI, influxDbToken);
var influxWrite = influx.GetWriteApiAsync();

const string CONTAINER = "mc-prom2";
const string PLAYER_LIST_PREFIX = "players online: ";
const string SPARK_INDICATOR = "[⚡]";
const int SPARK_TPS_REPORT_LINES = 9;

// STORAGE

string worldSizeStr = Command.Run("du", "-ms /srv/games/mc-prom2/world").Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)[0];
if (!decimal.TryParse(worldSizeStr, out decimal worldSize))
{
    Console.WriteLine("FAILURE: World Size was not a number!");
    return;
}
string backupsSizeStr = Command.Run("du", "-ms /srv/games/mc-prom2/backups").Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)[0];
if (!decimal.TryParse(backupsSizeStr, out decimal backupsSize))
{
    Console.WriteLine("FAILURE: Backups Size was not a number!");
    return;
}

await influxWrite.WritePointAsync(bucket: INFLUX_BUCKET, org: INFLUX_ORG, point: PointData
    .Measurement("storage")
    .Field("world_size_mb", worldSize)
    .Field("backups_size_mb", backupsSize)
    .Timestamp(DateTime.UtcNow, WritePrecision.Ms));

Console.WriteLine("Wrote Storage Data");

// COMPUTE

List<string> stats = Docker.Stats(CONTAINER).Split(Environment.NewLine)[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();

if (!decimal.TryParse(stats[2].Trim('%'), out decimal cpuCorePct))
{
    Console.WriteLine("FAILURE: CPU Core Percent was not a number!");
    return;
}

if (!decimal.TryParse(stats[6].Trim('%'), out decimal memoryPct))
{
    Console.WriteLine("FAILURE: Memory Percent was not a number!");
    return;
}

await influxWrite.WritePointAsync(bucket: INFLUX_BUCKET, org: INFLUX_ORG, point: PointData
    .Measurement("compute")
    .Field("cpu_core_pct", cpuCorePct)
    .Field("memory_pct", memoryPct)
    .Timestamp(DateTime.UtcNow, WritePrecision.Ms));

Console.WriteLine("Wrote Compute Data");

// ONLINE PLAYERS

string playersRaw = Docker.Exec(CONTAINER, "rcon-cli list");
int ixofPlayersPrefix = playersRaw.IndexOf(PLAYER_LIST_PREFIX);
List<string> playersOnline = new();
if (ixofPlayersPrefix != -1)
{
    playersOnline = playersRaw.Substring(ixofPlayersPrefix + PLAYER_LIST_PREFIX.Length).Trim().Split(", ").ToList();
}

await influxWrite.WritePointAsync(bucket: INFLUX_BUCKET, org: INFLUX_ORG, point: PointData
    .Measurement("players")
    .Field("count", playersOnline.Count)
    .Timestamp(DateTime.UtcNow, WritePrecision.Ms));

foreach(string player in playersOnline) {
    if (String.IsNullOrWhiteSpace(player)) continue;
    
    await influxWrite.WritePointAsync(bucket: INFLUX_BUCKET, org: INFLUX_ORG, point: PointData
        .Measurement("players_online")
        .Tag("name", player)
        .Field("online", 1)
        .Timestamp(DateTime.UtcNow, WritePrecision.Ms));
}
Console.WriteLine("Wrote Players Data");

// TICK PERFORMANCE

_ = Docker.Exec(CONTAINER, "rcon-cli spark tps");
await Task.Delay(TimeSpan.FromMilliseconds(100));
string tpsRaw = Docker.Logs(CONTAINER, 20);

List<string> tpsRawLines = tpsRaw.Split(Environment.NewLine).ToList();
tpsRawLines.Reverse();
List<string> tpsReport = new();

int foundLines = 0;
foreach(string line in tpsRawLines)
{
    if (!line.Contains(SPARK_INDICATOR)) continue;
    string reportLine = line.Substring(line.IndexOf(SPARK_INDICATOR) + SPARK_INDICATOR.Length).Trim();
    tpsReport.Add(reportLine);
    foundLines++;

    if (foundLines >= SPARK_TPS_REPORT_LINES) break;
}

if (foundLines < SPARK_TPS_REPORT_LINES)
{
    Console.WriteLine("FAILED GENERATING TICK REPORT: Not enough report lines found");
    return;
}

tpsReport.Reverse();

string tpsLine = tpsReport[1];
string msptLine = tpsReport[4];

List<string> tpsEntries = tpsLine.Split(", ").ToList();
tpsEntries = tpsEntries.Select(x => x.Trim('*')).ToList();

List<string> msptSections = msptLine.Split("; ").ToList();
List<string> mspt10sEntries = msptSections[0].Split('/').ToList();
List<string> mspt1mEntries = msptSections[1].Split('/').ToList();

if (!decimal.TryParse(tpsEntries[0], out decimal tps5s))
{
    Console.WriteLine("FAILURE: TPS 5S was not a number!");
    return;
}

if (!decimal.TryParse(tpsEntries[1], out decimal tps10s))
{
    Console.WriteLine("FAILURE: TPS 10S was not a number!");
    return;
}

if (!decimal.TryParse(tpsEntries[2], out decimal tps1m))
{
    Console.WriteLine("FAILURE: TPS 1M was not a number!");
    return;
}

if (!decimal.TryParse(tpsEntries[3], out decimal tps5m))
{
    Console.WriteLine("FAILURE: TPS 5M was not a number!");
    return;
}

if (!decimal.TryParse(tpsEntries[4], out decimal tps10m))
{
    Console.WriteLine("FAILURE: TPS 10M was not a number!");
    return;
}

if (!decimal.TryParse(mspt10sEntries[0], out decimal mspt10smin))
{
    Console.WriteLine("FAILURE: MSPT 10S MIN was not a number!");
    return;
}

if (!decimal.TryParse(mspt10sEntries[1], out decimal mspt10smed))
{
    Console.WriteLine("FAILURE: MSPT 10S MED was not a number!");
    return;
}

if (!decimal.TryParse(mspt10sEntries[2], out decimal mspt10s95pct))
{
    Console.WriteLine("FAILURE: MSPT 10S 95PCT was not a number!");
    return;
}

if (!decimal.TryParse(mspt10sEntries[3], out decimal mspt10smax))
{
    Console.WriteLine("FAILURE: MSPT 10S MAX was not a number!");
    return;
}

if (!decimal.TryParse(mspt1mEntries[0], out decimal mspt1mmin))
{
    Console.WriteLine("FAILURE: MSPT 1M MIN was not a number!");
    return;
}

if (!decimal.TryParse(mspt1mEntries[1], out decimal mspt1mmed))
{
    Console.WriteLine("FAILURE: MSPT 1M MED was not a number!");
    return;
}

if (!decimal.TryParse(mspt1mEntries[2], out decimal mspt1m95pct))
{
    Console.WriteLine("FAILURE: MSPT 1M 95PCT was not a number!");
    return;
}

if (!decimal.TryParse(mspt1mEntries[3], out decimal mspt1mmax))
{
    Console.WriteLine("FAILURE: MSPT 1M MAX was not a number!");
    return;
}

await influxWrite.WritePointAsync(bucket: INFLUX_BUCKET, org: INFLUX_ORG, point: PointData
    .Measurement("tick_performance")
    .Field("tps_5s", tps5s)
    .Field("tps_10s", tps10s)
    .Field("tps_1m", tps1m)
    .Field("tps_5m", tps5m)
    .Field("tps_10m", tps10m)
    .Field("mspt_10s_min", mspt10smin)
    .Field("mspt_10s_med", mspt10smed)
    .Field("mspt_10s_95pct", mspt10s95pct)
    .Field("mspt_10s_max", mspt10smax)
    .Field("mspt_1m_min", mspt1mmin)
    .Field("mspt_1m_med", mspt1mmed)
    .Field("mspt_1m_95pct", mspt1m95pct)
    .Field("mspt_1m_max", mspt1mmax)
    .Timestamp(DateTime.UtcNow, WritePrecision.Ms));

Console.WriteLine("Wrote Tick Performance Data");