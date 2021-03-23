# Headstart

Welcome! The purpose of this project is to give you and your business a "headstart" to building an e-commerce solution on OrderCloud. This is a complete and opinionated solution including three main parts:

1. [Middleware](./src/Middleware/README.md) - The backend written in ASP.NET Core. Extends and enhances the capabilities of OrderCloud by integrating with best-of-breed services.
2. [Buyer](./src/UI/Buyer/README.md) - The frontend buyer application written in Angular. This includes the entire shopping experience from the perspective of a buyer user.
3. [Seller](./src/UI/Seller/README.md) - The frontend admin application written in Angular. This includes everything needed to manage the data in your buyer application(s).

## Initial Setup

There are some tasks that must be completed before you can get an instance of Headstart running. This section will walk you through each of them.

### Accounts

This solution relies on various third-party services and credentials for those services. You should have a set of test credentials as well as production credentials. Start by creating an account for all of the services listed.

1. [Avalara](https://www.avalara.com/us/en/get-started/get-started-b.html?adobe_mc_ref=https%3A%2F%2Fwww.avalara.com%2Fus%2Fen%2Findex.html) - Tax calculation
2. [CardConnect](https://cardconnect.com/signup) - Credit card payment processor
3. [EasyPost](https://www.easypost.com/signup) - Shipping estimates
4. [SmartyStreets](https://smartystreets.com/pricing) - Address validation
5. [Sendgrid](https://signup.sendgrid.com/) - Transactional emails **(Optional but emails won't work until set up)**
6. [Zoho](https://www.zoho.com/signup.html) - ERP **(Optional)**

### Provisioning Azure Resources

[App Service](https://docs.microsoft.com/en-us/azure/app-service/overview) - you'll need at least one app service to host the middleware API. For simplicity, we also set up one for each Buyer & Seller application though since they are static sites you have a variety of options at your disposal for how to host those.

[Azure Cosmos Database](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction) - While we use the OrderCloud API to host all e-commerce data this is a complete solution that requires handling data that doesn't natively exist as part of OrderCloud API. Some examples are report templates and product history. To that end, we are using Cosmos as our DB of choice. You will need one database per environment (we recommend three environments: Test, UAT, and Production)

[Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview) - This is an optional but highly recommended addition and will actually show up as an option when adding an app service. There is some additional configuration if you want to track the frontend. Look at the frontend app configs to provide your `appInsightsInstrumentationKey`

[Storage Account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal) - Provides all of azure data storage objects. Used to store currency conversions and translation tables. You will need a storage account for each environment (we recommend three environments: Test, UAT, and Production)

[Azure App Configuration](https://docs.microsoft.com/en-us/azure/azure-app-configuration/overview) - Used to store sensitive app settings that are consumed by the backend middleware application. We've defined [a template for you](./src/Middleware/src/Headstart.Common/AppSettingConfigTemplate.json) with the settings that are used in this application. You can fill out the template and then use Azure's import functionality to easily import it into your app configuration resource. For more detail on what each setting means check out [our readme](./src/Middleware/src/Headstart.Common/AppSettingsReadme.md). 

You will need an azure app configuration for each environment (we recommend three environments: Test, UAT, and Production)

In order for you application to consume the settings, you'll need to define the environment variable `APP_CONFIG_CONNECTION` whose value should be the connection string (read-only) to your azure app configuration.

- For **local** development - In Visual Studio right-click the Headstart.API project and go to Properties -> Debug -> Environment Variables.
- For **hosted** apps - In Azure navigate to your app service. Go to the correct deployment slot, and go to Settings -> Configuration -> New application setting

It is also possible to consume app settings from a JSON file while developing locally. Simply add an `appSettings.json` file to the root of the Headstart.API project. Settings defined here are applied after and override any settings in the azure app configuration.

### Seeding OrderCloud Data

This solution depends on a lot of data to be initialized in a particular way. To make it easy when starting a new project we've created an endpoint that does this for you. Just call it with some information, wait a few seconds, and presto: You'll have an organization that is seeded with all the right data to get you started immediately.

> Note: Before starting this step make sure your azure app configuration is filled out almost completely. The only things that won't be filled out yet are: `OrderCloudSettings:MiddlewareClientID` and `OrderCloudSettings:MiddlewareClientSecret`. These will be returned on a successful seeding so that you can update your app settings.

Detailed Steps:

1. Sign in to the [ordercloud portal](https://portal.ordercloud.io/)
2. Create a new organization in the portal if you don't already have one.
3. Find your organization and save the unique identifier this is your SellerID in step 6.
4. Follow the instructions [here](./src/Middleware/README.md) to start your server locally
5. Download and open [Postman](https://www.postman.com/downloads/) so that you can make API calls to your local server
6. Make a POST to `/seed` endpoint with the body as defined [here](./src/Middleware/src/Headstart.Common/Models/Misc/EnvironmentSeed.cs)
7. A successful response will include:
   1. The middleware clientID and secret. Save these two values in your app configuration under `OrderCloudSettings:MiddlewareClientID` and `OrderCloudSettings:MiddlewareClientSecret`
   2. The buyer clientID. Follow the instructions in [frontend configuration](#frontend-configuration) and set it in the buyer config `clientID`
   3. The seller clientID. Follow the instructions in [frontend-configuration](#frontend-configuration) and set it in the seller config `clientID`

### Sendgrid (Email) Configuration

1. Ensure `SendgridSettings:ApiKey` and `SendgridSettings:FromEmail` are defined in your app settings
2. Ensure for each email type that you want to send that `{emailtype}TemplateID` is defined in app settings. You can use [these default templates](./src/Middleware/src/Headstart.Common/Assets/EmailTemplates) as a starting point but will want to update the contact email and may want to add a company banner. See the table below for a description of each email type.
3. Deploy your middleware application. Emails won't work until the first deployment because there needs to be a publicly accessible endpoint that OrderCloud can send event information to. 

|        Email Type         | Description                                                                                                                     |
| :-----------------------: | ------------------------------------------------------------------------------------------------------------------------------- |
|      CriticalSupport      | sent to support when criticial failures occur that require manual intervention                                                  |
|   LineItemStatusChange    | sent to the buyer user, seller user, and relevant supplier user when the status for line items on an order change               |
|          NewUser          | sent to the buyer user when their account is first created with username and instructions to set their password                 |
|       OrderApproval       | sent to the approving buyer user when an order requires their approval                                                          |
|        OrderSubmit        | sent to the buyer user when their order is submitted                                                                            |
|       PasswordReset       | sent to the buyer user when requesting password reset                                                                           |
| ProductInformationRequest | sent to the supplier (supplier.xp.SupportContact.Email) when a buyer user requests more information about one of their products |
|     QuoteOrderSubmit      | sent to the buyer user when their quote is submitted                                                                            |
|                           |                                                                                                                                 |

### Frontend Configuration

Your backend middleware is configured and all your resources have been provisioned and your data is seeded. Now we'll need to configure your buyer and seller applications. You can have any number of configurations each representing a deployment. By default, we've created three such deployments one for each environment.

- [Buyer](./src/UI/Buyer/src/assets/appConfigs)
- [Seller](./src/UI/Seller/src/assets/appConfigs)

### Validating Setup

Once your organization has been seeded and your applications are configured you'll want to make sure everything is working well.

First, fire up all three applications
* [Middleware](./src/Middleware/README.md)
* [Seller](./src/UI/Seller/README.md)
* [Buyer](./src/Buyer/README.md)

Then, follow these steps. Our goal is to get to the point where our buyer user can add a product to their cart.
1. Open your admin application and sign in with your initial admin user credentials
2. Navigate to the "Suppliers" tab - suppliers are responsible for creating products so we will need to do that first
3. Click "Create New supplier". Fill in the required details and create a new supplier.
4. Within that supplier click "Supplier Addresses"
5. Click "Create New Supplier Address", fill in the details, and create a new address. Use an actual address or it will fail address validation
6. Go back to the supplier detail page and then click "Users"
7. Note that there is already a user here with an ID that starts with `dev_` this user exists so that the middleware can act on behalf of it if needed to act as that supplier. Do not delete this user.
8. Click "Create New User" and fill out the details. Make sure to set "Active" true and for now assign all permissions to the user.
9. If you have [emails set up](#sendgrid-email-configuration) you can use the "forgot password" feature to set a password for the supplier user otherwise set the password for that user in the portal
10. Next click on "Buyers"
11. Click "Default HeadStart Buyer"
12. Click "Catalogs"
13. Click "Create New Catalog" this will ultimately be the container that holds our products
14. Go back to the buyer detail page
15. Click "Buyer Locations"
16. Click "Create New Buyer Location" - fill in the details and use a real address so it passes valiation. At the bottom assign our previously created Catalog to that buyer location.
17. Go back to the buyer detail page
18. Click "Users"
19. Click "Create a New User" - make sure Active is set to true
20. If you have [emails set up](#sendgrid-email-configuration) you can use the "forgot password" feature to set a password for the buyer user on your buyer application otherwise set the password for that user in the portal
21. At this point we've done all we can as a seller user. We now need to log in to the admin application as a supplier user to create our product
22. Log out of seller application
23. Log in as your previously created supplier user
24. Click on "Products"
25. Click "Create New Product" - select "Standard Product". Enter required fields - sections marked with a red asterisk in Product tab and Pricing tab
26. Now that the product is created our seller needs to define the visibility
27. Log out of seller application
28. Log in as your initial admin user
29. Click on "Products"
30. Click on the previously created product
31. Click the "Buyer Visibility" tab
32. Click "Edit" on "Default Headstart Buyer"
33. Click the toggle to make the product visible to our previously created catalog
34. Click "Save"
35. Go to your buyer application
36. Sign in as your buyer user
37. Click on "Products"
38. You should see your product in the list, click it.
39. Click "Add to cart"
40. Click the cart icon

Congrats! Hopefully you didn't get any errors and understand a little bit more about how everything is connected. If you did encounter errors please capture the details and submit an issue on Github.

## Deploying your application
We recommend using [Azure Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/get-started/what-is-azure-pipelines?view=azure-devops) for building and releasing your code. 

### Build Phase
We've included a [YAML build configuration file](./azure-pipelines.yml) that tells Azure how to:
* Build the middleware
* Run the middleware tests
* Build/Publish both the admin and buyer frontend
* Publish all the artifacts

The only small change you will need to make is to update the "Get Build Number" step and update the URL to point to your app's middleware. Information on how to use the YAML file can be found [here](https://docs.microsoft.com/en-us/azure/devops/pipelines/customize-pipeline?view=azure-devop)

### Release Phase
This project follows the [build once, deploy many](https://earlyandoften.wordpress.com/2010/09/09/build-once-deploy-many/) pattern to increase consistency for releases across environments. It accomplishes this by injecting the app configuration for the desired environment during the release phase as you'll see in the following steps. The example here is to deploy the test environment but the same process can be modified slightly for the other environments

* Configure Buyer for the environment - From the buyer artifacts directory run `node inject-css defaultbuyer-test && node inject-appconfig defaultbuyer-test`. This will inject both the css and the app settings for defaultbuyer-test environment
* Deploy Buyer - Deploy buyer artifacts to your buyer app service test slot
* Configure Seller for the environment - From the seller artifacts directory run `node inject-appconfig defaultadmin-test`. This will inject the app settings for defaultadmin-test.
* Deploy Seller - Deploy seller artifacts to your seller app service test slot
* Deploy Middleware - Deploy middleware artifacts to your middleware app service test slot. Make sure the environment variable `APP_CONFIG_CONNECTION` is set on your app service and points to the connection string to your azure app configuration

## Git Flow

1.  **Fork** the repo on GitHub
2.  **Clone** the project to your own machine
3.  **Commit** changes to your own branch
4.  **Push** your work back up to your fork
5.  Submit a **Pull request** so that we can review your changes
