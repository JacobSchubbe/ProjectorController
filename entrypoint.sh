#!/bin/sh

# Start Lirc
lircd --device=/dev/lirc0 --output=/var/run/lirc/lircd

# Start ASP.NET Core backend
dotnet /app/ProjectController.dll &

# Start Nginx
nginx -g "daemon off;"

