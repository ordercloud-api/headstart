# Vertex Integration For OrderCloud Headstart

## Vertex Basics 
Vertex Cloud is a cloud-based sales and use tax solution that is built on industry-leading Vertex software. Vertex Cloud integrates with leading e-commerce platforms and mid-market ERP systems. Customers can use Vertex Cloud to manage complex sales and use tax across multiple jurisdictions. Vertex Cloud provides tax calculations and signature-ready PDF returns in one comprehensive solution.

## Scope of this integration
This integration calculates tax on an OrderCloud Order using the Vertex Cloud [Tax Calculate for Sellers API endpoint](https://developer.vertexcloud.com/api/docs/#operation/Sale_Post). It is part of the open source Headstart project, which provides a complete, opinionated OrderCloud solution. It conforms to the [ITaxCalculator](../ordercloud.integrations.library/interfaces/ITaxCalculator.cs) interface, which exposes two functionalites: a tax estimate and a commited transaction. 
### Tax Estimate
A tax estimate is calculated in checkout after shipping selections are made and before payment. Following that, they are updated whenever the order is changed. This behavior is triggered by the OrderCalculate Checkout Integration Event. The request to vertex indicates it is an estimate by providing a `saleMessageType` of `QUOTATION`.  

### Commited Transaction 
A transaction is commited to vertex asynchronously directly following order submit. OrderCloud guareentees the submitted order details provided will be unchanged since the most recent tax estimate displayed to the user. This request is triggered by the PostOrderSubmit Checkout Integration Event. The Vertex `saleMessageType` is `INVOICE`.

## Steps to use
- Set up the headstart application. This is process is throughly documented [here](https://github.com/ordercloud-api/headstart#initial-setup).
- Sign up for vertex and login to the [Vertex Portal](https://portal.vertexsmb.com/Home) to get the credentials required in the next step.  
- Set environment variables required for Vertex authentication. See the vertex [authentication guide](https://developer.vertexcloud.com/access-token/). If you follow the headstart set up process env vars are stored in an Azure Config.   
	```VertexSettings:CompanyName
	VertexSettings:ClientID
	VertexSettings:ClientSecret
	VertexSettings:Username    
	VertexSettings:Password```
- Set an environment variable to indicate you want to use Vertex for tax calculation.
	```EnvironmentSettings:TaxProvider=Vertex```
- Redeploy your middleware and on the storefront, go through checkout pausing before entering payment. You will see tax calculated by Vertex!

