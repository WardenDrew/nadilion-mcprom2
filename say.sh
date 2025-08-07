#!/bin/bash
docker exec mc-prom2 rcon-cli "tellraw @a [\"\",{\"text\":\"<\"},{\"text\":\"Server\",\"italic\":true,\"color\":\"light_purple\"},{\"text\":\"> \"},{\"text\":\"$@\",\"color\":\"light_purple\"}]"
