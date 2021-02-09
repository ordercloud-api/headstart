// makes an original copy of index.html to reference during release phase
// this enables a "one build, many deploy" strategy

/* eslint-disable no-undef */
/* eslint-disable @typescript-eslint/no-var-requires */

const fs = require('fs')

const sourceDir = `./dist/default-components`

fs.copyFileSync(`${sourceDir}/index.html`, `${sourceDir}/index-original.html`)
