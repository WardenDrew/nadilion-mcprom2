# Nadilion MC Server Configuration

Configuration and management script archive for Prominence 2: Hasturian Era modpack running via Docker Compose using itzg/minecraft-server

world folder is gitignored so save specific server configs are not included (currently defaults, may be specifically included later)

stats-tool is the influxdb metrics recorder for stats.nadilion.com

rcon is not exposed publically, only internally on the host, so presence of rcon-password in server.properties does not pose an issue.
