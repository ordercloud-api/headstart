/**
 * In Buyer and Seller we are caching node_modules however since the SDK is built locally we can't rely on
 * package-lock.json to tell us when the SDK has changed and thus when the cache should be busted
 * this script produces a hash of the dist folder so that we know when to cache-bust node_modules
 */
 const { hashElement } = require('folder-hash')
 const fs = require('fs')
 hashElement('./dist').then(result => {
   fs.writeFileSync('./files-changed-hash', result.hash, { encoding: 'utf-8' })
 })