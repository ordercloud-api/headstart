# Getting Started
1. Open the Headstart.sln in Visual Studio
2. Right click on the Headstart.API project and click on "Properties"
3. Go to the "Debug" panel
4. Create a new profile and call it "Test"
5. Set Launch to Project
6. Add a new Environment Variable called APP_CONFIG_CONNECTION who's value is the connection string to your [azure app configuration](https://docs.microsoft.com/en-us/azure/azure-app-configuration/overview)
7. Repeat steps 4-6 for the other environments "UAT" and "Production"
8. Save your profiles now you should be able to select it and run the project locally

# Importing the Middleware API in Postman

In Postman, using a existing or new Workspace:
1. Click on **Import**.
2. Select the **Link** tab.
3. Enter the url *\<MiddlewareUrl\>*/swagger/v1/swagger.json, e.g. https://localhost:5001/swagger/v1/swagger.json.
4. Click **Continue**.
5. Leave the import configuration with the default settings (or change them if you are advanced user).
6. Finalise the import by clicking **Import**.
