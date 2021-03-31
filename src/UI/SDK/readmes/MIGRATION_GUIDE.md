# Migration Guide

The objective of this guide is to document the breaking changes and updates required to migrate from one major version to the next.

## version 3.x.x to version 4.x.x

* [Axios](https://www.npmjs.com/package/axios) is now a peer dependency. Peer dependencies are not installed automatically, they must be installed separately.

* Interacting directly with the SDK instance is no longer possible. Configuration of the sdk is now done via the `Configuration` service and setting tokens is done via the `Tokens` service. Setting a token in a browser environment will set the token in cookies, and on the server they will be stored on the sdk instance.

    Before:

    ```javascript
    const defaultClient = OrderCloud.Sdk.instance;

    // configuring baseApiPath and baseAuthUrl
    defaultClient.baseApiPath = 'https://marketplace-api-qa.azurewebsites.net';
    defaultClient.baseAuthPath = 'https://auth.ordercloud.io/oauth/token';

    // setting the token
    defaultClient.authentications['oauth2'].accessToken = 'my-token'; // setting token
    ```

    After:

    ```javascript
    Configuration.Set({
        baseApiUrl: 'https://marketplace-api-qa.azurewebsites.net',
        baseAuthUrl: 'https://auth.ordercloud.io/oauth/token'
    })

    Tokens.SetAccess('my-token');
    ```

* The `As()` method used for impersonation has been moved from being accessible from the sdk *to* each resource.

    Before:

    ```javascript
    OrderCloudSDK.As().Me.ListProducts();
    ```

    After:

    ```javascript
    OrderCloudSDK.Me.As().ListProducts

    // OR (if using selective imports)
    Me.As().ListProducts
    ```

* Sending filters on list calls has changed in order to provide better type support.

    Before:
    Filters were a shallow object where each key was the dot-referenced property to search on, and the value was the filter to apply.

    ```javascript
    Me.ListProducts({filters: { 'xp.Color': 'red' } })
    ```

    After:
    Filters match the shape of the item being searched. Nested properties are represented with nested objects instead of using dot-notation.

    ```javascript
    Me.ListProducts({filters: { xp: { Color: 'red' } } })
    ```

## version 2.x.x to version 3.x.x

* `ApiClient` renamed to `Sdk` to prevent name clash with new API resource [ApiClient](https://ordercloud.io/api-reference/seller/api-clients).

    Before:

    ```javascript
    const defaultClient = OrderCloud.ApiClient.instance;
    ```

    After:

    ```javascript
    const defaultClient = OrderCloud.Sdk.instance;
    ```

## version 1.x.x to version 2.x.x

* `searchOn` and `sortBy` now only accept a comma-delimited string. Previously accepted a comma-delimited string **or** an array of strings

* Renamed "Update" (used for PUT's) in favor of "Save" to clarify intent. For example `OrderCloudSDK.Orders.Update` now becomes `OrderCloudSDK.Orders.Save`. This is for *all* resources.
