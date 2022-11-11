### CardConnect Integration for Headstart

## Scope of this integration

This integration is responsible for authorization of payments on submit and optionally the capture of payment after submit

## Authorizing payments
Payment authorization happens during checkout immediately before order submit, if authorization fails then the user is not allowed to submit the order. Please note this is an auth only flow, no payments are captured on order submit. General best practices around credit card payments dictates that capture should not happen until the order has been confirmed (usually when the order ships)

## Payment Capture Job

There is an optional [payment capture job](../../src/Headstart.Jobs/Jobs/PaymentCaptureJob.cs) that can be enabled (azure function) and by default runs daily and will capture any authorized credit card payments. This may be useful for digital products or any other product than can be confirmed the same day. For all other instances we recommend waiting until the order has shipped. To enable this deploy the Headstart.Jobs project and ensure that `JobSettings:ShouldCaptureCreditCardPayments` is set to true

## Multiple currencies
By default, this integration only supports payments with one currency. This is due to a limitation in CardConnect whereby you can only have one currency per merchant account. If your solution needs to handle more than one currency then you will need to create a merchant for each currency you wish to support and additionally modify the code to switch to that merchant/credentials based on currency code. 

## Enabling Integration

1. Set `EnvironmentSettings:PaymentProvider` to `CardConnect`
2. Ensure `CardConnectSettings:Authorization` and `CardConnectSettings:MerchantID` are defined in your app settings
3. Optionally enable payment capture job by deploying Headstart.Jobs and setting `JobSettings:ShouldCaptureCreditCardPayments` to true
