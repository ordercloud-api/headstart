# Ordercloud HeadStart

 HeadStart is functional ecommerce site built with Ordercloud that you can start customizing today. This package is a client library for the [Head Start API](https://marketplace-api-test.azurewebsites.net/index.html).

## ⚙️ Installation

```shell
npm install @ordercloud/headstart-sdk --save
```

## Usage


```typescript
import { HeadStartSDK } from '@ordercloud/headstart-sdk';

let page = 1;
let pageSize = 100;
let search = "boots"
let taxCategory = "P0" // tangible personal property  

let taxCodes: ListPage<MarketplaceTaxCode> = await MarketplaceSDK.TaxCodes.GetTaxCodes({ filters: { Category: taxCategory }, search, page, pageSize });
```

## 🔐 Authentication

Marketplace Authentication uses tokens from the Ordercloud auth server 

```javascript
// Login 
HeadStartSDK.Tokens.SetAccessToken("ordercloud-access-token");

// Logout
HeadStartSDK.Tokens.RemoveAccessToken();
```

## 🔍 Filtering

All of the [filtering options](https://ordercloud.io/features/advanced-querying#filtering)  you love from the API are available through the SDK as well. Simply build an object that matches the model of the item you're filtering on where the `value` is the value you'd like to filter on.

Let's run through a couple scenarios and what the call will look like with the SDK:

My products where `xp.Featured` is `true`

```javascript
Me.ListProducts({ filters: { xp: { Featured: true } } })
  .then(productList => console.log(productList));
```

My orders submitted after April 20th, 2018

```javascript
Me.ListOrders({ filters: { DateSubmitted: '>2018-04-20' } })
  .then(orderList => console.log(orderList))
```

Users with the last name starting with Smith:

```javascript
Users.List('my-mock-buyerid', { filters: { LastName: 'Smith*' } })
  .then(userList => console.log(userList));
```

Users with the last name starting with Smith *or* users with the last name *ending* with Jones

```javascript
Users.List('my-mock-buyerid', { filters: { LastName: 'Smith*|*Jones' } })
  .then(userList => console.log(userList));
```

Products where xp.Color is not red *and* not blue

```javascript
Products.List({ filters: { xp: { Color: ['!red', '!blue'] } } })
    .then(productList => console.log(productList));
```

And of course you can mix and match filters to your heart's content.

## 📄 License

Marketplace Javascript SDK is an open-sourced software licensed under the [MIT license](./LICENSE).

## 🤝 Contributing

Check out our [Contributing](./readmes/CONTRIBUTING.md) guide.

## 🆘 Getting Help

If you're new to OrderCloud, exploring the [documentation](https://developer.ordercloud.io/documentation) is recommended, especially the [Intro to OrderCloud.io](https://developer.ordercloud.io/documentation/platform-guides/getting-started/introduction-to-ordercloud) and [Quick Start Guide](https://developer.ordercloud.io/documentation/platform-guides/getting-started/quick-start-guide). When you're ready to dive deeper, check out the [platform guides](https://developer.ordercloud.io/documentation/platform-guides) and [API reference](https://developer.ordercloud.io/documentation/api-reference).

For programming questions, please [ask](https://stackoverflow.com/questions/ask?tags=ordercloud) on Stack Overflow.

To report a bug or request a feature specific to the SDK, please open an [issue](https://github.com/ordercloud-api/ordercloud-javascript-sdk/issues/new).
