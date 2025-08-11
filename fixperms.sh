#!/bin/bash
cd "$(dirname "${BASH_SOURCE[0]}")"
sudo chown -R minecraft:docker minecraft
sudo chown -R minecraft:docker world
sudo chmod -R 770 minecraft
sudo chmod -R 770 world
