# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY traderview/traderview.client/package*.json ./

# Install dependencies
RUN npm ci

# Copy source files
COPY traderview/traderview.client/ ./

# Copy Docker environment file
COPY traderview/traderview.client/.env.docker ./.env.production

# Build the application using production config (no dev certs needed)
RUN npm run build -- --config vite.config.production.ts

# Runtime stage
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Remove default nginx static assets
RUN rm -rf ./*

# Copy built assets from build stage
COPY --from=build /app/dist .

# Copy nginx configuration
COPY traderview/traderview.client/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
ENTRYPOINT ["nginx", "-g", "daemon off;"]
