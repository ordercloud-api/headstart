## Building the Buyer App

1. If you have not before, install the [Angular CLI](https://github.com/angular/angular-cli/wiki) globally on your machine with `npm install -g @angular/cli`

2. Navigate to the `buyer` Directory with `cd src/UI/Buyer`

3. Install dependencies with `npm install`
4. Fill out the [test app configuration](rc/assets/appConfigs/defaultbuyer-test.json)

5. Run `npm run start` for a dev server. The app will automatically reload if you change any of the source files.

You can modify your local deployment by changing values in the [local.environment.ts](./src/environments/environment.local.ts) file to target a different buyer or use the locally hosted middleware API

### Considerations

- If your middleware API isn't yet hosted you will need to update the [local.environment.ts](./src/environments/environment.local.ts) to target the locally hosted middleware API. Set `useLocalMiddleware` to true and ensure `localMiddlewareURL` is the path your server is listening on.
