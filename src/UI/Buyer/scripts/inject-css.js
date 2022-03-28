// inject relevant theme css as well as conditional javascript scripts in index.html
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

/**
 * Inject theme specific styles into html
 */
const cssManifest = fs.readFileSync('./css-manifest.json', 'utf-8')
const indexHtml = fs.readFileSync('./index-original.html', 'utf-8')

const cssManifestJson = JSON.parse(cssManifest)
const styleID = appID.split('-')[0]
const originalCssFilename = `${styleID}.css`
const hashedCssFilename = cssManifestJson[originalCssFilename]
if (!hashedCssFilename) {
  throw new Error(`Unable to find css file for ${appID}`)
}
let updatedIndexHtml = indexHtml.replace(
  '<!-- client-theme-placeholder -->',
  `<link href="${hashedCssFilename}" rel="stylesheet" />`
)

/**
 * Inject conditional script (Sitecore CDP) into html
 */

const appConfig = JSON.parse(
  fs.readFileSync(`./assets/appConfigs/${appID}.json`, 'utf-8') || '{}'
)
if (appConfig.useSitecoreCDP) {
  const clientKey = appConfig.sitecoreCDPApiClient
  const target = appConfig.sitecoreCDPTargetEndpoint
  const cookieDomain = appConfig.sitecoreCDPCookieDomain
  const javascriptLibraryVersion = appConfig.sitecoreCDPJavascriptLibraryVersion
  const pointOfSale = appConfig.sitecoreCDPPointOfSale
  const webFlowTarget = appConfig.sitecoreCDPWebFlowTarget

  // Tracker installation https://doc.sitecore.com/cdp/en/developers/sitecore-customer-data-platform--data-model-2-1/javascript-tagging-examples-for-web-pages.html
  updatedIndexHtml = indexHtml.replace(
    '<!-- sitecore-cdp-placeholder -->',
    `<script type="text/javascript">
// Define the Boxever queue 
var _boxeverq = _boxeverq || [];

// Define the Boxever settings 
var _boxever_settings = {
    client_key: '${clientKey}',
    target: '${target}',
    cookie_domain: '${cookieDomain}',
    javascriptLibraryVersion: '${javascriptLibraryVersion}',
    pointOfSale: '${pointOfSale}', // Replace with the same point of sale configured in system settings"
    web_flow_target: '${webFlowTarget}', // Replace with path for the Amazon CloudFront CDN for Sitecore Personalize"
    web_flow_config: { async: false, defer: false } // Customize the async and defer script loading attributes
};
// Import the Boxever library asynchronously 
(function() {
      var s = document.createElement('script'); s.type = 'text/javascript'; s.async = true;  
      s.src = 'https://d1mj578wat5n4o.cloudfront.net/boxever-${javascriptLibraryVersion}.min.js';
      var x = document.getElementsByTagName('script')[0]; x.parentNode.insertBefore(s, x);
})();
</script>`
  )
}

fs.writeFileSync('./index.html', updatedIndexHtml)
