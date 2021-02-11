# Headstart

Welcome! The purpose of this project is to give you and your business a "headstart" to building an ecommerce solution on OrderCloud. This is a complete and opinionated solution including three main parts:

1. [Middleware](./src/Middleware/README.md) - The backend written in ASP.NET Core. Extends and enhances the capabilities of OrderCloud by integrating with best of breed services.
2. [Buyer](./src/UI/Buyer/README.md) - The frontend buyer application written in Angular. This includes the entire shopping experience from the perspective of a buyer user.
3. [Seller](./src/UI/Seller/README.md) - The frontend admin application written in Angular. This includes everything needed to manage the data in your buyer application.

## Initial Setup

There are some tasks that must be completed before you can get an instance of Headstart running. This section will walk you through each of them.

### Accounts

This solution relies on various third party services and credentials for those services. You should have a set of test credentials as well as production credentials. Start by creating an account for all of the services listed.

1. [Avalara](https://www.avalara.com/us/en/get-started/get-started-b.html?adobe_mc_ref=https%3A%2F%2Fwww.avalara.com%2Fus%2Fen%2Findex.html) - Tax calculation
2. [CardConnect](https://cardconnect.com/signup) - Credit card payment processor
3. [Sendgrid](https://signup.sendgrid.com/) - Transactional emails
4. [EasyPost](https://www.easypost.com/signup) - Shipping estimates
5. [SmartyStreets](https://smartystreets.com/pricing) - Address validation

### Provisioning Azure Resources

[App Service](https://docs.microsoft.com/en-us/azure/app-service/overview) - you'll need at least one app service to host the middleware API. For simplicity we also set up one for each Buyer & Seller applications though since they are static sites you have a variety of options at your disposal for how to host those.

[Azure Cosmos Database](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction) - While we use the OrderCloud API to host all ecommerce data this is a complete solution that requires handling data that doesn't natively exist as part of OrderCloud API. Some examples are: report templates and product history. To that end, we are using Cosmos as our DB of choice. You will need one database per environment (we recommend three environments: Test, UAT, and Production)

[Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview) - This is an optional but highly recommended addition and will actually show up as an option when adding an app service. There is some additional configuration if you want to track the frontend. Look at the frontend app configs to provide your `appInsightsInstrumentationKey`

[Storage Account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal) - Provides all of azure data storage objects. Used to store currency conversions and translation tables. You will need a storage account for each environment (we recommend three environments: Test, UAT, and Production)

[Azure App Configuration](https://docs.microsoft.com/en-us/azure/azure-app-configuration/overview) - Used to store sensitive app settings that are consumed by the backend middleware application. App configs can be imported via a JSON file and we've defined [a template for you](./src/Middleware/src/Headstart.Common/AppSettingConfigTemplate.json) with the settings that are used in this application. For more detail on what each setting is, check out [our readme](./src/Middleware/src/Headstart.Common/AppSettingsReadme.md).

In order for you application to consume the settings you'll need to define the environment variable `APP_CONFIG_CONNECTION` who's value should be the connection string (readonly) to your azure app configuration.

You will need an azure app configuration for each environment (we recommend three environments: Test, UAT, and Production)

### Creating Azure Pipelines

[Azure Pipeline](https://docs.microsoft.com/en-us/azure/devops/pipelines/get-started/what-is-azure-pipelines?view=azure-devops) - In order to deploy the code, you will need to set up a pipeline within Azure. To get setup and running quickly, we have included a YAML file that will build the middleware, run middleware tests, build/publish both UIs, along with publishing all the artifacts. [The template](./docs/Headstart-Azure-Pipeline.yml) is located within the docs folder in the root of the repo. The only change that is needed is within the "Get Build Number" job, update the URL to point to your app's middleware. Information on how to use the YAML file can be found [here](https://docs.microsoft.com/en-us/azure/devops/pipelines/customize-pipeline?view=azure-devops).

### Seeding OrderCloud Data

This solution depends on a lot of data to be initialized in a particular way. To make it easy when starting a new project we've created an endpoint that does this for you. Just call it with some information, wait a few seconds and presto: You'll have an organization that is seeded with all the right data to get you started immediately.

> Note: Before starting this step make sure your azure app configuration is filled out almost completely. The only things that won't be filled out yet, but should be after this are: `OrderCloudSettings:MiddlewareClientID` and `OrderCloudSettings:MiddlewareClientSecret`

Detailed Steps:

1. Sign in to the [ordercloud portal](https://portal.ordercloud.io/)
2. Create a new organization in the portal if you dont already have one.
3. Find your organization and save the unique identifier this is your SellerID in step 6.
4. Follow the instructions [here](./src/Middleware/README.md) to start your server locally
5. Download and open [Postman](https://www.postman.com/downloads/) so that you can make API calls to your local server
6. Make a POST to `/seed` endpoint with the body as defined [here]('./src/Middleware/src/Headstart.Common/Models/Misc/EnvironmentSeed.cs)
7. Validate data has been seeded in your organization. You should be able to list api clients. You'll want to find the api client with the name `Middleware Integrations` and set its ID in your azure app configuration as `OrderCloudSettings:MiddlewareClientID` and the secret to `OrderCloudSettings:MiddlewareClientSecret`

### Frontend Configuration

Your backend middleware is configured and all your resources have been provisioned and your data is seeded. Now we'll need to configure your buyer and seller applications. You can have any number of configurations each representing a deployment. By default we've created three such deployments one for each environment.

- [Buyer](./src/UI/Buyer/src/assets/appConfigs)
- [Seller](./src/UI/Seller/src/assets/appCOnfigs)

### Validating Setup

<!-- TODO: make this better, can probably do some simple product visibility stuff -->

Once your organization has been seeded and your applications are configured you'll want to make sure everything is working well.

For security reasons we don't create an admin user for you during the seeding process so you'll want to start by creating your first admin user in [the portal](https://portal.ordercloud.io/). Make sure you set the user to `"Active": true` and pick a strong password.

Next, fire up your server following the instructions [here](./src/Middleware/README.md). At the same time, build your seller application locally by following the steps [here](./src/UI/Seller/README.md). You should be able to log in as your admin user, create a buyer organization as well as a buyer user.

Once you've created a buyer user you can fire up your buyer application locally by following the instructions [here](./src/Buyer/README.md). You will need to use the forgot password feature before logging in for the first time. You will receive an email and once your password has been reset you should be able to log in.

## Git Flow

1.  **Fork** the repo on GitHub
2.  **Clone** the project to your own machine
3.  **Commit** changes to your own branch
4.  **Push** your work back up to your fork
5.  Submit a **Pull request** so that we can review your changes
