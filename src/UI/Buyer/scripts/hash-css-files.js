// angular doesn't hash lazy-loaded css so we'll do it ourselves
// https://github.com/angular/angular-cli/issues/12552

/* eslint-disable no-undef */
/* eslint-disable @typescript-eslint/no-var-requires */
const hasha = require('hasha')
const fs = require('fs')

let manifest = {}

const sourceDir = './dist'
fs.readdirSync(sourceDir)
  .filter((filename) => filename.endsWith('.css'))
  .forEach((filename) => {
    const hash = hasha.fromFileSync(`${sourceDir}/${filename}`).substring(0, 10) // unique enough for our cache busting purposes

    const withoutExtension = filename.slice(0, -4)
    const hashedFileName = `${withoutExtension}.${hash}.css`
    fs.renameSync(`${sourceDir}/${filename}`, `${sourceDir}/${hashedFileName}`)
    manifest[filename] = hashedFileName
  })

fs.writeFileSync(
  `${sourceDir}/css-manifest.json`,
  JSON.stringify(manifest, null, 4)
)
