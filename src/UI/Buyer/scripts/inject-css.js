// inject relevant theme css in index.html
// this enables a "one build, many deploy" strategy

/* eslint-disable no-undef */
/* eslint-disable @typescript-eslint/no-var-requires */
/* eslint-disable @typescript-eslint/no-unsafe-member-access */
/* eslint-disable @typescript-eslint/no-unsafe-assignment */
/* eslint-disable @typescript-eslint/restrict-template-expressions */

const fs = require('fs')
const args = process.argv
const appID = args[2]

if (!appID) {
  throw new Error('APP ID NOT PROVIDED')
}

const cssManifest = fs.readFileSync('./css-manifest.json', 'utf-8')
const indexHtml = fs.readFileSync('./index-original.html', 'utf-8')

const cssManifestJson = JSON.parse(cssManifest)
const styleID = appID.split('-')[0]
const originalCssFilename = `${styleID}.css`
const hashedCssFilename = cssManifestJson[originalCssFilename]
if (!hashedCssFilename) {
  throw new Error(`Unable to find css file for ${appID}`)
}
const updatedIndexHtml = indexHtml.replace(
  '<!-- client-theme-placeholder -->',
  `<link href="${hashedCssFilename}" rel="stylesheet" />`
)

fs.writeFileSync('./index.html', updatedIndexHtml)
