#################
# Build the app #
#################
FROM node:16-alpine as build

WORKDIR /app

COPY package.json package-lock.json ./

RUN npm install

COPY . .

RUN npm install -g @angular/cli

# RUN apk add gettext \
#     && envsubst < src/environments/environment.template.ts > src/environments/environment.ts

RUN ng build --configuration production --output-path=/dist

################
# Run in NGINX #
################
FROM nginx:latest

COPY --from=build /dist /usr/share/nginx/html
COPY /nginx.conf  /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["/bin/sh", "-c", "envsubst < /usr/share/nginx/html/assets/env.template.js > /usr/share/nginx/html/assets/env.js && exec nginx -g 'daemon off;'"]
