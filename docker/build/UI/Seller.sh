json -I -f /usr/share/nginx/html/assets/appConfigs/defaultadmin-test.json \
      -e "this.sellerID='$SELLER_ID'" \
      -e "this.clientID='$SELLER_CLIENT_ID'" \
      -e "this.middlewareUrl='$MIDDLEWARE_URL'" \
      -e "this.translateBlobUrl='$TRANSLATE_BLOB_URL'" \
      -e "this.blobStorageUrl='$BLOB_STORAGE_URL'" \
      -e "this.buyerConfigs['Default Buyer'].clientID='$BUYER_CLIENT_ID'"

cd /usr/share/nginx/html
node inject-appconfig defaultadmin-test
cd -

nginx -g 'daemon off;'