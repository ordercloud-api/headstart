You can have any number of configurations each representing a deployment. By default we've created three such deployments one for each environment.

| Name                          | Description                                                                                                                                      |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| hostedApp                     | Indicates that an app is hosted. This should always be true and gets set to false automatically if developing locally                            |
| appname                       | A short name for your app. It will be used as general display throughout the app.                                                                |
| appID                         | Unique ID for your app configuration it should be in the form {name}-{environment} for example defaultbuyer-uat                                  |
| clientID                      | The clientID for this application                                                                                                                |
| incrementorPrefix             | This string is prefixed to any ID that has been incremented                                                                                      |
| baseUrl                       | The base url for this hosted application                                                                                                         |
| middlewareUrl                 | The base url to the hosted backend middleware API                                                                                                |
| creditCardIframeUrl           | The base url to cardconnect's iframe url                                                                                                         |
| sellerID                      | The ID of the seller organization that this buyer belongs to. This can be found in the portal https://portal.ordercloud.io                       |
| translateBlobUrl              | The base url to the folder including your translations. See https://github.com/ngx-translate/core for more info                                  |
| orderCloudApiUrl              | The base url to the OrderCloud API. Can be one of: https://api.ordercloud.io, https://sandbox.ordercloud.io, https://staging.ordercloud.io       |
| theme                         | An object containing theme related configuration. At this time only logoSrc exists. logoSrc is the path to a logo to be used for general display |
| appInsightsInstrumentationKey | The instrumentation key for application insights. This is optional, if null no telemetry will be captured.                                       |
