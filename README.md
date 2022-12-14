# Headstart

Welcome! The purpose of this project is to give you and your business a "headstart" to building an e-commerce solution on OrderCloud. This is a complete and opinionated solution but is only meant to be a starting point to *your* complete solution, it is expected that you will need to make customizations to this project, after which the code is yours to own and maintain. This solution is composed of three main parts:

1. [Middleware](./src/Middleware/README.md) - The backend written in .NET 6. Extends and enhances the capabilities of OrderCloud by integrating with best-of-breed services.
2. [Buyer](./src/UI/Buyer/README.md) - The frontend buyer application written in Angular. This includes the entire shopping experience from the perspective of a buyer user.
3. [Seller](./src/UI/Seller/README.md) - The frontend admin application written in Angular. This includes everything needed to manage the data in your buyer application(s).

## Demo

Want to check out the features included in headstart without having to build and deploy your own instance? We have a hosted instances that you are free to log in and check out. 

### Buyer
| URL      | <https://headstartdemo-buyer-ui-test.azurewebsites.net> |
|----------|-------------------------------------------------------|
| Username | testbuyer                                             |
| Password | Summer2021!                                           |

### Admin

| URL      | <https://headstartdemo-admin-ui-test.azurewebsites.net> |
|----------|-------------------------------------------------------|
| Username | testadmin                                             |
| Password | Summer2021!                                           |

### Credentials

## Initial Setup

There are some tasks that must be completed before you can get an instance of Headstart running. This section will walk you through each of them.

### Provisioning Azure Resources

