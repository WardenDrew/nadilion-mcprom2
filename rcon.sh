#!/bin/bash
cd "$(dirname "${BASH_SOURCE[0]}")"

docker exec mc-prom2 rcon-cli "$@"
