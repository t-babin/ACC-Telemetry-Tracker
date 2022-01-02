FROM nginx:latest

COPY ./conf/default.conf /etc/nginx/conf.d/default.conf

CMD ["/bin/sh", "-c", "exec nginx -g 'daemon off;'"]