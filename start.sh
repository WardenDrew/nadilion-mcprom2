#!/bin/bash
cd "$(dirname "${BASH_SOURCE[0]}")"

echo "Starting server";
docker compose -f /srv/games/mc-prom2/compose.yml up --detach
