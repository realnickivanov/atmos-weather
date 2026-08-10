FROM node:26.7.0-alpine AS dependencies
WORKDIR /app
COPY src/frontend/atmos-web/package.json src/frontend/atmos-web/package-lock.json ./
RUN npm ci --no-audit --no-fund

FROM dependencies AS build
COPY src/frontend/atmos-web/ ./
RUN npm run build

FROM nginxinc/nginx-unprivileged:1.31.3-alpine AS final
COPY src/frontend/atmos-web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://localhost:8080/ || exit 1