[App Service](https://docs.microsoft.com/en-us/azure/app-service/overview) - you'll need at least one app service to host the middleware API. For simplicity, we also set up one for each Buyer & Seller application though since they are static sites you have a variety of options at your disposal for how to host those.

[Azure Cosmos Database](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction) - While we use the OrderCloud API to host all e-commerce data this is a complete solution that requires handling data that doesn't natively exist as part of OrderCloud API. Some examples are report templates. To that end, we are using Cosmos as our DB of choice (Core SQL). You will need one database per environment (we recommend three environments: Test, UAT, and Production).

[Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview) - This is an optional but highly recommended addition and will actually show up as an option when adding an app service. There is some additional configuration if you want to track the frontend. Look at the frontend app configs to provide your `appInsightsInstrumentationKey`

[Storage Account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal) - Provides all of azure data storage objects. Used to store currency conversions and translation tables. You will need a storage account for each environment (we recommend three environments: Test, UAT, and Production).

[Azure App Configuration](https://docs.microsoft.com/en-us/azure/azure-app-configuration/overview) - Used to store sensitive app settings that are consumed by the backend middleware application. Checkout our guide [here](./docs/AZURE_APP_CONFIGURATION.md) for configuring azure app configuration with the necessary settings. You will need an azure app configuration for each environment (we recommend three environments: Test, UAT, and Production)
### Seeding OrderCloud Data

This solution depends on a lot of data to be initialized in a particular way. To make it easy when starting a new project we've created an endpoint that does this for you. Just call it with some information, wait a few seconds, and presto: You'll have a marketplace that is seeded with all the right data to get you started immediately.

Detailed Steps:

1. Sign in to the [OrderCloud portal](https://portal.ordercloud.io/).
2. Create a new marketplace in the portal if you don't already have one.
3. Find your marketplace and save the unique identifier this is your MarketplaceID in step 6.
4. Follow the instructions [here](./src/Middleware/README.md) to start your server locally.
5. Download and open [Postman](https://www.postman.com/downloads/) so that you can make API calls to your local server.
6. Make a POST to `/seed` endpoint with [this template body](./assets/templates/SeedTemplate.json). For a description of the properties please refer to [the definition](./src/Middleware/integrations/OrderCloud.Integrations.EnvironmentSeed/Models/EnvironmentSeedRequest.cs).
7. You should now be able to add the following settings to your middleware based on the details from the successful response:
    - `OrderCloudSettings:MiddlewareClientID`
    - `OrderCloudSettings:MiddlewareClientSecret`
    - `OrderCloudSettings:ClientIDsWithAPIAccess` - ClientIDs of both the buyers and seller as a comma separated string
    - `OrderCloudSettings:WebhookHashKey` - part of the seed *request*
8. Restart your server so that the newly added app settings take effect
9. Continue to the next section for configuring your frontend applications

### Frontend Configuration

Your backend middleware is configured and all your resources have been provisioned and your data is seeded. Now we'll need to configure your buyer and seller applications. You can have any number of configurations each representing a deployment. By default, we've created three such deployments one for each environment.

- [Buyer](./src/UI/Buyer/src/assets/appConfigs)
- [Seller](./src/UI/Seller/src/assets/appConfigs)

Once your frontends are up and running, consider going through the ["Validating headstart setup"](./VALIDATING_HEADSTART_SETUP.md) guide to ensure everything is configured correctly. 
### Service Providers

In order to build a complete solution Headstart is integrated with various third-party services in many categories that we are defining as "Service Providers". By default, all of these service providers are _turned off_ and set to an empty string which indicates to the middleware to use a mocked service instead. This reduces friction when getting started however a production ready build will require some of these service providers to be enabled and credentials/configurations added to app settings, therefore we recommend getting those credentials and enabling these service providers early in the development process.

| Provider                | App Setting Key                                | App Setting Values                                                                                                                                                                                                                                     | Required for production    |
| ----------------------- | ---------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------- |
| Address Validation      | EnvironmentSettings:AddressValidationProvider  | [Smarty](./src/Middleware/integrations/OrderCloud.Integrations.Smarty/README.md)                                                                                                                                                                       | No                         |
| CMS                     | EnvironmentSettings:CMSProvider                | [Azure](./src/Middleware//integrations/OrderCloud.Integrations.CMS/README.md)                                                                                                                                                                                                                                                  | Yes                        |
| Currency Conversion     | EnvironmentSettings:CurrencyConversionProvider | [ExchangeRates](./src/Middleware/integrations/OrderCloud.Integrations.ExchangeRates/README.md)                                                                                                                                                                                                                                          | No                         |
| Email                   | EnvironmentSettings:EmailServiceProvider       | [SendGrid](./src/Middleware/integrations/OrderCloud.Integrations.SendGrid/README.md)                                                                                                                                                                                                                                               | No, but highly recommended |
| Order Management System | EnvironmentSettings:OMSProvider                | [Zoho](./src/Middleware/integrations/OrderCloud.Integrations.Zoho/README.md)                                                                                                                                                                                                                                                   | No                         |
| Payment                 | EnvironmentSettings:PaymentProvider            | [CardConnect](./src/Middleware/integrations/OrderCloud.Integrations.CardConnect/README.md)                                                                                                                                                                                                                                            | Yes                        |
| Shipping                | EnvironmentSettings:ShippingProvider           | EasyPost                                                                                                                                                                                                                                               | No                         |
| Tax                     | EnvironmentSettings:TaxProvider                | [Avalara](./src/Middleware/integrations/OrderCloud.Integrations.Avalara/README.md), [TaxJar](./src/Middleware/integrations/OrderCloud.Integrations.TaxJar/README.md), [Vertex](./src/Middleware/integrations/OrderCloud.Integrations.Vertex/README.md) | No, but highly recommended |

### Sitecore Send

Sitecore Send is a platform for sending automated email campaigns. It is integrated into the storefront in order to capture events like view product, add to cart and purchase. This data can provide intelligence for abandoned cart emails, user segmentation for personalized marketing and user-history-based product recommendations.

Usage is optional and controlled with the buyer setting `useSitecoreSend`. To connect Sitecore Send [get a website ID](https://doc.sitecore.com/send/en/users/sitecore-send/enable-website-tracking.html) and add it to buyer settings.

Sitecore Send and Ordercloud are both owned by Sitecore. You can expect the two products to be more integrated over time. SendGrid will be replaced by Sitecore Send once transactional email features are ready.

## Deploying your application

We recommend using [Azure Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/get-started/what-is-azure-pipelines?view=azure-devops) for building and releasing your code.

### Build Phase

We've included a [YAML build configuration file](./azure-pipelines.yml) that tells Azure how to:

- Build the middleware
- Run the middleware tests
- Build/Publish both the admin and buyer frontend
- Publish all the artifacts

The only small change you will need to make is to update the "Get Build Number" step and update the URL to point to your app's middleware. Information on how to use the YAML file can be found [here](https://docs.microsoft.com/en-us/azure/devops/pipelines/customize-pipeline?view=azure-devop)

### Release Phase

This project follows the [build once, deploy many](https://earlyandoften.wordpress.com/2010/09/09/build-once-deploy-many/) pattern to increase consistency for releases across environments. It accomplishes this by injecting the app configuration for the desired environment during the release phase as you'll see in the following steps. The example here is to deploy the test environment but the same process can be modified slightly for the other environments

- Configure Buyer for the environment - From the buyer artifacts directory run `node inject-css defaultbuyer-test && node inject-appconfig defaultbuyer-test`. This will inject both the css and the app settings for defaultbuyer-test environment
- Deploy Buyer - Deploy buyer artifacts to your buyer app service test slot
- Configure Seller for the environment - From the seller artifacts directory run `node inject-appconfig defaultadmin-test`. This will inject the app settings for defaultadmin-test.
- Deploy Seller - Deploy seller artifacts to your seller app service test slot
- Deploy Middleware - Deploy middleware artifacts to your middleware app service test slot. Make sure the environment variable `APP_CONFIG_CONNECTION` is set on your app service and points to the connection string to your azure app configuration

## Docker

You can run the project using Docker, sample docker-compose.yml file includes Buyer/Seller/Middleware as well as emulated Azure Storage via [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio), and Cosmos DB via the [Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/linux-emulator).

1. Make sure to [switch daemon to Linux containers](https://docs.docker.com/docker-for-windows/#switch-between-windows-and-linux-containers)
2. Copy `.env.template` file to `.env`
3. Add the following records to your Hosts file
    - 127.0.0.1 buyer.headstart.localhost
    - 127.0.0.1 seller.headstart.localhost
    - 127.0.0.1 api.headstart.localhost
4. From the project directory, start up your application by running `docker-compose up -d`
    - Note the `Middleware` container may take longer to start as it depends on the Cosmos emulator being healthy.
    - If you run into issues with @ordercloud/headstart-sdk not being found try running `docker-compose build --no-cache`
5. Follow the steps to seed the initial data values listed in the [Seeding OrderCloud Data](https://github.com/ordercloud-api/headstart#seeding-ordercloud-data) section above.
    - If you are building against a marketplace that has already been seeded you can instead open postman and call the endpoint PUT <http://headstart.api.localhost/devops/translations> to update your local azurite instance with the latest translation files.
6. Open `.env` file and populate the rest of the variables in the `REQUIRED ENVIRONMENT VARIABLES` section:
    - The following values comes from from the `/seed` command response executed in previous step
        - SELLER_CLIENT_ID
        - BUYER_CLIENT_ID
        - OrderCloudSettings_MiddlewareClientID
        - OrderCloudSettings_MiddlewareClientSecret
        - OrderCloudSettings_ClientIDsWithAPIAccess (a comma delimited list of API clients that can access middleware - start with BUYER_CLIENT_ID and SELLER_CLIENT_ID)
        - SELLER_ID (This should be set to the `MarketplaceID`)

7. Restart your docker containers to make use of the new env vars by running `docker-compose down` followed by `docker-compose up -d`.
    - Note you can't run a `docker-compose restart` here as if the containers are already running then the Middleware app will restart before Cosmos is healthy.
8. Follow the steps in the [Validating Setup](https://github.com/ordercloud-api/headstart#validating-setup) section above to walk through generating some sample data and testing each of the application. Access your applications from:
    - <http://buyer.headstart.localhost>
    - <http://seller.headstart.localhost>
    - <http://api.headstart.localhost>

## Git Flow

1. **Fork** the repo on GitHub
2. **Clone** the project to your own machine
3. **Commit** changes to your own branch
4. **Push** your work back up to your fork
5. Submit a **Pull request** so that we can review your changes
