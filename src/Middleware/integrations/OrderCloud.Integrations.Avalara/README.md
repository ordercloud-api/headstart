# Avalara Integration For OrderCloud Headstart

## Scope of this integration
This integration calculates sales tax for an Order using the Avalara AvaTax API. It is part of the open source Headstart project, which provides a complete, opinionated OrderCloud solution. It conforms to the [ITaxCalculator](../ordercloud.integrations.library/interfaces/ITaxCalculator.cs) interface. 

Use Cases:
- Sales Tax Estimate
- Committed Transactions

## Avalara Basics 
[Avalara](https://www.avalara.com/us/en/index.html) tax automation software works together to create a holistic compliance platform. Products are available as a suite or as stand-alone options to help you customize based on your business needs. Our innovative, cloud-based sales tax calculation program, AvaTax, determines and calculates the latest rates based on location, item, legislative changes, regulations, and more. Customize your tax calculation solution to pay for what’s right for your business.

## Sales Tax Estimate
The sales tax cost on an Order is first calculated in checkout after shipping selections are made and before payment. Following that, they are updated whenever the order is changed. 

**Avalara Side -** Get a tax estimate by calling Avalara's [create transaction endpoint](https://developer.avalara.com/api-reference/avatax/rest/v2/methods/Transactions/CreateTransaction) with `type` set to `SalesOrder`.

**OrderCloud Side -** This integration should be triggered by the **`OrderCalculate`** Checkout Integration Event. Learn more about [checkout integration events](https://ordercloud.io/knowledge-base/order-checkout-integration); 

## Committed Transactions
A taxable transaction is committed to avalara asynchronously shortly following order submit. This enables businesses to easily file sales tax returns. OrderCloud guarantees the submitted order details provided will be unchanged since the most recent tax estimate displayed to the user.

**Avalara Side -** Commit a transaction in Avalara by calling the  [create transaction endpoint](https://developer.avalara.com/api-reference/avatax/rest/v2/methods/Transactions/CreateTransaction) with `type` set to `SalesInvoice`.

**OrderCloud Side -** This integration should be triggered by the **`PostOrderSubmit`** Checkout Integration Event. Learn more about [checkout integration events](https://ordercloud.io/knowledge-base/order-checkout-integration); 

## Steps to use
- Set up the headstart application. This is process is throughly documented [here](https://github.com/ordercloud-api/headstart#initial-setup).
- Sign up for Avalara, login and visit [License and API keys](https://integrations.avalara.com/#/software-keys) to get connection settings.
- Set environment variables required for Avalara. If you follow the headstart set up process, env vars are stored in an Azure Config.   
```
AvalaraSettings:BaseApiUrl  ("https://sandbox-rest.avatax.com/api/v2" or "https://rest.avatax.com/api/v2")
AvalaraSettings:AccountID
AvalaraSettings:LicenseKey    
AvalaraSettings:CompanyCode    
AvalaraSettings:CompanyID    
```
- Set an environment variable to indicate you want to use Avalara for tax calculation.
```
EnvironmentSettings:TaxProvider=Avalara
```
- Redeploy your middleware and on the storefront, go through checkout pausing before entering payment. You will see tax calculated by Avalara!

