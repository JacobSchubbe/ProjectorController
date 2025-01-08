#!/bin/sh

# Start ASP.NET Core backend
dotnet /app/ProjectController.dll &

# Start Nginx
nginx -g "daemon off;"