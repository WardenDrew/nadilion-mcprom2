echo "Connecting via RCON inside the docker container";
echo "Use the command 'exit' to close the RCON connection without stopping the server";
docker exec -i mc-prom2 rcon-cli
