#!/bin/bash
echo "Notifying Server: 'Server Stopping NOW'";
docker exec mc-prom2 rcon-cli "title @a title \"Server Stopping NOW\""
echo "Waiting 5 seconds";
sleep 5
echo "Stopping Server";
docker compose -f /srv/games/mc-prom2/compose.yml down
