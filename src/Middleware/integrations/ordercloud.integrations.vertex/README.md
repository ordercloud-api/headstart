# Vertex Integration For OrderCloud Headstart

## Scope of this integration
This integration calculates sales tax for an Order using the vertex cloud API. It is part of the open source Headstart project, which provides a complete, opinionated OrderCloud solution. It conforms to the [ITaxCalculator](../ordercloud.integrations.library/interfaces/ITaxCalculator.cs) interface. 

Use Cases:
- Sales Tax Estimate
- Finalized Order Forwarding 

## Vertex Basics 
[Vertex](https://www.vertexinc.com/) is a cloud or on premise **sales and use tax solution**. Vertex Cloud integrates with leading e-commerce platforms and mid-market ERP systems. Customers can use Vertex Cloud to manage complex sales and use tax across multiple jurisdictions. Vertex Cloud provides tax calculations and signature-ready PDF returns in one comprehensive solution.

## Sales Tax Estimate
The sales tax cost on an Order is first calculated in checkout after shipping selections are made and before payment. Following that, they are updated whenever the order is changed. 

**Vertex Side -** Get a tax estimate in Vertex Cloud by calling the [Tax Calculate for Sellers API endpoint](https://developer.vertexcloud.com/api/docs/#operation/Sale_Post) with `saleMessageType` set to `QUOTATION`.

**OrderCloud Side -** This integration should be triggered by the **`OrderCalculate`** Checkout Integration Event. Learn more about [checkout integration events](https://ordercloud.io/knowledge-base/order-checkout-integration); 

## Order Forwarding
A taxable transaction is committed to vertex asynchronously shortly following order submit. This enables businesses to easily file sales tax returns. OrderCloud guarantees the submitted order details provided will be unchanged since the most recent tax estimate displayed to the user.

**Vertex Side -** Commit a transaction in Vertex Cloud by calling the [Tax Calculate for Sellers API endpoint](https://developer.vertexcloud.com/api/docs/#operation/Sale_Post) with `saleMessageType` set to `INVOICE`.

**OrderCloud Side -** This integration should be triggered by the **`PostOrderSubmit`** Checkout Integration Event. Learn more about [checkout integration events](https://ordercloud.io/knowledge-base/order-checkout-integration); 

## Steps to use
- Set up the headstart application. This is process is throughly documented [here](https://github.com/ordercloud-api/headstart#initial-setup).
- Sign up for vertex and login to the [Vertex Portal](https://portal.vertexsmb.com/Home) to get the credentials required in the next step.  
- Set environment variables required for Vertex authentication. See the vertex [authentication guide](https://developer.vertexcloud.com/access-token/). If you follow the headstart set up process env vars are stored in an Azure Config.   
```
VertexSettings:CompanyName
VertexSettings:ClientID
VertexSettings:ClientSecret
VertexSettings:Username    
VertexSettings:Password
```
- Set an environment variable to indicate you want to use Vertex for tax calculation.
```
EnvironmentSettings:TaxProvider=Vertex
```
- Redeploy your middleware and on the storefront, go through checkout pausing before entering payment. You will see tax calculated by Vertex!
