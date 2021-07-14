using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dynamitey;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Http;
using Headstart.Common.Constants;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Misc;
using Microsoft.WindowsAzure.Storage.Blob;
using OrderCloud.SDK;
using SendGrid;
using SendGrid.Helpers.Mail;
using static Headstart.Common.Models.SendGridModels;
using Headstart.Common.Mappers;
using Newtonsoft.Json;
using ordercloud.integrations.cardconnect;
using Headstart.Common.Models.Misc;
using ordercloud.integrations.library;
using Headstart.Models.Headstart;
using OrderCloud.Catalyst;

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

        public virtual async Task SendSingleTemplateEmail(string from, string to, string templateID, object templateData)
        {
            Require.That(templateID != null, new ErrorCode("SendgridError", 501, "Required Sengrid template ID not configured in app settings"));
            {
                var fromEmail = new EmailAddress(from);
                var toEmail = new EmailAddress(to);
                var msg = MailHelper.CreateSingleTemplateEmail(fromEmail, toEmail, templateID, templateData);
                var response = await _client.SendEmailAsync(msg);
                if(!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error sending sendgrid email");
                }
            }
        }

        public virtual async Task SendSingleTemplateEmailMultipleRcpts(string from, List<EmailAddress> tos, string templateID, object templateData)
        {
            Require.That(templateID != null, new ErrorCode("SendgridError", 501, "Required Sengrid template ID not configured in app settings"));
            {
                var fromEmail = new EmailAddress(from);
                var msg = MailHelper.CreateSingleTemplateEmailToMultipleRecipients(fromEmail, tos, templateID, templateData);
                var response = await _client.SendEmailAsync(msg);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error sending sendgrid email");
                }
            }
        }

        public async Task SendSingleTemplateEmailMultipleRcptsAttachment(string from, List<EmailAddress> tos, string templateID, object templateData, CloudAppendBlob fileReference, string fileName)
        {
            Require.That(templateID != null, new ErrorCode("SendgridError", 501, "Required Sengrid template ID not configured in app settings"));
            {
                var fromEmail = new EmailAddress(from);
                var msg = MailHelper.CreateSingleTemplateEmailToMultipleRecipients(fromEmail, tos, templateID, templateData);
                using (var stream = await fileReference.OpenReadAsync())
                {
                    await msg.AddAttachmentAsync(fileName, stream);
                }
                var response = await _client.SendEmailAsync(msg);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error sending sendgrid email");
                }
            }
        }

        public async Task SendSingleTemplateEmailSingleRcptAttachment(string from, string to, string templateID, object templateData, IFormFile fileReference)
        {
            Require.That(templateID != null, new ErrorCode("SendgridError", 501, "Required Sengrid template ID not configured in app settings"));
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
                var response = await _client.SendEmailAsync(msg);
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
                    PasswordRenewalUrl = messageNotification?.EventBody?.PasswordRenewalUrl
                }
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.PasswordResetTemplateID, templateData);
        }

        private List<LineItemProductData> CreateTemplateProductList(List<HSLineItem> lineItems, LineItemStatusChanges lineItemStatusChanges)
        {
            //  first get line items that actually had a change
            var changedLiIds = lineItemStatusChanges.Changes.Where(change => change.Quantity > 0).Select(change => change.ID);
            var changedLineItems = changedLiIds.Select(i => lineItems.Single(l => l.ID == i));
            //  now map to template data
            return changedLineItems.Select(lineItem =>
            {
                var lineItemStatusChange = lineItemStatusChanges.Changes.First(li => li.ID == lineItem.ID);
                return SendgridMappers.MapToTemplateProduct(lineItem, lineItemStatusChange, lineItemStatusChanges.Status);
            }).ToList();
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
            var productsList = CreateTemplateProductList(lineItems, lineItemStatusChanges);
            var templateData = new EmailTemplate<LineItemStatusChangeData>()
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
            var order = messageNotification.EventBody.Order;
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetRequestedApprovalText()
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.OrderApprovalTemplateID, templateData);
        }

        public async Task SendOrderRequiresApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            var order = messageNotification.EventBody.Order;
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetOrderRequiresApprovalText()
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.OrderApprovalTemplateID, templateData);
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
                    BaseAppURL = _settings?.UI?.BaseAdminUrl,
                    Username = messageNotification?.EventBody?.Username
                }
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.NewUserTemplateID, templateData);
        }

        public async Task SendOrderApprovedEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            var order = messageNotification.EventBody.Order;
            var approval = messageNotification.EventBody.Approvals.FirstOrDefault();
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetOrderApprovedText()
            };
            templateData.Data.Comments = approval.Comments;
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.OrderApprovalTemplateID, templateData);
        }

        public async Task SendOrderDeclinedEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            var order = messageNotification.EventBody.Order;
            var approval = messageNotification.EventBody.Approvals.FirstOrDefault();
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetOrderDeclinedText()
            };

            templateData.Data.Comments = approval.Comments;
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.OrderApprovalTemplateID, templateData);
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
                    Message = OrderSubmitEmailConstants.GetOrderSubmitText(orderWorksheet.Order.ID, firstName, lastName, VerifiedUserType.admin)
                };
                var buyerTemplateData = new EmailTemplate<OrderTemplateData>()
                {
                    Data = orderData,
                    Message = OrderSubmitEmailConstants.GetOrderSubmitText(orderWorksheet.Order.ID, firstName, lastName, VerifiedUserType.buyer)
                };

                var sellerEmailList = await GetSellerEmails();

                //  send emails
                
                await SendSingleTemplateEmailMultipleRcpts(_settings?.SendgridSettings?.FromEmail, sellerEmailList, _settings?.SendgridSettings?.OrderSubmitTemplateID, sellerTemplateData);
                await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, orderWorksheet.Order.FromUser.Email, _settings?.SendgridSettings?.OrderSubmitTemplateID, buyerTemplateData);
                await SendSupplierOrderSubmitEmails(orderWorksheet);
            }
            else if (orderWorksheet.Order.xp.OrderType == OrderType.Quote)
            {
                var orderData = SendgridMappers.GetQuoteOrderTemplateData(orderWorksheet.Order, orderWorksheet.LineItems);

                var buyerTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
                {
                    Data = orderData,
                    Message = OrderSubmitEmailConstants.GetQuoteOrderSubmitText(VerifiedUserType.buyer)
                };
                var supplierTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
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
                var filterString = String.Join("|", orderWorksheet.Order.xp.SupplierIDs);
                suppliers = await _oc.Suppliers.ListAsync<HSSupplier>(filters: $"ID={filterString}");
            }
            foreach(var supplier in suppliers.Items)
            {
                if(supplier?.xp?.NotificationRcpts?.Count() >0)
                {
                    // get orderworksheet for supplier order and fill in some information from buyer order worksheet
                    var supplierOrderWorksheet = await BuildSupplierOrderWorksheet(orderWorksheet, supplier.ID);
                    var supplierTemplateData = new EmailTemplate<OrderTemplateData>()
                    {
                        Data = SendgridMappers.GetOrderTemplateData(supplierOrderWorksheet.Order, supplierOrderWorksheet.LineItems),
                        Message = OrderSubmitEmailConstants.GetOrderSubmitText(orderWorksheet.Order.ID, supplierOrderWorksheet.Order.FromUser.FirstName, supplierOrderWorksheet.Order.FromUser.LastName, VerifiedUserType.supplier)
                    };

                    // SEB-Specific Data
                    ((OrderTemplateData)supplierTemplateData.Data).BillTo = new Address()
                    {
                        CompanyName = "SEB Vendor Portal - BES",
                        Street1 = "8646 Eagle Creek Circle",
                        Street2 = "Suite 107",
                        City = "Savage",
                        State = "MN",
                        Zip = "55378",
                        Phone = "877-771-9123",
                        xp =
                        {
                            Email = "accounting@sebvendorportal.com"
                        }
                    };

                    var supplierTos = new List<EmailAddress>();
                    foreach (var rcpt in supplier.xp.NotificationRcpts)
                    {
                        supplierTos.Add(new EmailAddress(rcpt));
                    };
                    await SendSingleTemplateEmailMultipleRcpts(_settings?.SendgridSettings?.FromEmail, supplierTos, _settings?.SendgridSettings?.OrderSubmitTemplateID, supplierTemplateData);
                }   
            }
        }

        private async Task<HSOrderWorksheet> BuildSupplierOrderWorksheet(HSOrderWorksheet orderWorksheet, string supplierID)
        {
            var supplierOrderWorksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Outgoing, $"{orderWorksheet.Order.ID}-{supplierID}");
            supplierOrderWorksheet.Order.BillingAddress = orderWorksheet.Order.BillingAddress;
            supplierOrderWorksheet.Order.FromUser = orderWorksheet.Order.FromUser;
            return supplierOrderWorksheet;
        }

        private async Task<List<EmailAddress>> GetSupplierEmails(HSOrderWorksheet orderWorksheet)
        {
            ListPage<HSSupplier> suppliers = null;
            if (orderWorksheet.Order.xp.SupplierIDs != null)
            {
                var filterString = String.Join("|", orderWorksheet.Order.xp.SupplierIDs);
                suppliers = await _oc.Suppliers.ListAsync<HSSupplier>(filters: $"ID={filterString}");
            }
            var supplierTos = new List<EmailAddress>();
            foreach (var supplier in suppliers.Items)
            {
                if (supplier?.xp?.NotificationRcpts?.Count() > 0)
                {
                    foreach (var rcpt in supplier.xp.NotificationRcpts)
                    {
                        supplierTos.Add(new EmailAddress(rcpt));
                    };
                }
            }
            return supplierTos;
        }

        private async Task<List<EmailAddress>> GetSellerEmails()
        {
            var sellerUsers = await _oc.AdminUsers.ListAsync<HSSellerUser>();
            var sellerTos = new List<EmailAddress>();
            foreach (var seller in sellerUsers.Items)
            {
                if (seller?.xp?.OrderEmails ?? false)
                {
                    sellerTos.Add(new EmailAddress(seller.Email));
                };
                if (seller?.xp?.AddtlRcpts?.Any() ?? false)
                {
                    foreach (var rcpt in seller.xp.AddtlRcpts)
                    {
                        sellerTos.Add(new EmailAddress(rcpt));
                    };
                };
            };
            return sellerTos;
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
                    DynamicText2 = lineItemEmailDisplayText?.DynamicText2
                }
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, email, _settings?.SendgridSettings?.LineItemStatusChangeTemplateID, templateData);
        }

        public async Task SendContactSupplierAboutProductEmail(ContactSupplierBody template)
        {
            var supplier = await _oc.Suppliers.GetAsync<HSSupplier>(template.Product.DefaultSupplierID);
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
                    Note = template?.BuyerRequest?.Comments
                }
            };
            await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, supplierEmail, _settings?.SendgridSettings?.ProductInformationRequestTemplateID, templateData);
            var sellerUsers = await _oc.AdminUsers.ListAllAsync<HSUser>(filters: $"xp.RequestInfoEmails=true");
            foreach (var sellerUser in sellerUsers)
            {
                await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, sellerUser.Email, _settings?.SendgridSettings?.ProductInformationRequestTemplateID, templateData);
                if (sellerUser.xp.AddtlRcpts.Any())
                {
                    foreach (var rcpt in sellerUser.xp.AddtlRcpts)
                    {
                        await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, rcpt, _settings?.SendgridSettings?.ProductInformationRequestTemplateID, templateData);
                    }
                }
            }
        }

        public async Task EmailVoidAuthorizationFailedAsync(HSPayment payment, string transactionID, HSOrder order, CreditCardVoidException ex)
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
                    ErrorJsonString = JsonConvert.SerializeObject(ex.ApiError)
                },
                Message = new EmailDisplayText()
                {
                    EmailSubject = "Manual intervention required for this order",
                    DynamicText = "Error encountered while trying to void authorization on this order. Please contact customer and help them manually void authorization"
                }
            };
            var toList = new List<EmailAddress>();
            var supportEmails = _settings?.SendgridSettings?.CriticalSupportEmails.Split(",");
            foreach (var email in supportEmails)
            {
                toList.Add(new EmailAddress { Email = email });
            }
            await SendSingleTemplateEmailMultipleRcpts(_settings?.SendgridSettings?.FromEmail, toList, _settings?.SendgridSettings?.CriticalSupportTemplateID, templateData);
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
                    DynamicText = supportCase.Message
                }
            };
            var recipient = SendgridMappers.DetermineRecipient(_settings, supportCase.Subject);
            await SendSingleTemplateEmailSingleRcptAttachment(_settings?.SendgridSettings?.FromEmail, recipient, _settings?.SendgridSettings?.CriticalSupportTemplateID, templateData, supportCase.File);
        }
    }
}