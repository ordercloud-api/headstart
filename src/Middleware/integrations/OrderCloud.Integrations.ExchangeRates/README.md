# Exchange Rates Integration for Headstart

## Scope of this integration

### Overview
This integration is responsible for dynamically updating pricing from a base currency (product.xp.Currency) to a specific user's currency. Note, that this is all calculated on the fly, there is no chance to manually set price for certain currencies. If that functionality is desired look to implement [Multi currency in OrderCloud](https://ordercloud.io/knowledge-base/locale-how-to-globalize-your-ecommerce)


### Defining Currency for a Product
The base currency for products is inherited from supplier.xp.Currency, and is USD otherwise

### Defining Currency for a User
We are storing xp.Country on the buyer location (usergroup). Any user belonging to that buyer location will then inherit the country, and from there we can determine which currency to use. If a user does not belong to a location, or that location does not have a country set, then you can expect to see an error on product list calls (where the pricing is updated)

### Updating Exchange Rates
The exchangeratesapi will only be called once initially and then stores the results in blob storage for future lookup. It does not handle updating exchange rates after that so it is your responsibility to add that feature if desired. For example you may choose to build an azure function that retrieves new exchange rates every day, or perhaps every week. You can call ICurrencyConversionCommand.Update for the update to occur.

## Enabling Integration
1. Create a free account on [exchangeratesapi.io](https://exchangeratesapi.io/)
2. Ensure StorageAccountSettings:ConnectionString is defined with a connection string to your Azure storage account. This is used to cache exchange rates in blob storage after successfully retrieving from exchangeratesapi
3. Set `EnvironmentSettings:CurrencyConversionProvider` to ExchangeRates
4. Ensure `ExchangeRateSettings:ApiKey` is defined