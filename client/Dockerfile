#################
# Build the app #
#################
FROM node:16-alpine as build

WORKDIR /app

COPY package.json package-lock.json ./

RUN npm install

COPY . .

RUN npm install -g @angular/cli \
    && envsubs < src/environments/environment.template.ts > src/environments/environment.ts

RUN ng build --configuration production --output-path=/dist

################
# Run in NGINX #
################
FROM nginx:latest

COPY --from=build /dist /usr/share/nginx/html
COPY /nginx.conf  /etc/nginx/conf.d/default.conf

EXPOSE 80