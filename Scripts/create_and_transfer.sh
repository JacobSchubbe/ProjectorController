#!/bin/bash

# Variables (customize these for your project)
IMAGE_NAME="projectorcontroller"                     # Replace with your Docker image name
TAR_FILE="projectorcontroller.tar"                        # Name of the exported TAR file
TARGET_HOST="jacob.schubbe@192.168.0.174"                     # Replace with your target host (e.g., pi)
REMOTE_DIR="/home/jacob.schubbe/projectorcontroller"                # Replace with the target directory on the remote host
DOCKER_COMPOSE_PATH="../docker-compose.yaml"   # Replace with your Docker Compose file path
DOCKER_REMOTE_COMPOSE_DIR="$REMOTE_DIR/"     # Remote location for the docker-compose.yml file
PI_USER="jacob.schubbe"                   # Raspberry Pi username
PI_HOST="192.168.0.174"    # Raspberry Pi hostname or IP
PI_LOG_PATH="/home/logs"    # Path to logs on the Raspberry Pi
LOCAL_DEST="./logs"            # Local directory to save logs

echo "Docker compose build"
docker compose build --no-cache

echo "Saving docker image to TAR"
docker save -o $TAR_FILE $IMAGE_NAME

echo "Transferring TAR and Docker Compose file to the remote machine"
scp $TAR_FILE $DOCKER_COMPOSE_PATH $TARGET_HOST:$REMOTE_DIR/

echo "Transferring logs before loading new image and deleting old logs"
mkdir -p "$LOCAL_DEST"
TODAY=$(date +"%Y-%m-%d")
YESTERDAY=$(date -d "yesterday" +"%Y-%m-%d")
scp "${PI_USER}@${PI_HOST}:${PI_LOG_PATH}/*${TODAY}*" "$LOCAL_DEST"
scp "${PI_USER}@${PI_HOST}:${PI_LOG_PATH}/*${YESTERDAY}*" "$LOCAL_DEST"


echo "Running remote commands in a single SSH session"
ssh $TARGET_HOST << EOF
    set -e  # Exit on error
    echo "Stopping any running containers"
    cd $DOCKER_REMOTE_COMPOSE_DIR && docker-compose down

    echo "Removing old logs"
    sudo rm -rf /home/logs/*
    echo "Logs removed from the pi."

    echo "Removing old image"
    docker rmi $IMAGE_NAME || true  # Do not exit if image is not found
    docker rmi \$(docker images -f "dangling=true" -q) || true

    echo "Loading Docker image from TAR"
    docker load < $REMOTE_DIR/$TAR_FILE

    echo "Starting Docker Compose"
    docker-compose up -d

    echo "Checking running state of container"
    docker ps -a

    echo "Cleanup: removing the transferred TAR"
    rm $REMOTE_DIR/$TAR_FILE
EOF

echo "Cleanup: removing the original TAR from host machine"
rm $TAR_FILE

echo "Done!"

#docker rmi $(docker images -f "dangling=true" -q)

#docker run -d -p 8081:80 -p 19521:19521 -e ASPNETCORE_ENVIRONMENT=Production -e CustomConfig__IPAddress=localhost -e CustomConfig__
#DebugVersion=false projectorcontroller