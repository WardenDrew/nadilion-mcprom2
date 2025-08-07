#!/bin/bash
echo "Starting server and waiting for healthy status with a 120 second timeout";
docker compose -f /srv/games/mc-prom2/compose.yml up --no-recreate --detach
