FROM node:20 AS node-build
WORKDIR /webapp
COPY ./signalr-vue-app .

# Set the environment variable before building the Vue app
ARG VUE_APP_API_URL
ENV VUE_APP_API_URL=$VUE_APP_API_URL

ARG VUE_APP_PORT
ENV VUE_APP_PORT=$VUE_APP_PORT

RUN npm install
RUN npm run build

# Serve the built Vue.js application using NGINX
FROM nginx:alpine AS production
COPY --from=node-build /webapp/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]