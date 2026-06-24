FROM node:22-alpine AS build
WORKDIR /app

COPY Web/InfiniteJourney.Web/package*.json ./
RUN npm ci

COPY Web/InfiniteJourney.Web/ .
RUN npm run build -- --configuration production

FROM nginx:1.27-alpine AS final
COPY Web/InfiniteJourney.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/InfiniteJourney.Web/browser /usr/share/nginx/html
COPY Web/InfiniteJourney.Web/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

EXPOSE 80
ENTRYPOINT ["/docker-entrypoint.sh"]
