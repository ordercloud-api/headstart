using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Headstart.Common.Constants;
using Headstart.Common.Models;
using Headstart.Common.Settings;
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
        private readonly SendGridSettings sendGridSettings;
        private readonly UI uiSettings;
        private readonly IOrderCloudClient orderCloudClient;
        private readonly ISendGridClient sendGridClient;

        public SendGridService(SendGridSettings sendGridSettings, UI uiSettings, IOrderCloudClient orderCloudClient, ISendGridClient sendGridClient)
        {
            this.orderCloudClient = orderCloudClient;
            this.sendGridClient = sendGridClient;
            this.sendGridSettings = sendGridSettings;
            this.uiSettings = uiSettings;
        }

        public async Task SendQuoteRequestConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail)
        {
            var buyerTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
            {
                Data = SendgridMappers.GetQuoteOrderTemplateData(order, new List<HSLineItem> { lineItem }),
                Message = OrderSubmitEmailConstants.GetQuoteRequestConfirmationText(),
            };
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, buyerEmail, sendGridSettings?.QuoteOrderSubmitTemplateID, buyerTemplateData);
        }

        public async Task SendQuotePriceConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail)
        {
            var buyerTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
            {
                Data = SendgridMappers.GetQuoteOrderTemplateData(order, new List<HSLineItem> { lineItem }),
                Message = OrderSubmitEmailConstants.GetQuotePriceConfirmationText(),
            };
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, buyerEmail, sendGridSettings?.QuoteOrderSubmitTemplateID, buyerTemplateData);
        }

        public virtual async Task SendSingleTemplateEmail(string from, string to, string templateID, object templateData)
        {
            if (string.IsNullOrEmpty(templateID))
            {
                Console.WriteLine("Skipped email as required template ID was not provided");
                return;
            }

            var fromEmail = new EmailAddress(from);
            var toEmail = new EmailAddress(to);
            var msg = MailHelper.CreateSingleTemplateEmail(fromEmail, toEmail, templateID, templateData);
            var response = await sendGridClient.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error sending sendgrid email");
            }
        }

        public virtual async Task SendSingleTemplateEmailMultipleRcpts(string from, List<string> tos, string templateID, object templateData)
        {
            if (string.IsNullOrEmpty(templateID))
            {
                Console.WriteLine("Skipped email as required template ID was not provided");
                return;
            }

            var fromEmail = new EmailAddress(from);
            var toEmails = tos.Select(email => new EmailAddress(email)).ToList();
            var msg = MailHelper.CreateSingleTemplateEmailToMultipleRecipients(fromEmail, toEmails, templateID, templateData);
            var response = await sendGridClient.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error sending sendgrid email");
            }
        }

        public async Task SendSingleTemplateEmailMultipleRcptsAttachment(string from, List<string> tos, string templateID, object templateData, CloudAppendBlob fileReference, string fileName)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(templateID) || tos == null || tos.Count == 0)
            {
                Console.WriteLine("\nSkipping email send, required variables not defined\n");
            }

            var fromEmail = new EmailAddress(from);
            var toEmails = tos.Select(email => new EmailAddress(email)).ToList();
            var msg = MailHelper.CreateSingleTemplateEmailToMultipleRecipients(fromEmail, toEmails, templateID, templateData);
            using (var stream = await fileReference.OpenReadAsync())
            {
                await msg.AddAttachmentAsync(fileName, stream);
            }

            var response = await sendGridClient.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error sending sendgrid email");
            }
        }

        public async Task SendSingleTemplateEmailSingleRcptAttachment(string from, string to, string templateID, object templateData, IFormFile fileReference)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || string.IsNullOrEmpty(templateID))
            {
                Console.WriteLine("\nSkipping email send, required variables not defined\n");
                return;
            }

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

            var response = await sendGridClient.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error sending sendgrid email");
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
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendGridSettings?.PasswordResetTemplateID, templateData);
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
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, email, sendGridSettings?.LineItemStatusChangeTemplateID, templateData);
        }

        public async Task SendLineItemStatusChangeEmailMultipleRcpts(HSOrder order, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItems, List<string> tos, EmailDisplayText lineItemEmailDisplayText)
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
            await SendSingleTemplateEmailMultipleRcpts(sendGridSettings?.FromEmail, tos, sendGridSettings?.LineItemStatusChangeTemplateID, templateData);
        }

        public async Task SendOrderSubmittedForApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            var order = messageNotification.EventBody.Order;
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetRequestedApprovalText(),
            };
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendGridSettings?.OrderApprovalTemplateID, templateData);
        }

        public async Task SendOrderRequiresApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            var order = messageNotification.EventBody.Order;
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetOrderRequiresApprovalText(),
            };
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendGridSettings?.OrderApprovalTemplateID, templateData);
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
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendGridSettings?.NewUserTemplateID, templateData);
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
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendGridSettings?.OrderApprovalTemplateID, templateData);
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
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendGridSettings?.OrderApprovalTemplateID, templateData);
        }

        public async Task SendOrderReturnApprovedEmail(MessageNotification<OrderReturnEventBody> messageNotification)
        {
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = BuildOrderReturnData(messageNotification),
                Message = new EmailDisplayText
                {
                    EmailSubject = "Your order return has been approved",
                    DynamicText = "Your order return has been approved. Once the seller has received the return items they will process a refund for the approved amount.",
                },
            };
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendGridSettings?.OrderReturnTemplateID, templateData);
        }

        public async Task SendOrderReturnCompletedEmail(MessageNotification<OrderReturnEventBody> messageNotification)
        {
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = BuildOrderReturnData(messageNotification),
                Message = new EmailDisplayText
                {
                    EmailSubject = "Your order return is complete",
                    DynamicText = "Your order return is now complete. A refund has been issued for the amount approved. It may take a few days to appear in your account.",
                },
            };
            Console.WriteLine($"\nRECIPIENT: {messageNotification.Recipient?.Email}\n");
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendGridSettings?.OrderReturnTemplateID, templateData);
        }

        public async Task SendOrderReturnDeclinedEmail(MessageNotification<OrderReturnEventBody> messageNotification)
        {
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = BuildOrderReturnData(messageNotification),
                Message = new EmailDisplayText
                {
                    EmailSubject = "Your order return has been declined",
                    DynamicText = "Your return has been declined. If you think this was done in error, please contact the seller before resubmitting.",
                },
            };
            Console.WriteLine($"\nRECIPIENT: {messageNotification.Recipient?.Email}\n");
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendGridSettings?.OrderReturnTemplateID, templateData);
        }

        public async Task SendOrderReturnSubmittedForApprovalEmail(MessageNotification<OrderReturnEventBody> messageNotification)
        {
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = BuildOrderReturnData(messageNotification),
                Message = new EmailDisplayText
                {
                    EmailSubject = "Your order return has been submitted",
                    DynamicText = "Your order return has been submitted and is now awaiting approval from the seller",
                },
            };
            Console.WriteLine($"\nRECIPIENT: {messageNotification.Recipient?.Email}\n");
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendGridSettings?.OrderReturnTemplateID, templateData);
        }

        public async Task SendOrderReturnSubmittedForYourApprovalEmail(MessageNotification<OrderReturnEventBody> messageNotification)
        {
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = BuildOrderReturnData(messageNotification),
                Message = new EmailDisplayText
                {
                    EmailSubject = "An order return is awaiting your approval",
                    DynamicText = "A buyer user has submitted a return that requires your approval. Please log into the admin application to approve, approve with changes, or decline the return.",
                },
            };
            Console.WriteLine($"\nRECIPIENT: {messageNotification.Recipient?.Email}\n");
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendGridSettings?.OrderReturnTemplateID, templateData);
        }

        public async Task SendOrderReturnSubmittedForYourApprovalHasBeenApprovedEmail(MessageNotification<OrderReturnEventBody> messageNotification)
        {
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = BuildOrderReturnData(messageNotification),
                Message = new EmailDisplayText
                {
                    EmailSubject = "An order submitted for your approval has been approved",
                    DynamicText = "An order that was previously awaiting your approval has been approved. This may have been completed by you or someone else in the approving group.",
                },
            };
            Console.WriteLine($"\nRECIPIENT: {messageNotification.Recipient?.Email}\n");
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendGridSettings?.OrderReturnTemplateID, templateData);
        }

        public async Task SendOrderReturnSubmittedForYourApprovalHasBeenDeclinedEmail(MessageNotification<OrderReturnEventBody> messageNotification)
        {
            var templateData = new EmailTemplate<OrderTemplateData>()
            {
                Data = BuildOrderReturnData(messageNotification),
                Message = new EmailDisplayText
                {
                    EmailSubject = "An order submitted for your approval has been declined",
                    DynamicText = "An order that was previously awaiting your approval has been declined. This may have been completed by you or someone else in the approving group.",
                },
            };
            Console.WriteLine($"\nRECIPIENT: {messageNotification.Recipient?.Email}\n");
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, messageNotification?.Recipient?.Email, sendGridSettings?.OrderReturnTemplateID, templateData);
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
                await SendSingleTemplateEmailMultipleRcpts(sendGridSettings?.FromEmail, sellerEmailList, sendGridSettings?.OrderSubmitTemplateID, sellerTemplateData);
                await SendSingleTemplateEmail(sendGridSettings?.FromEmail, orderWorksheet.Order.FromUser.Email, sendGridSettings?.OrderSubmitTemplateID, buyerTemplateData);
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
                await SendSingleTemplateEmailMultipleRcpts(sendGridSettings?.FromEmail, supplierEmailList, sendGridSettings?.QuoteOrderSubmitTemplateID, supplierTemplateData);
                await SendSingleTemplateEmail(sendGridSettings?.FromEmail, orderWorksheet.Order.FromUser.Email, sendGridSettings?.QuoteOrderSubmitTemplateID, buyerTemplateData);
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
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, email, sendGridSettings?.LineItemStatusChangeTemplateID, templateData);
        }

        public async Task SendContactSupplierAboutProductEmail(ContactSupplierBody template)
        {
            var supplier = await orderCloudClient.Suppliers.GetAsync<HSSupplier>(template.Product.DefaultSupplierID);
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
            await SendSingleTemplateEmail(sendGridSettings?.FromEmail, supplierEmail, sendGridSettings?.ProductInformationRequestTemplateID, templateData);
            var sellerUsers = await orderCloudClient.AdminUsers.ListAllAsync<HSUser>(filters: $"xp.RequestInfoEmails=true");
            foreach (var sellerUser in sellerUsers)
            {
                await SendSingleTemplateEmail(sendGridSettings?.FromEmail, sellerUser.Email, sendGridSettings?.ProductInformationRequestTemplateID, templateData);
                if (sellerUser.xp.AddtlRcpts.Any())
                {
                    foreach (var rcpt in sellerUser.xp.AddtlRcpts)
                    {
                        await SendSingleTemplateEmail(sendGridSettings?.FromEmail, rcpt, sendGridSettings?.ProductInformationRequestTemplateID, templateData);
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

            var supportEmails = sendGridSettings?.CriticalSupportEmails.Split(",")?.ToList();

            await SendSingleTemplateEmailMultipleRcpts(sendGridSettings?.FromEmail, supportEmails, sendGridSettings?.CriticalSupportTemplateID, templateData);
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
            var recipient = SendgridMappers.DetermineRecipient(sendGridSettings, supportCase.Subject);
            await SendSingleTemplateEmailSingleRcptAttachment(sendGridSettings?.FromEmail, recipient, sendGridSettings?.CriticalSupportTemplateID, templateData, supportCase.File);
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
                suppliers = await orderCloudClient.Suppliers.ListAsync<HSSupplier>(filters: $"ID={filterString}");
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

                    await SendSingleTemplateEmailMultipleRcpts(sendGridSettings?.FromEmail, supplier.xp.NotificationRcpts, sendGridSettings?.OrderSubmitTemplateID, supplierTemplateData);
                }
            }
        }

        private async Task<HSOrderWorksheet> BuildSupplierOrderWorksheet(HSOrderWorksheet orderWorksheet, string supplierID)
        {
            var supplierOrderWorksheet = await orderCloudClient.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Outgoing, $"{orderWorksheet.Order.ID}-{supplierID}");
            supplierOrderWorksheet.Order.BillingAddress = orderWorksheet.Order.BillingAddress;
            supplierOrderWorksheet.Order.FromUser = orderWorksheet.Order.FromUser;
            return supplierOrderWorksheet;
        }

        private OrderReturnTemplateData BuildOrderReturnData(MessageNotification<OrderReturnEventBody> messageNotification)
        {
            var order = messageNotification.EventBody.Order;
            var lineitems = messageNotification.EventBody.LineItems;
            var orderReturn = messageNotification.EventBody.OrderReturn;
            var orderApprovals = messageNotification.EventBody.Approvals;
            var data = SendgridMappers.GetOrderReturnTemplateData(order, lineitems, orderReturn, orderApprovals);
            return data;
        }

        private async Task<List<string>> GetSupplierEmails(HSOrderWorksheet orderWorksheet)
        {
            ListPage<HSSupplier> suppliers = null;
            if (orderWorksheet.Order.xp.SupplierIDs != null)
            {
                var filterString = string.Join("|", orderWorksheet.Order.xp.SupplierIDs);
                suppliers = await orderCloudClient.Suppliers.ListAsync<HSSupplier>(filters: $"ID={filterString}");
            }

            var supplierTos = new List<string>();
            foreach (var supplier in suppliers.Items)
            {
                if (supplier?.xp?.NotificationRcpts?.Any() ?? false)
                {
                    supplierTos.AddRange(supplier.xp.NotificationRcpts);
                }
            }

            return supplierTos;
        }

        private async Task<List<string>> GetSellerEmails()
        {
            var sellerUsers = await orderCloudClient.AdminUsers.ListAsync<HSSellerUser>();
            var sellerTos = new List<string>();

            foreach (var seller in sellerUsers.Items)
            {
                if (seller?.xp?.OrderEmails ?? false)
                {
                    sellerTos.Add(seller.Email);
                }

                if (seller?.xp?.AddtlRcpts?.Any() ?? false)
                {
                    sellerTos.AddRange(seller.xp.AddtlRcpts);
                }
            }

            return sellerTos;
        }
    }
}
