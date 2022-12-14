# Starting server for seeding
1. Configure app settings, see instructions [here](../../docs/AZURE_APP_CONFIGURATION.md#local-development)
2. Ensure at least the following settings are defined otherwise you will get errors on Startup:
    - `CosmosSettings:EndpointUri`
    - `CosmosSettings:PrimaryKey`
    - `CosmosSettings:DatabaseName`
    - `StorageAccountSettings:ConnectionString`
    - `StorageAccountSettings:BlobPrimaryEndpoint`
3. Start the application by selecting your profile from the dropdown (result of step #1)

# Starting server for normal operations
1. Configure app settings, see instructions [here](../../docs/AZURE_APP_CONFIGURATION.md#local-development)
2. Ensure at least the following settings are defined otherwise you will get errors on Startup:
    - `CosmosSettings:EndpointUri`
    - `CosmosSettings:PrimaryKey`
    - `CosmosSettings:DatabaseName`
    - `StorageAccountSettings:ConnectionString`
    - `StorageAccountSettings:BlobPrimaryEndpoint`
3. Ensure the following settings are defined (are available after [seeding your marketplace](../../README.md#seeding-ordercloud-data))
    - `OrderCloudSettings:MiddlewareClientID`
    - `OrderCloudSettings:MiddlewareClientSecret`
    - `OrderCloudSettings:ClientIDsWithAPIAccess` - ClientIDs of both the buyers and seller as a comma separated string
    - `OrderCloudSettings:WebhookHashKey` - part of the seed *request*
4. Start the application by selecting your debug profile from the dropdown (result of step #1)

# Importing the Middleware API in Postman

In Postman, using a existing or new Workspace:
1. Click on **Import**.
2. Select the **Link** tab.
3. Enter the url *\<MiddlewareUrl\>*/swagger/v1/swagger.json, e.g. https://localhost:5001/swagger/v1/swagger.json.
4. Click **Continue**.
5. Leave the import configuration with the default settings (or change them if you are advanced user).
6. Finalise the import by clicking **Import**.
