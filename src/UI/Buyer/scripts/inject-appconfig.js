// injects app config for this deploy into compiled code
// this enables a "one build, many deploy" strategy

/* eslint-disable no-undef */
/* eslint-disable @typescript-eslint/no-var-requires */

const fs = require('fs')
const args = process.argv
const appID = args[2]

if (!appID) {
  throw new Error('APP ID NOT PROVIDED')
}

const buildID = '<PLACEHOLDER>' // replace this in file transfer
const sourceDir = '.'
const mainJs = fs.readdirSync(sourceDir).find((filename) => {
  if (buildID !== '<PLACEHOLDER>') {
    return (
      filename.startsWith('main.') &&
      filename.includes(buildID) &&
      filename.endsWith('.js')
    )
  } else {
    return filename.startsWith('main.') && filename.endsWith('.js')
  }
})

const appConfig = fs.readFileSync(
  `${sourceDir}/assets/appConfigs/${appID}.json`,
  'utf-8'
)

const data = fs.readFileSync(`${sourceDir}/main-original.js`, 'utf-8')
const replaced = data.replace(/"#{environmentConfig}"/g, appConfig)
fs.writeFileSync(`${sourceDir}/${mainJs}`, replaced)
