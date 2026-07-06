# 1. Build stage
FROM node:22-alpine AS build
WORKDIR /app

# Copier les fichiers de dépendances
COPY package*.json ./
RUN npm ci

# Copier le reste du code
COPY . .

# On définit une variable d'env par défaut pour le build 
# (Sera remplacée dynamiquement au lancement du conteneur)
ENV VITE_API_URL=__VITE_API_URL_PLACEHOLDER__

# Build production
RUN npm run build

# 2. Serve stage (nginx)
FROM nginx:alpine

# Copier le build de l'étape précédente
COPY --from=build /app/dist /usr/share/nginx/html

# On ajoute un script pour remplacer le placeholder par la vraie variable d'env
# au démarrage du conteneur
RUN echo '#!/bin/sh' > /docker-entrypoint.d/inject-env.sh && \
    echo 'find /usr/share/nginx/html -type f -name "*.js" -exec sed -i "s|__VITE_API_URL_PLACEHOLDER__|$VITE_API_URL|g" {} +' >> /docker-entrypoint.d/inject-env.sh && \
    chmod +x /docker-entrypoint.d/inject-env.sh

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]