using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Headstart.Common.Constants;
using Headstart.Common.Models.Misc;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Common.Settings;
using Headstart.Models;
using Headstart.Models.Headstart;
using Headstart.Models.Misc;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Emails;
using OrderCloud.SDK;
using SendGrid;
using SendGrid.Helpers.Mail;
using static OrderCloud.Integrations.SendGrid.SendGridModels;

namespace OrderCloud.Integrations.SendGrid
{
    public class SendGridService : IEmailServiceProvider
    {
        private readonly SendGridSettings sendgridSettings;
        private readonly UI uiSettings;
        private readonly IOrderCloudClient oc;
        private readonly ISendGridClient client;

        public SendGridService(SendGridSettings sendgridSettings, UI uiSettings, IOrderCloudClient ocClient, ISendGridClient client)
        {
            oc = ocClient;
            this.client = client;
            this.sendgridSettings = sendgridSettings;
            this.uiSettings = uiSettings;
        }

        public async Task SendQuoteRequestConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail)
        {
            var buyerTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
            {
                Data = SendgridMappers.GetQuoteOrderTemplateData(order, new List<HSLineItem> { lineItem }),
                Message = OrderSubmitEmailConstants.GetQuoteRequestConfirmationText(),
            };
            await SendSingleTemplateEmail(sendgridSettings?.FromEmail, buyerEmail, sendgridSettings?.QuoteOrderSubmitTemplateID, buyerTemplateData);
        }

