#!/bin/bash
cd "$(dirname "${BASH_SOURCE[0]}")"

source ./stop.sh

rm -f ./minecraft/logs/*

source ./start.sh
