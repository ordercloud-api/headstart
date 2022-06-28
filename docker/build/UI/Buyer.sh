json -I -f  /usr/share/nginx/html/assets/appConfigs/defaultbuyer-test.json \
      -e "this.clientID='$BUYER_CLIENT_ID'" \
      -e "this.middlewareUrl='$MIDDLEWARE_URL'" \
      -e "this.marketplaceID='$MARKETPLACE_ID'" \
      -e "this.marketplaceName='$MARKETPLACE_NAME'" \
      -e "this.translateBlobUrl='$TRANSLATE_BLOB_URL'" \
      -e "this.baseUrl='$BUYER_URL'"  \
      -e "this.supportedLanguages='$SUPPORTED_LANGUAGES'" \
      -e "this.defaultLanguage='$DEFAULT_LANGUAGE'"

cd /usr/share/nginx/html
node inject-appconfig defaultbuyer-test
node inject-css defaultbuyer-test
cd -

nginx -g 'daemon off;'