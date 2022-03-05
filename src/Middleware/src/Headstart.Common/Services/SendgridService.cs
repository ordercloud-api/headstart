using System;
using SendGrid;
using System.IO;
using System.Linq;
using OrderCloud.SDK;
using Newtonsoft.Json;
using Headstart.Models;
using OrderCloud.Catalyst;
using SendGrid.Helpers.Mail;
using Headstart.Models.Misc;
using System.Threading.Tasks;
using Headstart.Common.Mappers;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Headstart.Common.Constants;
using Headstart.Models.Headstart;
using Headstart.Common.Models.Misc;
using Microsoft.WindowsAzure.Storage.Blob;
using ordercloud.integrations.cardconnect;
using static Headstart.Common.Models.SendGridModels;
using Headstart.Common.Services.ShippingIntegration.Models;

namespace Headstart.Common.Services
{
    public interface ISendgridService
    {
        Task SendSingleTemplateEmail(string from, string to, string templateID, object templateData);
        Task SendSingleTemplateEmailMultipleRcpts(string from, List<EmailAddress> tos, string templateID, object templateData);
        Task SendSingleTemplateEmailMultipleRcptsAttachment(string from, List<EmailAddress> tos, string templateID, object templateData, CloudAppendBlob fileReference, string fileName);
        Task SendSingleTemplateEmailSingleRcptAttachment(string from, string to, string templateID, object templateData, IFormFile fileReference);
        Task SendOrderSubmitEmail(HSOrderWorksheet orderData);
        Task SendNewUserEmail(MessageNotification<PasswordResetEventBody> payload);
        Task SendPasswordResetEmail(MessageNotification<PasswordResetEventBody> messageNotification);
        Task SendOrderRequiresApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification);
        Task SendOrderSubmittedForApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification);
        Task SendOrderApprovedEmail(MessageNotification<OrderSubmitEventBody> messageNotification);
        Task SendOrderDeclinedEmail(MessageNotification<OrderSubmitEventBody> messageNotification);
        Task SendLineItemStatusChangeEmail(HSOrder order, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItems, string firstName, string lastName, string email, EmailDisplayText lineItemEmailDisplayText);
        Task SendLineItemStatusChangeEmailMultipleRcpts(HSOrder order, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItems, List<EmailAddress> tos, EmailDisplayText lineItemEmailDisplayText);
        Task SendContactSupplierAboutProductEmail(ContactSupplierBody template);
        Task EmailVoidAuthorizationFailedAsync(HSPayment payment, string transactionID, HSOrder order, CreditCardVoidException ex);
        Task EmailGeneralSupportQueue(SupportCase supportCase);
        Task SendQuotePriceConfirmationEmail(HSOrder order, HSLineItem LineItem, string buyerEmail);
        Task SendQuoteRequestConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail);
    }


    public class SendgridService : ISendgridService
    {
        private readonly AppSettings _settings;
        private readonly IOrderCloudClient _oc;
        private readonly ISendGridClient _client;

        public SendgridService(AppSettings settings, IOrderCloudClient ocClient, ISendGridClient client)
        {
            _oc = ocClient;
            _client = client;
            _settings = settings;
        }

        public async Task SendQuoteRequestConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail)
        {
            EmailTemplate<QuoteOrderTemplateData> buyerTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
            {
                Data = SendgridMappers.GetQuoteOrderTemplateData(order, new List<HSLineItem> { lineItem }),
                Message = OrderSubmitEmailConstants.GetQuoteRequestConfirmationText()
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, buyerEmail, _settings?.SendgridSettings?.QuoteOrderSubmitTemplateID, buyerTemplateData);
        }

        public async Task SendQuotePriceConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail)
        {
            EmailTemplate<QuoteOrderTemplateData> buyerTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
            {
                Data = SendgridMappers.GetQuoteOrderTemplateData(order, new List<HSLineItem> { lineItem }),
                Message = OrderSubmitEmailConstants.GetQuotePriceConfirmationText()
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, buyerEmail, _settings?.SendgridSettings?.QuoteOrderSubmitTemplateID, buyerTemplateData);
        }

        public virtual async Task SendSingleTemplateEmail(string from, string to, string templateID, object templateData)
        {
            Require.That(templateID != null, new ErrorCode($@"SendgridError", $@"The required Sengrid template ID not configured in app settings.", 501));
            {
                EmailAddress fromEmail = new EmailAddress(from);
                EmailAddress toEmail = new EmailAddress(to);
                SendGridMessage msg = MailHelper.CreateSingleTemplateEmail(fromEmail, toEmail, templateID, templateData);
                Response response = await _client.SendEmailAsync(msg);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($@"Error sending sendgrid email");
                }
            }
        }

        public virtual async Task SendSingleTemplateEmailMultipleRcpts(string from, List<EmailAddress> tos, string templateID, object templateData)
        {
            Require.That(templateID != null, new ErrorCode($@"SendgridError", $@"The required Sengrid template ID not configured in app settings.", 501));
            {
                EmailAddress fromEmail = new EmailAddress(from);
                SendGridMessage msg = MailHelper.CreateSingleTemplateEmailToMultipleRecipients(fromEmail, tos, templateID, templateData);
                Response response = await _client.SendEmailAsync(msg);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($@"Error sending sendgrid email");
                }
            }
        }

        public async Task SendSingleTemplateEmailMultipleRcptsAttachment(string from, List<EmailAddress> tos, string templateID, object templateData, CloudAppendBlob fileReference, string fileName)
        {
            Require.That(templateID != null, new ErrorCode($@"SendgridError", $@"The required Sengrid template ID not configured in app settings.", 501));
            {
                EmailAddress fromEmail = new EmailAddress(from);
                SendGridMessage msg = MailHelper.CreateSingleTemplateEmailToMultipleRecipients(fromEmail, tos, templateID, templateData);
                using (Stream stream = await fileReference.OpenReadAsync())
                {
                    await msg.AddAttachmentAsync(fileName, stream);
                }
                Response response = await _client.SendEmailAsync(msg);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($@"Error sending sendgrid email");
                }
            }
        }

        public async Task SendSingleTemplateEmailSingleRcptAttachment(string from, string to, string templateID, object templateData, IFormFile fileReference)
        {
            Require.That(templateID != null, new ErrorCode($@"SendgridError", $@"The required Sengrid template ID not configured in app settings.", 501));
            {
                EmailAddress fromEmail = new EmailAddress(from);
                EmailAddress toEmail = new EmailAddress(to);
                SendGridMessage msg = MailHelper.CreateSingleTemplateEmail(fromEmail, toEmail, templateID, templateData);
                if (fileReference != null)
                {
                    using (Stream stream = fileReference.OpenReadStream())
                    {
                        await msg.AddAttachmentAsync(fileReference.FileName, stream);
                    }
                }
                Response response = await _client.SendEmailAsync(msg);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($@"Error sending sendgrid email");
                }
            }
        }

        public async Task SendPasswordResetEmail(MessageNotification<PasswordResetEventBody> messageNotification)
        {
            EmailTemplate<PasswordResetData> templateData = new EmailTemplate<PasswordResetData>()
            {
                Data = new PasswordResetData
                {
                    FirstName = messageNotification?.Recipient?.FirstName,
                    LastName = messageNotification?.Recipient?.LastName,
                    Username = messageNotification?.Recipient?.Username,
                    PasswordRenewalVerificationCode = messageNotification?.EventBody?.PasswordRenewalVerificationCode,
                    PasswordRenewalAccessToken = messageNotification?.EventBody?.PasswordRenewalAccessToken,
                    PasswordRenewalUrl = messageNotification?.EventBody?.PasswordRenewalUrl
                }
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.PasswordResetTemplateID, templateData);
        }

        private List<LineItemProductData> CreateTemplateProductList(List<HSLineItem> lineItems, LineItemStatusChanges lineItemStatusChanges)
        {
            //  first get line items that actually had a change
            IEnumerable<string> changedLiIds = lineItemStatusChanges.Changes.Where(change => change.Quantity > 0).Select(change => change.ID);
            IEnumerable<HSLineItem> changedLineItems = changedLiIds.Select(i => lineItems.Single(l => l.ID == i));
            //  now map to template data
            return changedLineItems.Select(lineItem =>
            {
                LineItemStatusChange lineItemStatusChange = lineItemStatusChanges.Changes.First(li => li.ID == lineItem.ID);
                return SendgridMappers.MapToTemplateProduct(lineItem, lineItemStatusChange, lineItemStatusChanges.Status);
            }).ToList();
        }

        public async Task SendLineItemStatusChangeEmail(HSOrder order, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItems, string firstName, string lastName, string email, EmailDisplayText lineItemEmailDisplayText)
        {
            List<LineItemProductData> productsList = CreateTemplateProductList(lineItems, lineItemStatusChanges);
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
                    TrackingNumber = lineItemStatusChanges.SuperShipment?.Shipment?.TrackingNumber
                },
                Message = new EmailDisplayText
                {
                    EmailSubject = lineItemEmailDisplayText?.EmailSubject,
                    DynamicText = lineItemEmailDisplayText?.DynamicText,
                    DynamicText2 = lineItemEmailDisplayText?.DynamicText2
                }
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, email, _settings?.SendgridSettings?.LineItemStatusChangeTemplateID, templateData);
        }

        public async Task SendLineItemStatusChangeEmailMultipleRcpts(HSOrder order, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItems, List<EmailAddress> tos, EmailDisplayText lineItemEmailDisplayText)
        {
            List<LineItemProductData> productsList = CreateTemplateProductList(lineItems, lineItemStatusChanges);
            EmailTemplate<LineItemStatusChangeData> templateData = new EmailTemplate<LineItemStatusChangeData>()
            {
                Data = new LineItemStatusChangeData
                {
                    FirstName = "",
                    LastName = "",
                    Products = productsList,
                    DateSubmitted = order.DateSubmitted.ToString(),
                    OrderID = order.ID,
                    Comments = order.Comments
                },
                Message = new EmailDisplayText
                {
                    EmailSubject = lineItemEmailDisplayText?.EmailSubject,
                    DynamicText = lineItemEmailDisplayText?.DynamicText,
                    DynamicText2 = lineItemEmailDisplayText?.DynamicText2,
                }
            };
            await SendSingleTemplateEmailMultipleRcpts(_settings?.SendgridSettings?.FromEmail, tos, _settings?.SendgridSettings?.LineItemStatusChangeTemplateID, templateData);
        }

        public async Task SendOrderSubmittedForApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            HSOrder order = messageNotification.EventBody.Order;
            EmailTemplate<OrderTemplateData> templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetRequestedApprovalText()
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.OrderApprovalTemplateID, templateData);
        }

        public async Task SendOrderRequiresApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            HSOrder order = messageNotification.EventBody.Order;
            EmailTemplate<OrderTemplateData> templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetOrderRequiresApprovalText()
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.OrderApprovalTemplateID, templateData);
        }

        public async Task SendNewUserEmail(MessageNotification<PasswordResetEventBody> messageNotification)
        {
            EmailTemplate<NewUserData> templateData = new EmailTemplate<NewUserData>()
            {
                Data = new NewUserData
                {
                    FirstName = messageNotification?.Recipient?.FirstName,
                    LastName = messageNotification?.Recipient?.LastName,
                    PasswordRenewalAccessToken = messageNotification?.EventBody?.PasswordRenewalAccessToken,
                    BaseAppURL = _settings?.UI?.BaseAdminUrl,
                    Username = messageNotification?.EventBody?.Username
                }
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.NewUserTemplateID, templateData);
        }

        public async Task SendOrderApprovedEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            HSOrder order = messageNotification.EventBody.Order;
            OrderApproval approval = messageNotification.EventBody.Approvals.FirstOrDefault();
            EmailTemplate<OrderTemplateData> templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetOrderApprovedText()
            };
            templateData.Data.Comments = approval.Comments;
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.OrderApprovalTemplateID, templateData);
        }

        public async Task SendOrderDeclinedEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            HSOrder order = messageNotification.EventBody.Order;
            OrderApproval approval = messageNotification.EventBody.Approvals.FirstOrDefault();
            EmailTemplate<OrderTemplateData> templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetOrderDeclinedText()
            };

            templateData.Data.Comments = approval.Comments;
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.OrderApprovalTemplateID, templateData);
        }

        public async Task SendOrderSubmitEmail(HSOrderWorksheet orderWorksheet)
        {
            List<EmailAddress> supplierEmailList = await GetSupplierEmails(orderWorksheet);
            string firstName = orderWorksheet.Order.FromUser.FirstName;
            string lastName = orderWorksheet.Order.FromUser.LastName;
            if (orderWorksheet.Order.xp.OrderType == OrderType.Standard)
            {
                OrderTemplateData orderData = SendgridMappers.GetOrderTemplateData(orderWorksheet.Order, orderWorksheet.LineItems);
                EmailTemplate<OrderTemplateData> sellerTemplateData = new EmailTemplate<OrderTemplateData>()
                {
                    Data = orderData,
                    Message = OrderSubmitEmailConstants.GetOrderSubmitText(orderWorksheet.Order.ID, firstName, lastName, VerifiedUserType.admin)
                };
                EmailTemplate<OrderTemplateData> buyerTemplateData = new EmailTemplate<OrderTemplateData>()
                {
                    Data = orderData,
                    Message = OrderSubmitEmailConstants.GetOrderSubmitText(orderWorksheet.Order.ID, firstName, lastName, VerifiedUserType.buyer)
                };

                List<EmailAddress> sellerEmailList = await GetSellerEmails();

                //  send emails

                await SendSingleTemplateEmailMultipleRcpts(_settings?.SendgridSettings?.FromEmail, sellerEmailList, _settings?.SendgridSettings?.OrderSubmitTemplateID, sellerTemplateData);
                await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, orderWorksheet.Order.FromUser.Email, _settings?.SendgridSettings?.OrderSubmitTemplateID, buyerTemplateData);
                await SendSupplierOrderSubmitEmails(orderWorksheet);
            }
            else if (orderWorksheet.Order.xp.OrderType == OrderType.Quote)
            {
                QuoteOrderTemplateData orderData = SendgridMappers.GetQuoteOrderTemplateData(orderWorksheet.Order, orderWorksheet.LineItems);

                EmailTemplate<QuoteOrderTemplateData> buyerTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
                {
                    Data = orderData,
                    Message = OrderSubmitEmailConstants.GetQuoteOrderSubmitText(VerifiedUserType.buyer)
                };
                EmailTemplate<QuoteOrderTemplateData> supplierTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
                {
                    Data = orderData,
                    Message = OrderSubmitEmailConstants.GetQuoteOrderSubmitText(VerifiedUserType.supplier)
                };

                //  send emails
                await SendSingleTemplateEmailMultipleRcpts(_settings?.SendgridSettings?.FromEmail, supplierEmailList, _settings?.SendgridSettings?.QuoteOrderSubmitTemplateID, supplierTemplateData);
                await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, orderWorksheet.Order.FromUser.Email, _settings?.SendgridSettings?.QuoteOrderSubmitTemplateID, buyerTemplateData);
            }
        }

        private async Task SendSupplierOrderSubmitEmails(HSOrderWorksheet orderWorksheet)
        {
            ListPage<HSSupplier> suppliers = null;
            if (orderWorksheet.Order.xp.SupplierIDs != null)
            {
                string filterString = string.Join($@"|", orderWorksheet.Order.xp.SupplierIDs);
                suppliers = await _oc.Suppliers.ListAsync<HSSupplier>(filters: $@"ID={filterString}");
            }
            foreach (HSSupplier supplier in suppliers.Items)
            {
                if (supplier?.xp?.NotificationRcpts?.Count() > 0)
                {
                    // get orderworksheet for supplier order and fill in some information from buyer order worksheet
                    HSOrderWorksheet supplierOrderWorksheet = await BuildSupplierOrderWorksheet(orderWorksheet, supplier.ID);
                    EmailTemplate<OrderTemplateData> supplierTemplateData = new EmailTemplate<OrderTemplateData>()
                    {
                        Data = SendgridMappers.GetOrderTemplateData(supplierOrderWorksheet.Order, supplierOrderWorksheet.LineItems),
                        Message = OrderSubmitEmailConstants.GetOrderSubmitText(orderWorksheet.Order.ID, supplierOrderWorksheet.Order.FromUser.FirstName, supplierOrderWorksheet.Order.FromUser.LastName, VerifiedUserType.supplier)
                    };

                    // SEB-Specific Data
                    supplierTemplateData.Data.BillTo = new Address()
                    {
                        CompanyName = $@"SEB Vendor Portal - BES",
                        Street1 = $@"8646 Eagle Creek Circle",
                        Street2 = $@"Suite 107",
                        City = $@"Savage",
                        State = $@"MN",
                        Zip = $@"55378",
                        Phone = $@"877-771-9123",
                        xp =
                        {
                            Email = $@"accounting@sebvendorportal.com"
                        }
                    };

                    List<EmailAddress> supplierTos = new List<EmailAddress>();
                    foreach (string rcpt in supplier.xp.NotificationRcpts)
                    {
                        supplierTos.Add(new EmailAddress(rcpt));
                    }
                    await SendSingleTemplateEmailMultipleRcpts(_settings?.SendgridSettings?.FromEmail, supplierTos, _settings?.SendgridSettings?.OrderSubmitTemplateID, supplierTemplateData);
                }
            }
        }

        private async Task<HSOrderWorksheet> BuildSupplierOrderWorksheet(HSOrderWorksheet orderWorksheet, string supplierID)
        {
            HSOrderWorksheet supplierOrderWorksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Outgoing, $"{orderWorksheet.Order.ID}-{supplierID}");
            supplierOrderWorksheet.Order.BillingAddress = orderWorksheet.Order.BillingAddress;
            supplierOrderWorksheet.Order.FromUser = orderWorksheet.Order.FromUser;
            return supplierOrderWorksheet;
        }

        private async Task<List<EmailAddress>> GetSupplierEmails(HSOrderWorksheet orderWorksheet)
        {
            ListPage<HSSupplier> suppliers = null;
            if (orderWorksheet.Order.xp.SupplierIDs != null)
            {
                string filterString = string.Join($@"|", orderWorksheet.Order.xp.SupplierIDs);
                suppliers = await _oc.Suppliers.ListAsync<HSSupplier>(filters: $@"ID={filterString}");
            }
            List<EmailAddress> supplierTos = new List<EmailAddress>();
            foreach (HSSupplier supplier in suppliers.Items)
            {
                if (supplier?.xp?.NotificationRcpts?.Count() > 0)
                {
                    foreach (string rcpt in supplier.xp.NotificationRcpts)
                    {
                        supplierTos.Add(new EmailAddress(rcpt));
                    }
                }
            }
            return supplierTos;
        }

        private async Task<List<EmailAddress>> GetSellerEmails()
        {
            ListPage<HSSellerUser> sellerUsers = await _oc.AdminUsers.ListAsync<HSSellerUser>();
            List<EmailAddress> sellerTos = new List<EmailAddress>();
            foreach (HSSellerUser seller in sellerUsers.Items)
            {
                if (seller?.xp?.OrderEmails ?? false)
                {
                    sellerTos.Add(new EmailAddress(seller.Email));
                }
                if (seller?.xp?.AddtlRcpts?.Any() ?? false)
                {
                    foreach (string rcpt in seller.xp.AddtlRcpts)
                    {
                        sellerTos.Add(new EmailAddress(rcpt));
                    }
                }
            }
            return sellerTos;
        }

        public async Task SendLineItemStatusChangeEmail(LineItemStatusChange lineItemStatusChange, List<HSLineItem> lineItems, string firstName, string lastName, string email, EmailDisplayText lineItemEmailDisplayText)
        {
            List<LineItemProductData> productsList = lineItems.Select(SendgridMappers.MapLineItemToProduct).ToList();

            EmailTemplate<LineItemStatusChangeData> templateData = new EmailTemplate<LineItemStatusChangeData>()
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
                    DynamicText2 = lineItemEmailDisplayText?.DynamicText2
                }
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, email, _settings?.SendgridSettings?.LineItemStatusChangeTemplateID, templateData);
        }

        public async Task SendContactSupplierAboutProductEmail(ContactSupplierBody template)
        {
            HSSupplier supplier = await _oc.Suppliers.GetAsync<HSSupplier>(template.Product.DefaultSupplierID);
            string supplierEmail = supplier.xp.SupportContact.Email;
            EmailTemplate<ProductInformationRequestData> templateData = new EmailTemplate<ProductInformationRequestData>()
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
                    Note = template?.BuyerRequest?.Comments
                }
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, supplierEmail, _settings?.SendgridSettings?.ProductInformationRequestTemplateID, templateData);
            List<HSUser> sellerUsers = await _oc.AdminUsers.ListAllAsync<HSUser>(filters: $@"xp.RequestInfoEmails=true");
            foreach (HSUser sellerUser in sellerUsers)
            {
                await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, sellerUser.Email, _settings?.SendgridSettings?.ProductInformationRequestTemplateID, templateData);
                if (sellerUser.xp.AddtlRcpts.Any())
                {
                    foreach (string rcpt in sellerUser.xp.AddtlRcpts)
                    {
                        await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, rcpt, _settings?.SendgridSettings?.ProductInformationRequestTemplateID, templateData);
                    }
                }
            }
        }

        public async Task EmailVoidAuthorizationFailedAsync(HSPayment payment, string transactionID, HSOrder order, CreditCardVoidException ex)
        {
            EmailTemplate<SupportTemplateData> templateData = new EmailTemplate<SupportTemplateData>()
            {
                Data = new SupportTemplateData
                {
                    OrderID = order.ID,
                    DynamicPropertyName1 = $@"BuyerID",
                    DynamicPropertyValue1 = order.FromCompanyID,
                    DynamicPropertyName2 = $@"Username",
                    DynamicPropertyValue2 = order.FromUser.Username,
                    DynamicPropertyName3 = $@"PaymentID",
                    DynamicPropertyValue3 = payment.ID,
                    DynamicPropertyName4 = $@"TransactionID",
                    DynamicPropertyValue4 = transactionID,
                    ErrorJsonString = JsonConvert.SerializeObject(ex.ApiError)
                },
                Message = new EmailDisplayText()
                {
                    EmailSubject = $@"Manual intervention required for this order.",
                    DynamicText = $@"An Error was encountered while trying to void authorization on this order. Please contact customer and help them manually void authorization."
                }
            };
            List<EmailAddress> toList = new List<EmailAddress>();
            string[] supportEmails = _settings?.SendgridSettings?.CriticalSupportEmails.Split(",");
            foreach (string email in supportEmails)
            {
                toList.Add(new EmailAddress { Email = email });
            }
            await SendSingleTemplateEmailMultipleRcpts(_settings?.SendgridSettings?.FromEmail, toList, _settings?.SendgridSettings?.CriticalSupportTemplateID, templateData);
        }

        public async Task EmailGeneralSupportQueue(SupportCase supportCase)
        {
            EmailTemplate<SupportTemplateData> templateData = new EmailTemplate<SupportTemplateData>()
            {
                Data = new SupportTemplateData
                {
                    DynamicPropertyName1 = $@"FirstName",
                    DynamicPropertyValue1 = supportCase.FirstName,
                    DynamicPropertyName2 = $@"LastName",
                    DynamicPropertyValue2 = supportCase.LastName,
                    DynamicPropertyName3 = $@"Email",
                    DynamicPropertyValue3 = supportCase.Email,
                    DynamicPropertyName4 = $@"Vendor",
                    DynamicPropertyValue4 = supportCase.Vendor ?? $@"N/A",
                },
                Message = new EmailDisplayText()
                {
                    EmailSubject = supportCase.Subject,
                    DynamicText = supportCase.Message
                }
            };
            string recipient = SendgridMappers.DetermineRecipient(_settings, supportCase.Subject);
            await SendSingleTemplateEmailSingleRcptAttachment(_settings?.SendgridSettings?.FromEmail, recipient, _settings?.SendgridSettings?.CriticalSupportTemplateID, templateData, supportCase.File);
        }
    }
}