You can have any number of configurations each representing a deployment. By default we've created three such deployments one for each environment (test, uat, and production).

| Name                                | Description |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| hostedApp                           | Indicates that an app is hosted. This should always be true and gets set to false automatically if developing. |
| appname                             | A short name for your app. It will be used as general display throughout the app. |
| appID                               | Unique ID for your app configuration it should be in the form {name}-{environment} for example *defaultbuyer-uat*. |
| clientID                            | The OrderCloud API Client ID that the buyer app will use for direct requests to the OrderCloud API. |
| incrementorPrefix                   | This string is prefixed to any ID that has been incremented. |
| baseUrl                             | The base url for this hosted application. |
| middlewareUrl                       | The base url to the hosted backend middleware API. |
| creditCardIframeUrl                 | The base url to cardconnect's iframe url. |
| translateBlobUrl                    | The base url to the folder including your translations. See https://github.com/ngx-translate/core for more info. |
| supportedLanguages                  | The list of locales ids that can be website can be translated to. The source of translations files are in [wwwroot/i18n](../../../../../Middleware\src\Headstart.API\wwwroot\i18n). |
| defaultLanguage                     | The default language that the website will be rendered in. Must also exist in the supported languages. |
| marketplaceID                       | The ID of the seller company that this buyer belongs to. This can be found in the portal https://portal.ordercloud.io. |
| marketplaceName                     | The Name of the seller company that this buyer belongs to. This can be found in the portal https://portal.ordercloud.io. |
| sellerQuoteContactEmail             | The default email for quote products that are not owned by a supplier, i.e. a product's `xp.DefaultSupplierID` is not set. |
| orderCloudApiUrl                    | The base url to the OrderCloud API - generally follows the format *https://{region}-{environment}.ordercloud.io* and can be found in the portal under marketplace settings. |
| useSitecoreSend                     | Enables the Sitecore Send integration to capture website events. Requires a valid `sitecoreSendWebsiteID`. |
| sitecoreSendWebsiteID               | The website ID in Sitecore Send. Sitecore Send can be configured following the [Enable website tracking](https://doc.sitecore.com/send/en/users/sitecore-send/enable-website-tracking.html) documentation. |
| useSitecoreCDP                      | Enables the Sitecore CDP integration for customer tracking. Requires valid *`sitecoreCDP`* prefixed configurations below. See [JavaScript tagging examples for webpages](https://doc.sitecore.com/cdp/en/developers/sitecore-customer-data-platform--data-model-2-1/javascript-tagging-examples-for-webpages.html) for more information. |
| sitecoreCDPTargetEndpoint           | This is the API target endpoint for Sitecore CDP and Sitecore Personalize. |
| sitecoreCDPCookieDomain             | This is the top-level cookie domain of the website that you are integrating. Ensure that you include the period at the domain root. This ensures that Sitecore CDP cookie is stored as a first-party cookie. |
| sitecoreCDPJavascriptLibraryVersion | This is the version of the Sitecore CDP (Boxever) JavaScript library. |
| sitecoreCDPPointOfSale              | The point of sale (storefront) captured on each page the guest visited. |
| sitecoreCDPWebFlowTarget            | This is the path for the Amazon CloudFront Content Delivery Network (CDN) for Sitecore Personalize. |
| theme                               | An object containing theme related configuration. At this time only logoSrc exists. logoSrc is the path to a logo to be used for general display |
| appInsightsInstrumentationKey       | The instrumentation key for application insights. This is optional, if null no telemetry will be captured. |
| anonymousShoppingEnabled            | Enables anonymous shoppers in the storefront. Requires the buyer app's API Client `IsAnonBuyer` is true and a `DefaultContextUserName` is configured. |
| acceptedPaymentMethods              | The array of accepted payment methods in the storefront checkout. |
