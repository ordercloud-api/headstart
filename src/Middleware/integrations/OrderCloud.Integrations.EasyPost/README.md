### EasyPost Integration for Headstart

## Scope of this integration

This integration is responsible for retrieving shipping estimates during checkout based on product dimensions

## Shipping Providers

This integration can be configured to retrieve rates from either UPS or Fedex. To enable this functionality set `EasyPostSettings:USPSAccountId` or `EasyPostSettingsFedexAccountId`

## Enabling Integration

1. Set `EnvironmentSettings:ShippingProvider` to EasyPost 
2. Ensure `EasyPostSettings:ApiKey` and `EasyPostSettings:CustomsSigner` are defined
3. Ensure either `EasyPostSettings:USPSAccountId` or `EasyPostSettingsFedexAccountId` are defined
