ARG TARGETPLATFORM

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ProjectController/ProjectController.csproj", "ProjectController/"]
RUN dotnet restore "ProjectController/ProjectController.csproj"
COPY . .
WORKDIR "/src/ProjectController"
RUN dotnet publish "ProjectController.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    -r linux-arm64 \
    --self-contained false \
    /p:UseAppHost=false
        
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
    bash \
    lirc

RUN apk add --no-cache ca-certificates && update-ca-certificates
RUN wget -q -O /etc/apk/keys/sgerrand.rsa.pub https://alpine-pkgs.sgerrand.com/sgerrand.rsa.pub && \
    wget -q https://github.com/sgerrand/alpine-pkg-glibc/releases/download/2.35-r0/glibc-2.35-r0.apk && \
    wget -q https://github.com/sgerrand/alpine-pkg-glibc/releases/download/2.35-r0/glibc-bin-2.35-r0.apk && \
    wget -q https://github.com/sgerrand/alpine-pkg-glibc/releases/download/2.35-r0/glibc-i18n-2.35-r0.apk && \
    apk add --no-cache --force-overwrite \
    glibc-2.35-r0.apk \
    glibc-bin-2.35-r0.apk \
    glibc-i18n-2.35-r0.apk && \
    rm -f *.apk && \
    /usr/glibc-compat/bin/localedef -i en_US -f UTF-8 en_US.UTF-8        

ENV LD_LIBRARY_PATH=/app/runtimes/linux-arm64/native:$LD_LIBRARY_PATH
ENV LD_DEBUG=libs

WORKDIR /app
COPY --from=build /app/publish /app/
COPY nginx/nginx.conf /etc/nginx/http.d/default.conf
COPY --from=frontend-builder /signalr-vue-app/dist /usr/share/nginx/html
COPY ./LIRCConfigs/DenverCableBox.lircd.conf /etc/lirc/lircd.conf.d/DenverCableBox.lircd.conf
COPY ./LIRCConfigs/lirc_options.conf /etc/lirc/lirc_options.conf
RUN rm -f /etc/lirc/lircd.conf.d/default.lircd.conf
RUN mkdir -p /app/runtimes/linux-arm64/native/ && \
    cp /app/libSystem.IO.Ports.Native.so /app/runtimes/linux-arm64/native/

EXPOSE 80 19521 5037 5555

COPY ./entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]