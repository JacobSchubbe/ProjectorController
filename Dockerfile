ARG TARGETPLATFORM

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ProjectController/ProjectController.csproj", "ProjectController/"]
COPY . .
WORKDIR "/src/ProjectController"
RUN dotnet restore "ProjectController.csproj"
RUN dotnet publish "ProjectController.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM --platform=$TARGETPLATFORM node:18-alpine AS frontend-builder
WORKDIR /signalr-vue-app
COPY ./signalr-vue-app/package*.json ./
ARG VUE_APP_API_URL
ARG VUE_APP_PORT
ENV VUE_APP_API_URL=${VUE_APP_API_URL}
ENV VUE_APP_PORT=${VUE_APP_PORT}
RUN npm install
COPY ./signalr-vue-app ./
RUN npm run build

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish /app/
RUN apk add --no-cache nginx icu-libs
COPY nginx/nginx.conf /etc/nginx/http.d/default.conf
COPY --from=frontend-builder /signalr-vue-app/dist /usr/share/nginx/html

EXPOSE 80
EXPOSE 19521

COPY ./entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]