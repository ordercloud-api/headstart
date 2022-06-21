You can have any number of configurations each representing a deployment. By default we've created three such deployments one for each environment (test, uat, and production).

| Name                | Description |
|---------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| hostedApp          | Indicates that an app is hosted. This should always be true and gets set to false automatically if developing locally. |
| marketplaceID      | The ID of the marketplace that this buyer belongs to. This can be found in the portal <https://portal.ordercloud.io>. |
| marketplaceName    | The name of the marketplace this buyer belongs to. Used for general display. |
| appname            | A short name for your app. It will be used as general display throughout the app. |
| clientID           | The OrderCloud API Client ID that the seller app will use for direct requests to the OrderCloud API. |
| middlewareUrl      | The base url to the hosted backend middleware API. |
| translateBlobUrl   | The base url to the folder including your translations. See <https://github.com/ngx-translate/core> for more info. |
| supportedLanguages | The list of locales ids that can be website can be translated to. The source of translations files are in [wwwroot/i18n](../../../../../Middleware\src\Headstart.API\wwwroot\i18n). |
| defaultLanguage    | The default language that the website will be rendered in. Must also exist in the supported languages. |
| blobStorageUrl     | The base url to the blob storage account. |
| orderCloudApiUrl   | The base url to the OrderCloud API - generally follows the format *https://{region}-{environment}.ordercloud.io* and can be found in the portal under marketplace settings. |
