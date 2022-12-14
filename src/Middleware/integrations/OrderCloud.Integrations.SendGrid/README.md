### SendGrid Integration for Headstart

## Scope of this integration

This integration is responsible for sending transactional emails related to commerce activities via SendGrid.

### Email Types

| Email Type                | Description                                                                                                                     |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| CriticalSupport           | sent to support when criticial failures occur that require manual intervention                                                  |
| LineItemStatusChange      | sent to the buyer user, seller user, and relevant supplier user when the status for line items on an order change               |
| NewUser                   | sent to the buyer user when their account is first created with username and instructions to set their password                 |
| OrderApproval             | sent to the approving buyer user when an order requires their approval                                                          |
| OrderSubmit               | sent to the buyer user when their order is submitted                                                                            |
| PasswordReset             | sent to the buyer user when requesting password reset                                                                           |
| ProductInformationRequest | sent to the supplier (supplier.xp.SupportContact.Email) when a buyer user requests more information about one of their products |
| QuoteOrderSubmit          | sent to the buyer user when their quote is submitted                                                                            |
| OrderReturn               | sent to buyer user and approving admin user as order return status changes                                                      |

### Managing Message Senders

This integration is using a combination of [OrderCloud Message Senders](https://ordercloud.io/knowledge-base/message-senders) and custom triggered emails in app code to send emails. For any message sender triggered email type (NewUser, OrderApproval, OrderSubmit, and PasswordReset) you can define more granularly to whom these emails should be sent to by party (Buyer, Seller, or Supplier). To do this manage message senders in the OrderCloud portal and toggle on/off message types.

## Enabling Integration

1.  Set `EnvironmentSettings:EmailServiceProvider` to SendGrid
2.  Ensure `EnvironmentSettings:ApiKey` and `SendgridSettings:FromEmail` are defined in your app settings
3.  Ensure for each email type that you want to send that `{emailtype}TemplateID` is defined in app settings. You can use [these default templates](./assets/templates/email) as a starting point but will want to update the contact email and may want to add a company banner
4.  Deploy your middleware application. Emails won't work until the first deployment because there needs to be a publicly accessible endpoint that OrderCloud can send event information to.
