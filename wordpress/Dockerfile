FROM wordpress:latest
# COPY .htaccess /var/www/html/.htaccess
COPY dev.ini /usr/local/etc/php/conf.d/dev.ini
# COPY wp-config.php /var/www/html/wp-config.php
RUN curl -LkSso /usr/bin/mhsendmail "https://github.com/mailhog/mhsendmail/releases/download/v0.2.0/mhsendmail_linux_amd64" \
    && chmod 0755 /usr/bin/mhsendmail