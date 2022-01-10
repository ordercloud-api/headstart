# Getting Started
1. Open the Headstart.sln in Visual Studio
2. Add a "local.settings.json" file at the room of Headstart.Jobs with the following content:
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AzureWebJobsDashboard": "UseDevelopmentStorage=true"
  },
  "Host": {
    "LocalHttpPort": 7071,
    "CORS": "*",
    "CORSCredentials": false
  },
  "ConnectionStrings": {

  }
}
3. Right click on the Headstart.Jobs project and click on "Properties"
4. Go to the "Debug" panel
5. Create a new profile and call it "Test"
6. Set Launch to Project
7. Add a new Environment Variable called `APP_CONFIG_CONNECTION` and set the value to the connection string from your [azure app configuration](https://docs.microsoft.com/en-us/azure/azure-app-configuration/overview)
8. Repeat steps 4-6 for the other environments "UAT" and "Production"
9. Save your profiles now you should be able to select them and run the project locally

# Debugging locally
1. Comment out any jobs you don't want to run. There seems to be a bug where they will run locally even if they shouldn't
2. Run the Headstart.Jobs project locally selecting a profile created in #getting-started
3. [Open Postman](https://www.postman.com/)
4. POST to http://localhost:7071/admin/functions/{function_name} with a JSON body {"input": ""}
5. Click Send

# Deploying
1. Create azure functions service
2. Because of how the settings in Jobs.cs are retrieved you'll need to add those settings specifically to the functions app service itself under "Configuration"
3. Add also the setting APP_CONFIG_CONNECTION so that it can retrieve all settings not in Jobs.cs
4. Build & Deploy your functions app
5. Naviate to "Functions" on your functions app, you should see the names of all the functions defined in Jobs.cs as well as inspect logs to ensure everything is running specifically.

# Accessing logs
1. Navigate within Azure > Function App to the [seb-jobs](https://portal.azure.com/#@four51.com/resource/subscriptions/736cd8bd-0185-4184-b3dd-8c372c076f3f/resourceGroups/Marketplace-winOS/providers/Microsoft.Web/sites/seb-jobs/appServices) azure function
2. Select the environment
	- Test: on the left hand menu click "Deployment Slots" and select "test"
	- Staging: on the left hand menu click "Deployment Slots" and select "staging"
	- Production: no action required, this is the default
3. On the left hand menu click "Functions"
4. Select the function you're interested in
5. On the left hand menu click "Monitor"

# Timer Syntax
Azure functions uses the [NCrontab library](https://github.com/atifaziz/NCrontab#ncrontab-crontab-for-net) under the hood. You may see examples in azure using the six-part format which allows you to represent seconds. Generally that's too granular for us to care about so prefer to use the five-part format that is accurate to just minutes. Its more commonly used and you can plug it into tools like [this online crontab calculator](https://crontab.guru/)

Just remember the time represented in crontab is UTC so you'll need to do a slight conversion to determine what is in CST (Subtract 6 hours)