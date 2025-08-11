#!/bin/bash
cd "$(dirname "${BASH_SOURCE[0]}")"

echo "Notifying Server: 'Server Restarting NOW'";
docker exec mc-prom2 rcon-cli "title @a title \"Server Restarting NOW\""
echo "Waiting 5 seconds";
sleep 5
echo "Restarting Server";
docker compose -f /srv/games/mc-prom2/compose.yml down
docker compose -f /srv/games/mc-prom2/compose.yml up --detach
