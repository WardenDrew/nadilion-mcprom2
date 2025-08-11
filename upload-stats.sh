#!/bin/bash

# Called by crontab every minute

export $(grep -v '^#' /srv/games/mc-prom2/.stats.env | xargs -d '\n')
dotnet /srv/games/mc-prom2/stats-tool/bin/release/net9.0/Nadilion.McProm2StatsTool.dll
unset $(grep -v '^#' /srv/games/mc-prom2/.stats.env | sed -E 's/(.*)=.*/\1/' | xargs)