// makes an original copy of main.js to reference during release phase
// this enables a "one build, many deploy" strategy

/* eslint-disable no-undef */
/* eslint-disable @typescript-eslint/no-var-requires */

const fs = require('fs')

const sourceDir = `./dist`
const mainJs = fs
  .readdirSync(sourceDir)
  .find((filename) => filename.startsWith('main') && filename.endsWith('.js'))

fs.copyFileSync(`${sourceDir}/${mainJs}`, `${sourceDir}/main-original.js`)
