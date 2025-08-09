#!/bin/bash
cd "$(dirname "${BASH_SOURCE[0]}")"

source ./stop.sh

rm -f minecraft/logs/*
7z a backups/backup-inprogress.7z world

# Rotate these in the future
rm -f backups/backup-latest.7z
mv backups/backup-inprogress.7z backups/backup-latest.7z

source ./start.sh
