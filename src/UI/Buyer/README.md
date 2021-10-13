## Building the Buyer App

### Install the Angular CLI
If you have not before - install the [Angular CLI](https://github.com/angular/angular-cli/wiki) globally on your machine with `npm install -g @angular/cli`

### Install and build the Headstart SDK
Both the buyer and the seller application rely on a shared SDK that lives in this codebase under src/UI/SDK. This SDK is used to interact with the middleware API. You'll need to install dependencies and build that project first.

1. Navigate to the `SDK` directory in src/UI/SDK
2. Install dependencies with `npm install`
3. Build the project with `npm run build` 

### Install and build the Buyer app

1. Navigate to the `Buyer` directory in src/UI/Buyer
2. Install dependencies with `npm install`
3. Fill out the [test app configuration](src/assets/appConfigs/defaultbuyer-test.json)
4. Run `npm run start` for a dev server. The app will automatically reload if you change any of the source files.

You can modify your local deployment by changing values in the [environment.local.ts](./src/environments/environment.local.ts) file to target a different buyer or use the locally hosted middleware API

## Considerations

If your middleware API isn't yet hosted you will need to update the [environment.local.ts](./src/environments/environment.local.ts) to target the locally hosted middleware API. Set `useLocalMiddleware` to true and ensure `localMiddlewareURL` is the path your server is listening on.
