ARG TARGETPLATFORM

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ProjectController/ProjectController.csproj", "ProjectController/"]
RUN dotnet restore "ProjectController/ProjectController.csproj"
COPY . .
WORKDIR "/src/ProjectController"
RUN dotnet publish "ProjectController.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM --platform=$TARGETPLATFORM node:18-alpine AS frontend-builder
WORKDIR /signalr-vue-app

RUN apk add --no-cache --update \
    android-tools \
    bash \
    && rm -rf /var/cache/apk/*

COPY ./signalr-vue-app/package*.json ./
ARG VUE_APP_API_URL
ARG VUE_APP_PORT
ENV VUE_APP_API_URL=${VUE_APP_API_URL}
ENV VUE_APP_PORT=${VUE_APP_PORT}
RUN npm install
COPY ./signalr-vue-app ./
RUN npm run build

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS final
RUN apk add --no-cache \
    nginx \
    icu-libs \
    android-tools \
    bash

WORKDIR /app
COPY --from=build /app/publish /app/

COPY nginx/nginx.conf /etc/nginx/http.d/default.conf

COPY --from=frontend-builder /signalr-vue-app/dist /usr/share/nginx/html

EXPOSE 80 19521 5037 5555

COPY ./entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]