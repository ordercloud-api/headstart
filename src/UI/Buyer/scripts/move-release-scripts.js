// move scripts needed for release phase into dist

/* eslint-disable no-undef */
/* eslint-disable @typescript-eslint/no-var-requires */

const fs = require('fs')
fs.copyFileSync(
  './scripts/inject-appconfig.js',
  './dist/default-components/inject-appconfig.js'
)

fs.copyFileSync(
  './scripts/inject-css.js',
  './dist/default-components/inject-css.js'
)