        public async Task SendQuotePriceConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail)
        {
            var buyerTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
            {
                Data = SendgridMappers.GetQuoteOrderTemplateData(order, new List<HSLineItem> { lineItem }),
                Message = OrderSubmitEmailConstants.GetQuotePriceConfirmationText(),
            };
            await SendSingleTemplateEmail(sendgridSettings?.FromEmail, buyerEmail, sendgridSettings?.QuoteOrderSubmitTemplateID, buyerTemplateData);
        }

        public virtual async Task SendSingleTemplateEmail(string from, string to, string templateID, object templateData)
        {
            Require.That(templateID != null, new ErrorCode("SendgridError", "Required Sengrid template ID not configured in app settings", HttpStatusCode.NotImplemented));
            {
                var fromEmail = new EmailAddress(from);
                var toEmail = new EmailAddress(to);
                var msg = MailHelper.CreateSingleTemplateEmail(fromEmail, toEmail, templateID, templateData);
                var response = await client.SendEmailAsync(msg);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error sending sendgrid email");
                }
            }
        }

        public virtual async Task SendSingleTemplateEmailMultipleRcpts(string from, List<EmailAddress> tos, string templateID, object templateData)
        {
            Require.That(templateID != null, new ErrorCode("SendgridError", "Required Sengrid template ID not configured in app settings", HttpStatusCode.NotImplemented));
            {
                var fromEmail = new EmailAddress(from);
                var msg = MailHelper.CreateSingleTemplateEmailToMultipleRecipients(fromEmail, tos, templateID, templateData);
                var response = await client.SendEmailAsync(msg);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error sending sendgrid email");
                }
            }
        }

        public async Task SendSingleTemplateEmailMultipleRcptsAttachment(string from, List<EmailAddress> tos, string templateID, object templateData, CloudAppendBlob fileReference, string fileName)
        {
            Require.That(templateID != null, new ErrorCode("SendgridError", "Required Sengrid template ID not configured in app settings", HttpStatusCode.NotImplemented));
            {
                var fromEmail = new EmailAddress(from);
                var msg = MailHelper.CreateSingleTemplateEmailToMultipleRecipients(fromEmail, tos, templateID, templateData);
                using (var stream = await fileReference.OpenReadAsync())
                {
                    await msg.AddAttachmentAsync(fileName, stream);
                }

                var response = await client.SendEmailAsync(msg);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error sending sendgrid email");
                }
            }
        }

        public async Task SendSingleTemplateEmailSingleRcptAttachment(string from, string to, string templateID, object templateData, IFormFile fileReference)
        {
            Require.That(templateID != null, new ErrorCode("SendgridError", "Required Sengrid template ID not configured in app settings", HttpStatusCode.NotImplemented));
            {
                var fromEmail = new EmailAddress(from);
                var toEmail = new EmailAddress(to);
                var msg = MailHelper.CreateSingleTemplateEmail(fromEmail, toEmail, templateID, templateData);
                if (fileReference != null)
                {
                    using (var stream = fileReference.OpenReadStream())
                    {
                        await msg.AddAttachmentAsync(fileReference.FileName, stream);
                    }
                }

                var response = await client.SendEmailAsync(msg);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error sending sendgrid email");
                }
            }
        }

        public async Task SendPasswordResetEmail(MessageNotification<PasswordResetEventBody> messageNotification)
        {
            var templateData = new EmailTemplate<PasswordResetData>()
            {
                Data = new PasswordResetData
                {
                    FirstName = messageNotification?.Recipient?.FirstName,
                    LastName = messageNotification?.Recipient?.LastName,
                    Username = messageNotification?.Recipient?.Username,
                    PasswordRenewalVerificationCode = messageNotification?.EventBody?.PasswordRenewalVerificationCode,
                    PasswordRenewalAccessToken = messageNotification?.EventBody?.PasswordRenewalAccessToken,
                    PasswordRenewalUrl = messageNotification?.EventBody?.PasswordRenewalUrl,
                },
            };
            await SendSingleTemplateEmail(sendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendgridSettings?.PasswordResetTemplateID, templateData);
        }

        public async Task SendLineItemStatusChangeEmail(HSOrder order, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItems, string firstName, string lastName, string email, EmailDisplayText lineItemEmailDisplayText)
        {
            var productsList = CreateTemplateProductList(lineItems, lineItemStatusChanges);
            EmailTemplate<LineItemStatusChangeData> templateData = new EmailTemplate<LineItemStatusChangeData>()
            {
                Data = new LineItemStatusChangeData
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Products = productsList,
                    DateSubmitted = order?.DateSubmitted?.ToString(),
                    OrderID = order.ID,
                    Comments = order.Comments,
                    TrackingNumber = lineItemStatusChanges.SuperShipment?.Shipment?.TrackingNumber,
                },
                Message = new EmailDisplayText
                {
                    EmailSubject = lineItemEmailDisplayText?.EmailSubject,
                    DynamicText = lineItemEmailDisplayText?.DynamicText,
                    DynamicText2 = lineItemEmailDisplayText?.DynamicText2,
                },
            };
            await SendSingleTemplateEmail(sendgridSettings?.FromEmail, email, sendgridSettings?.LineItemStatusChangeTemplateID, templateData);
        }

        public async Task SendLineItemStatusChangeEmailMultipleRcpts(HSOrder order, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItems, List<EmailAddress> tos, EmailDisplayText lineItemEmailDisplayText)
        {
            var productsList = CreateTemplateProductList(lineItems, lineItemStatusChanges);
            var templateData = new EmailTemplate<LineItemStatusChangeData>()
            {
                Data = new LineItemStatusChangeData
                {
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Products = productsList,
                    DateSubmitted = order.DateSubmitted.ToString(),
                    OrderID = order.ID,
                    Comments = order.Comments,
                },
                Message = new EmailDisplayText
                {
                    EmailSubject = lineItemEmailDisplayText?.EmailSubject,
                    DynamicText = lineItemEmailDisplayText?.DynamicText,
                    DynamicText2 = lineItemEmailDisplayText?.DynamicText2,
                },
            };
            await SendSingleTemplateEmailMultipleRcpts(sendgridSettings?.FromEmail, tos, sendgridSettings?.LineItemStatusChangeTemplateID, templateData);
        }

        public async Task SendOrderSubmittedForApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            var order = messageNotification.EventBody.Order;
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetRequestedApprovalText(),
            };
            await SendSingleTemplateEmail(sendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendgridSettings?.OrderApprovalTemplateID, templateData);
        }

        public async Task SendOrderRequiresApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            var order = messageNotification.EventBody.Order;
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetOrderRequiresApprovalText(),
            };
            await SendSingleTemplateEmail(sendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendgridSettings?.OrderApprovalTemplateID, templateData);
        }

        public async Task SendNewUserEmail(MessageNotification<PasswordResetEventBody> messageNotification)
        {
            var templateData = new EmailTemplate<NewUserData>()
            {
                Data = new NewUserData
                {
                    FirstName = messageNotification?.Recipient?.FirstName,
                    LastName = messageNotification?.Recipient?.LastName,
                    PasswordRenewalAccessToken = messageNotification?.EventBody?.PasswordRenewalAccessToken,
                    BaseAppURL = uiSettings?.BaseAdminUrl,
                    Username = messageNotification?.EventBody?.Username,
                },
            };
            await SendSingleTemplateEmail(sendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendgridSettings?.NewUserTemplateID, templateData);
        }

        public async Task SendOrderApprovedEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            var order = messageNotification.EventBody.Order;
            var approval = messageNotification.EventBody.Approvals.FirstOrDefault();
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetOrderApprovedText(),
            };
            templateData.Data.Comments = approval.Comments;
            await SendSingleTemplateEmail(sendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendgridSettings?.OrderApprovalTemplateID, templateData);
        }

        public async Task SendOrderDeclinedEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            var order = messageNotification.EventBody.Order;
            var approval = messageNotification.EventBody.Approvals.FirstOrDefault();
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetOrderDeclinedText(),
            };

            templateData.Data.Comments = approval.Comments;
            await SendSingleTemplateEmail(sendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendgridSettings?.OrderApprovalTemplateID, templateData);
        }

        public async Task SendOrderSubmitEmail(HSOrderWorksheet orderWorksheet)
        {
            var supplierEmailList = await GetSupplierEmails(orderWorksheet);
            var firstName = orderWorksheet.Order.FromUser.FirstName;
            var lastName = orderWorksheet.Order.FromUser.LastName;
            if (orderWorksheet.Order.xp.OrderType == OrderType.Standard)
            {
                var orderData = SendgridMappers.GetOrderTemplateData(orderWorksheet.Order, orderWorksheet.LineItems);
                var sellerTemplateData = new EmailTemplate<OrderTemplateData>()
                {
                    Data = orderData,
                    Message = OrderSubmitEmailConstants.GetOrderSubmitText(orderWorksheet.Order.ID, firstName, lastName, VerifiedUserType.admin),
                };
                var buyerTemplateData = new EmailTemplate<OrderTemplateData>()
                {
                    Data = orderData,
                    Message = OrderSubmitEmailConstants.GetOrderSubmitText(orderWorksheet.Order.ID, firstName, lastName, VerifiedUserType.buyer),
                };

                var sellerEmailList = await GetSellerEmails();

                // send emails
                await SendSingleTemplateEmailMultipleRcpts(sendgridSettings?.FromEmail, sellerEmailList, sendgridSettings?.OrderSubmitTemplateID, sellerTemplateData);
                await SendSingleTemplateEmail(sendgridSettings?.FromEmail, orderWorksheet.Order.FromUser.Email, sendgridSettings?.OrderSubmitTemplateID, buyerTemplateData);
                await SendSupplierOrderSubmitEmails(orderWorksheet);
            }
            else if (orderWorksheet.Order.xp.OrderType == OrderType.Quote)
            {
                var orderData = SendgridMappers.GetQuoteOrderTemplateData(orderWorksheet.Order, orderWorksheet.LineItems);

                var buyerTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
                {
                    Data = orderData,
                    Message = OrderSubmitEmailConstants.GetQuoteOrderSubmitText(VerifiedUserType.buyer),
                };
                var supplierTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
                {
                    Data = orderData,
                    Message = OrderSubmitEmailConstants.GetQuoteOrderSubmitText(VerifiedUserType.supplier),
                };

                // send emails
                await SendSingleTemplateEmailMultipleRcpts(sendgridSettings?.FromEmail, supplierEmailList, sendgridSettings?.QuoteOrderSubmitTemplateID, supplierTemplateData);
                await SendSingleTemplateEmail(sendgridSettings?.FromEmail, orderWorksheet.Order.FromUser.Email, sendgridSettings?.QuoteOrderSubmitTemplateID, buyerTemplateData);
            }
        }

        public async Task SendLineItemStatusChangeEmail(LineItemStatusChange lineItemStatusChange, List<HSLineItem> lineItems, string firstName, string lastName, string email, EmailDisplayText lineItemEmailDisplayText)
        {
            var productsList = lineItems.Select(SendgridMappers.MapLineItemToProduct).ToList();

            var templateData = new EmailTemplate<LineItemStatusChangeData>()
            {
                Data = new LineItemStatusChangeData
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Products = productsList,
                },
                Message = new EmailDisplayText()
                {
                    EmailSubject = lineItemEmailDisplayText?.EmailSubject,
                    DynamicText = lineItemEmailDisplayText?.DynamicText,
                    DynamicText2 = lineItemEmailDisplayText?.DynamicText2,
                },
            };
            await SendSingleTemplateEmail(sendgridSettings?.FromEmail, email, sendgridSettings?.LineItemStatusChangeTemplateID, templateData);
        }

        public async Task SendContactSupplierAboutProductEmail(ContactSupplierBody template)
        {
            var supplier = await oc.Suppliers.GetAsync<HSSupplier>(template.Product.DefaultSupplierID);
            var supplierEmail = supplier.xp.SupportContact.Email;
            var templateData = new EmailTemplate<ProductInformationRequestData>()
            {
                Data = new ProductInformationRequestData
                {
                    ProductID = template?.Product?.ID,
                    ProductName = template?.Product?.Name,
                    FirstName = template?.BuyerRequest?.FirstName,
                    LastName = template?.BuyerRequest?.LastName,
                    Location = template?.BuyerRequest?.BuyerLocation,
                    Phone = template?.BuyerRequest?.Phone,
                    Email = template?.BuyerRequest?.Email,
                    Note = template?.BuyerRequest?.Comments,
                },
            };
            await SendSingleTemplateEmail(sendgridSettings?.FromEmail, supplierEmail, sendgridSettings?.ProductInformationRequestTemplateID, templateData);
            var sellerUsers = await oc.AdminUsers.ListAllAsync<HSUser>(filters: $"xp.RequestInfoEmails=true");
            foreach (var sellerUser in sellerUsers)
            {
                await SendSingleTemplateEmail(sendgridSettings?.FromEmail, sellerUser.Email, sendgridSettings?.ProductInformationRequestTemplateID, templateData);
                if (sellerUser.xp.AddtlRcpts.Any())
                {
                    foreach (var rcpt in sellerUser.xp.AddtlRcpts)
                    {
                        await SendSingleTemplateEmail(sendgridSettings?.FromEmail, rcpt, sendgridSettings?.ProductInformationRequestTemplateID, templateData);
                    }
                }
            }
        }

        public async Task EmailVoidAuthorizationFailedAsync(HSPayment payment, string transactionID, HSOrder order, ApiError apiError)
        {
            var templateData = new EmailTemplate<SupportTemplateData>()
            {
                Data = new SupportTemplateData
                {
                    OrderID = order.ID,
                    DynamicPropertyName1 = "BuyerID",
                    DynamicPropertyValue1 = order.FromCompanyID,
                    DynamicPropertyName2 = "Username",
                    DynamicPropertyValue2 = order.FromUser.Username,
                    DynamicPropertyName3 = "PaymentID",
                    DynamicPropertyValue3 = payment.ID,
                    DynamicPropertyName4 = "TransactionID",
                    DynamicPropertyValue4 = transactionID,
                    ErrorJsonString = JsonConvert.SerializeObject(apiError),
                },
                Message = new EmailDisplayText()
                {
                    EmailSubject = "Manual intervention required for this order",
                    DynamicText = "Error encountered while trying to void authorization on this order. Please contact customer and help them manually void authorization",
                },
            };
            var toList = new List<EmailAddress>();
            var supportEmails = sendgridSettings?.CriticalSupportEmails.Split(",");
            foreach (var email in supportEmails)
            {
                toList.Add(new EmailAddress { Email = email });
            }

            await SendSingleTemplateEmailMultipleRcpts(sendgridSettings?.FromEmail, toList, sendgridSettings?.CriticalSupportTemplateID, templateData);
        }

        public async Task EmailGeneralSupportQueue(SupportCase supportCase)
        {
            var templateData = new EmailTemplate<SupportTemplateData>()
            {
                Data = new SupportTemplateData
                {
                    DynamicPropertyName1 = "FirstName",
                    DynamicPropertyValue1 = supportCase.FirstName,
                    DynamicPropertyName2 = "LastName",
                    DynamicPropertyValue2 = supportCase.LastName,
                    DynamicPropertyName3 = "Email",
                    DynamicPropertyValue3 = supportCase.Email,
                    DynamicPropertyName4 = "Vendor",
                    DynamicPropertyValue4 = supportCase.Vendor ?? "N/A",
                },
                Message = new EmailDisplayText()
                {
                    EmailSubject = supportCase.Subject,
                    DynamicText = supportCase.Message,
                },
            };
            var recipient = SendgridMappers.DetermineRecipient(sendgridSettings, supportCase.Subject);
            await SendSingleTemplateEmailSingleRcptAttachment(sendgridSettings?.FromEmail, recipient, sendgridSettings?.CriticalSupportTemplateID, templateData, supportCase.File);
        }

        private List<LineItemProductData> CreateTemplateProductList(List<HSLineItem> lineItems, LineItemStatusChanges lineItemStatusChanges)
        {
            // first get line items that actually had a change
            var changedLiIds = lineItemStatusChanges.Changes.Where(change => change.Quantity > 0).Select(change => change.ID);
            var changedLineItems = changedLiIds.Select(i => lineItems.Single(l => l.ID == i));

            // now map to template data
            return changedLineItems.Select(lineItem =>
            {
                var lineItemStatusChange = lineItemStatusChanges.Changes.First(li => li.ID == lineItem.ID);
                return SendgridMappers.MapToTemplateProduct(lineItem, lineItemStatusChange, lineItemStatusChanges.Status);
            }).ToList();
        }

        private async Task SendSupplierOrderSubmitEmails(HSOrderWorksheet orderWorksheet)
        {
            ListPage<HSSupplier> suppliers = null;
            if (orderWorksheet.Order.xp.SupplierIDs != null)
            {
                var filterString = string.Join("|", orderWorksheet.Order.xp.SupplierIDs);
                suppliers = await oc.Suppliers.ListAsync<HSSupplier>(filters: $"ID={filterString}");
            }

            foreach (var supplier in suppliers.Items)
            {
                if (supplier?.xp?.NotificationRcpts?.Count() > 0)
                {
                    // get orderworksheet for supplier order and fill in some information from buyer order worksheet
                    var supplierOrderWorksheet = await BuildSupplierOrderWorksheet(orderWorksheet, supplier.ID);
                    var supplierTemplateData = new EmailTemplate<OrderTemplateData>()
                    {
                        Data = SendgridMappers.GetOrderTemplateData(supplierOrderWorksheet.Order, supplierOrderWorksheet.LineItems),
                        Message = OrderSubmitEmailConstants.GetOrderSubmitText(orderWorksheet.Order.ID, supplierOrderWorksheet.Order.FromUser.FirstName, supplierOrderWorksheet.Order.FromUser.LastName, VerifiedUserType.supplier),
                    };

                    var supplierTos = new List<EmailAddress>();
                    foreach (var rcpt in supplier.xp.NotificationRcpts)
                    {
                        supplierTos.Add(new EmailAddress(rcpt));
                    }

                    await SendSingleTemplateEmailMultipleRcpts(sendgridSettings?.FromEmail, supplierTos, sendgridSettings?.OrderSubmitTemplateID, supplierTemplateData);
                }
            }
        }

        private async Task<HSOrderWorksheet> BuildSupplierOrderWorksheet(HSOrderWorksheet orderWorksheet, string supplierID)
        {
            var supplierOrderWorksheet = await oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Outgoing, $"{orderWorksheet.Order.ID}-{supplierID}");
            supplierOrderWorksheet.Order.BillingAddress = orderWorksheet.Order.BillingAddress;
            supplierOrderWorksheet.Order.FromUser = orderWorksheet.Order.FromUser;
            return supplierOrderWorksheet;
        }

        private async Task<List<EmailAddress>> GetSupplierEmails(HSOrderWorksheet orderWorksheet)
        {
            ListPage<HSSupplier> suppliers = null;
            if (orderWorksheet.Order.xp.SupplierIDs != null)
            {
                var filterString = string.Join("|", orderWorksheet.Order.xp.SupplierIDs);
                suppliers = await oc.Suppliers.ListAsync<HSSupplier>(filters: $"ID={filterString}");
            }

            var supplierTos = new List<EmailAddress>();
            foreach (var supplier in suppliers.Items)
            {
                if (supplier?.xp?.NotificationRcpts?.Count() > 0)
                {
                    foreach (var rcpt in supplier.xp.NotificationRcpts)
                    {
                        supplierTos.Add(new EmailAddress(rcpt));
                    }
                }
            }

            return supplierTos;
        }

        private async Task<List<EmailAddress>> GetSellerEmails()
        {
            var sellerUsers = await oc.AdminUsers.ListAsync<HSSellerUser>();
            var sellerTos = new List<EmailAddress>();
            foreach (var seller in sellerUsers.Items)
            {
                if (seller?.xp?.OrderEmails ?? false)
                {
                    sellerTos.Add(new EmailAddress(seller.Email));
                }

                if (seller?.xp?.AddtlRcpts?.Any() ?? false)
                {
                    foreach (var rcpt in seller.xp.AddtlRcpts)
                    {
                        sellerTos.Add(new EmailAddress(rcpt));
                    }
                }
            }

            return sellerTos;
        }
    }
}
