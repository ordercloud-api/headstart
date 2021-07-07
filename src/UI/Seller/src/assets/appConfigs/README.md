You can have any number of configurations each representing a deployment. By default we've created three such deployments one for each environment.

| Name                          | Description                                                                                                                                      |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| hostedApp                     | Indicates that an app is hosted. This should always be true and gets set to false automatically if developing locally                            |
| sellerID                      | The ID of the seller organization that this buyer belongs to. This can be found in the portal https://portal.ordercloud.io                       |
| sellerName                    | The name of the seller this buyer belongs to. Used for general display                                                                           |
| appname                       | A short name for your app. It will be used as general display throughout the app.                                                                |
| clientID                      | The clientID for this application                                                                                                                |
| middlewareUrl                 | The base url to the hosted backend middleware API                                                                                                |
| translateBlobUrl              | The base url to the folder including your translations. See https://github.com/ngx-translate/core for more info                                  |
| blobStorageUrl                | the base url to the blob storage account                                                                                                         |
| orderCloudApiUrl              | The base url to the OrderCloud API. Can be one of: https://api.ordercloud.io, https://sandboxapi.ordercloud.io, https://stagingapi.ordercloud.io       |                                                     |
