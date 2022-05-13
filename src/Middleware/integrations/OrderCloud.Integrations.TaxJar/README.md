# Taxjar Integration For OrderCloud Headstart

## Scope of this integration
This integration calculates sales tax for an Order using the TaxJar API. It is part of the open source Headstart project, which provides a complete, opinionated OrderCloud solution. It conforms to the [ITaxCalculator](../ordercloud.integrations.library/interfaces/ITaxCalculator.cs) interface. 

Use Cases:
- Sales Tax Estimate
- Finialized Order Forwarding 

## Taxjar Basics 
[TaxJar](https://www.taxjar.com/) is reimagining how businesses manage sales tax compliance. Our cloud-based platform automates the entire sales tax life cycle across all of your sales channels — from calculations and nexus tracking to reporting and filing. With innovative technology and award-winning support, we simplify sales tax compliance so you can grow with ease.

## Sales Tax Estimate
The sales tax cost on an Order is first calculated in checkout after shipping selections are made and before payment. Following that, they are updated whenever the order is changed. 

**TaxJar Side -** Multiple requests are made to Taxjar's [calculate tax endpoint](https://developers.taxjar.com/api/reference/#post-calculate-sales-tax-for-an-order). Multiple because Taxjar's order model is limited to 1 shipping address and an OrderCloud order can contain LineItems shipping to different addresses. A TaxJar order request is made for each OrderCloud LineItem and each ShipEstimate.

**OrderCloud Side -** This integration should be triggered by the **`OrderCalculate`** Checkout Integration Event. Learn more about [checkout integration events](https://ordercloud.io/knowledge-base/order-checkout-integration); 

## Order Forwarding 
A taxable transaction is committed to taxjar asynchronously shortly following order submit. This enables businesses to easily file sales tax returns. OrderCloud guarantees the submitted order details provided will be unchanged since the most recent tax estimate displayed to the user.

**TaxJar Side -** Multiple requests are made to Taxjar's [create order endpoint](https://developers.taxjar.com/api/reference/#post-create-an-order-transaction). The TaxJar transactionId will look like `OrderID:|{orderID}|LineItemID:|{lineItemID}` or `OrderID:|{orderID}|ShipEstimateID:|{shipEstimateID}`.

**OrderCloud Side -** This integration should be triggered by the **`PostOrderSubmit`** Checkout Integration Event. Learn more about [checkout integration events](https://ordercloud.io/knowledge-base/order-checkout-integration); 

## Steps to use
- Set up the headstart application. This is process is throughly documented [here](https://github.com/ordercloud-api/headstart#initial-setup).
- Sign up for TaxJar and visit [Account](https://app.taxjar.com/account#api-access) to get an ApiKey.  
- Set environment variables required for TaxJar. If you follow the headstart set up process, env vars are stored in an Azure Config.   
```
TaxJarSettings:ApiKey
TaxJarSettings:Environment    ("Sandbox" or "Production")
```
- Set an environment variable to indicate you want to use TaxJar for tax calculation.
```
EnvironmentSettings:TaxProvider=Taxjar
```
- Redeploy your middleware and on the storefront, go through checkout pausing before entering payment. You will see tax calculated by TaxJar!

