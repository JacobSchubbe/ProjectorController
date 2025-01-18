#!/bin/bash

# Variables
PI_USER="jacob.schubbe"                   # Raspberry Pi username
PI_HOST="raspberrypi.local"    # Raspberry Pi hostname or IP
PI_LOG_PATH="/home/logs"    # Path to logs on the Raspberry Pi
LOCAL_DEST="./logs"            # Local directory to save logs

# Create local destination directory if it doesn't exist
mkdir -p "$LOCAL_DEST"
TODAY=$(date +"%Y-%m-%d")
YESTERDAY=$(date -d "yesterday" +"%Y-%m-%d")

# Copy logs from Raspberry Pi to local machine
scp "${PI_USER}@${PI_HOST}:${PI_LOG_PATH}/*${TODAY}*" "$LOCAL_DEST"
scp "${PI_USER}@${PI_HOST}:${PI_LOG_PATH}/*${YESTERDAY}*" "$LOCAL_DEST"

echo "Logs successfully extracted to $LOCAL_DEST"
