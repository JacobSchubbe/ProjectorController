#!/bin/bash

# Variables (customize these for your project)
IMAGE_NAME="projectorcontroller"                     # Replace with your Docker image name
TAR_FILE="projectorcontroller.tar"                        # Name of the exported TAR file
TARGET_HOST="jacob.schubbe@raspberrypi.local"                     # Replace with your target host (e.g., pi)
REMOTE_DIR="/home/jacob.schubbe/projectorcontroller"                # Replace with the target directory on the remote host
DOCKER_COMPOSE_PATH="docker-compose.yaml"   # Replace with your Docker Compose file path
DOCKER_REMOTE_COMPOSE_DIR="$REMOTE_DIR/"     # Remote location for the docker-compose.yml file

#echo "Docker compose build"
#docker compose build

#echo "Saving docker image to TAR"
#docker save -o $TAR_FILE $IMAGE_NAME
#
#echo "Transferring TAR to the remote machine"
#scp $TAR_FILE $TARGET_HOST:$REMOTE_DIR/
#
#echo "Transferring docker compose configuration"
#scp $DOCKER_COMPOSE_PATH $TARGET_HOST:$DOCKER_REMOTE_COMPOSE_DIR
#
#echo "Stopping any running containers"
#ssh $TARGET_HOST "(cd $DOCKER_REMOTE_COMPOSE_DIR && docker compose down)"
#
#echo "Loading image on the remote machine"
#ssh $TARGET_HOST "docker load < $REMOTE_DIR/$TAR_FILE"
#
#echo "Listing Docker images on the remote machine"
#ssh $TARGET_HOST "docker images"
#
echo "Starting Docker Compose on the remote machine"
ssh $TARGET_HOST "(cd $DOCKER_REMOTE_COMPOSE_DIR && docker compose up -d)"
#
#echo "Cleanup: removing the transferred TAR on the remote machine"
#ssh $TARGET_HOST "rm $REMOTE_DIR/$TAR_FILE"
#
#echo "Cleanup: removing the original TAR from host machine"
#rm $TAR_FILE

echo "Done!"