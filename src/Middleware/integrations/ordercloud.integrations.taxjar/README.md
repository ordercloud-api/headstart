# Taxjar Integration For OrderCloud Headstart

## Taxjar Basics 
TaxJar is reimagining how businesses manage sales tax compliance. Our cloud-based platform automates the entire sales tax life cycle across all of your sales channels — from calculations and nexus tracking to reporting and filing. With innovative technology and award-winning support, we simplify sales tax compliance so you can grow with ease.

## Scope of this integration
This integration calculates tax on an OrderCloud Order using the TaxJar API. It is part of the open source Headstart project, which provides a complete, opinionated OrderCloud solution. It conforms to the [ITaxCalculator](../ordercloud.integrations.library/interfaces/ITaxCalculator.cs) interface, which exposes two functionalites: a tax estimate and a commited transaction. 
### Tax Estimate
A tax estimate is calculated in checkout after shipping selections are made and before payment. Following that, tax is updated whenever the order is changed. This behavior is triggered by the OrderCalculate Checkout Integration Event. Multiple requests are made to Taxjar's [calculate tax endpoint](https://developers.taxjar.com/api/reference/#post-calculate-sales-tax-for-an-order). Multiple because Taxjar's order model is limited to 1 shipping address and an OrderCloud order can contain LineItems shipping to different addresses. A TaxJar order request is made for each OrderCloud LineItem and each ShipEstimate.

### Commited Transaction 
A transaction is commited to TaxJar asynchronously directly following order submit. OrderCloud guareentees that the submitted order details provided will be unchanged since the most recent tax estimate displayed to the user. This request is triggered by the PostOrderSubmit Checkout Integration Event. Multiple requests are made to Taxjar's [create order endpoint](https://developers.taxjar.com/api/reference/#post-create-an-order-transaction). The TaxJar transactionId will look like `OrderID:|{orderID}|LineItemID:|{lineItemID}` or `OrderID:|{orderID}|ShipEstimateID:|{shipEstimateID}`.

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

