using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Headstart.Common.Constants;
using Headstart.Common.Mappers;
using Headstart.Common.Models.Misc;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using Headstart.Models.Misc;
using Microsoft.WindowsAzure.Storage.Blob;
using OrderCloud.SDK;
using SendGrid;
using SendGrid.Helpers.Mail;
using Newtonsoft.Json;
using OrderCloud.Integrations.CardConnect;
using OrderCloud.Catalyst;
using System.Net;
using static Headstart.Common.Models.SendGridModels;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

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

        Task SendQuotePriceConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail);

		Task SendQuoteRequestConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail);
	}

	public class SendgridService : ISendgridService
	{
		private readonly AppSettings settings;
		private readonly IOrderCloudClient oc;
		private readonly ISendGridClient client;

		/// <summary>
		/// The IOC based constructor method for the SendgridService class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="ocClient"></param>
		/// <param name="client"></param>
		public SendgridService(AppSettings settings, IOrderCloudClient ocClient, ISendGridClient client)
		{
			try
			{				
				this.settings = settings;
				oc = ocClient;
				this.client = client;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		/// <summary>
		/// Public re-usable SendQuoteRequestConfirmationEmail task method
		/// </summary>
		/// <param name="order"></param>
		/// <param name="lineItem"></param>
		/// <param name="buyerEmail"></param>
		/// <returns></returns>
		public async Task SendQuoteRequestConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail)
		{
			var buyerTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
			{
				Data = SendgridMappers.GetQuoteOrderTemplateData(order, new List<HSLineItem> { lineItem }),
                Message = OrderSubmitEmailConstants.GetQuoteRequestConfirmationText(),
			};
            await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, buyerEmail, settings?.SendgridSettings?.QuoteOrderSubmitTemplateID, buyerTemplateData);
		}

		/// <summary>
		/// Public re-usable SendQuotePriceConfirmationEmail task method
		/// </summary>
		/// <param name="order"></param>
		/// <param name="lineItem"></param>
		/// <param name="buyerEmail"></param>
		/// <returns></returns>
		public async Task SendQuotePriceConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail)
		{
			var buyerTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
			{
				Data = SendgridMappers.GetQuoteOrderTemplateData(order, new List<HSLineItem> { lineItem }),
                Message = OrderSubmitEmailConstants.GetQuotePriceConfirmationText(),
			};
            await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, buyerEmail, settings?.SendgridSettings?.QuoteOrderSubmitTemplateID, buyerTemplateData);
		}

		/// <summary>
		/// Public re-usable SendSingleTemplateEmail task method
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="templateID"></param>
		/// <param name="templateData"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public virtual async Task SendSingleTemplateEmail(string from, string to, string templateID, object templateData)
		{
			Require.That(templateID != null, new ErrorCode("SendgridError", "Required Sengrid template ID not configured in app settings", HttpStatusCode.NotImplemented));
			{
				var fromEmail = new EmailAddress(from);
				var toEmail = new EmailAddress(to);
				var msg = MailHelper.CreateSingleTemplateEmail(fromEmail, toEmail, templateID, templateData);
                var response = await client.SendEmailAsync(msg);
				if(!response.IsSuccessStatusCode)
				{
					throw new Exception("Error sending sendgrid email");
				}
			}
		}

		/// <summary>
		/// Public re-usable SendSingleTemplateEmailMultipleRcpts task method
		/// </summary>
		/// <param name="from"></param>
		/// <param name="tos"></param>
		/// <param name="templateID"></param>
		/// <param name="templateData"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
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

		/// <summary>
		/// Public re-usable SendSingleTemplateEmailMultipleRcptsAttachment task method
		/// </summary>
		/// <param name="from"></param>
		/// <param name="tos"></param>
		/// <param name="templateID"></param>
		/// <param name="templateData"></param>
		/// <param name="fileReference"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
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

		/// <summary>
		/// Public re-usable SendSingleTemplateEmailSingleRcptAttachment task method
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="templateID"></param>
		/// <param name="templateData"></param>
		/// <param name="fileReference"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
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

		/// <summary>
		/// Public re-usable SendPasswordResetEmail task method
		/// </summary>
		/// <param name="messageNotification"></param>
		/// <returns></returns>
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
            await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, settings?.SendgridSettings?.PasswordResetTemplateID, templateData);
		}

		/// <summary>
		/// Public re-usable SendLineItemStatusChangeEmail task method
		/// </summary>
		/// <param name="order"></param>
		/// <param name="lineItemStatusChanges"></param>
		/// <param name="lineItems"></param>
		/// <param name="firstName"></param>
		/// <param name="lastName"></param>
		/// <param name="email"></param>
		/// <param name="lineItemEmailDisplayText"></param>
		/// <returns></returns>
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
            await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, email, settings?.SendgridSettings?.LineItemStatusChangeTemplateID, templateData);
		}

		/// <summary>
		/// Public re-usable SendLineItemStatusChangeEmailMultipleRcpts task method
		/// </summary>
		/// <param name="order"></param>
		/// <param name="lineItemStatusChanges"></param>
		/// <param name="lineItems"></param>
		/// <param name="tos"></param>
		/// <param name="lineItemEmailDisplayText"></param>
		/// <returns></returns>
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
            await SendSingleTemplateEmailMultipleRcpts(settings?.SendgridSettings?.FromEmail, tos, settings?.SendgridSettings?.LineItemStatusChangeTemplateID, templateData);
		}

		/// <summary>
		/// Public re-usable SendOrderSubmittedForApprovalEmail task method
		/// </summary>
		/// <param name="messageNotification"></param>
		/// <returns></returns>
		public async Task SendOrderSubmittedForApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
		{
			var order = messageNotification.EventBody.Order;
			var templateData = new EmailTemplate<OrderTemplateData>()
			{
				Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetRequestedApprovalText(),
			};
            await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, settings?.SendgridSettings?.OrderApprovalTemplateID, templateData);
		}

		/// <summary>
		/// Public re-usable SendOrderRequiresApprovalEmail task method
		/// </summary>
		/// <param name="messageNotification"></param>
		/// <returns></returns>
		public async Task SendOrderRequiresApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
		{
			var order = messageNotification.EventBody.Order;
			var templateData = new EmailTemplate<OrderTemplateData>()
			{
				Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
                Message = OrderSubmitEmailConstants.GetOrderRequiresApprovalText(),
			};
            await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, settings?.SendgridSettings?.OrderApprovalTemplateID, templateData);
		}

		/// <summary>
		/// Public re-usable SendNewUserEmail task method
		/// </summary>
		/// <param name="messageNotification"></param>
		/// <returns></returns>
		public async Task SendNewUserEmail(MessageNotification<PasswordResetEventBody> messageNotification)
		{
			var templateData = new EmailTemplate<NewUserData>()
			{
				Data = new NewUserData
				{
					FirstName = messageNotification?.Recipient?.FirstName,
					LastName = messageNotification?.Recipient?.LastName,
					PasswordRenewalAccessToken = messageNotification?.EventBody?.PasswordRenewalAccessToken,
                    BaseAppURL = settings?.UI?.BaseAdminUrl,
                    Username = messageNotification?.EventBody?.Username,
                },
			};
            await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, settings?.SendgridSettings?.NewUserTemplateID, templateData);
		}

		/// <summary>
		/// Public re-usable SendOrderApprovedEmail task method
		/// </summary>
		/// <param name="messageNotification"></param>
		/// <returns></returns>
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
            await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, settings?.SendgridSettings?.OrderApprovalTemplateID, templateData);
		}

		/// <summary>
		/// Public re-usable SendOrderDeclinedEmail task method
		/// </summary>
		/// <param name="messageNotification"></param>
		/// <returns></returns>
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
            await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, settings?.SendgridSettings?.OrderApprovalTemplateID, templateData);
		}

		/// <summary>
		/// Public re-usable SendOrderSubmitEmail task method
		/// </summary>
		/// <param name="orderWorksheet"></param>
		/// <returns></returns>
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

				//  send emails
                await SendSingleTemplateEmailMultipleRcpts(settings?.SendgridSettings?.FromEmail, sellerEmailList, settings?.SendgridSettings?.OrderSubmitTemplateID, sellerTemplateData);
                await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, orderWorksheet.Order.FromUser.Email, settings?.SendgridSettings?.OrderSubmitTemplateID, buyerTemplateData);
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

				//  send emails
                await SendSingleTemplateEmailMultipleRcpts(settings?.SendgridSettings?.FromEmail, supplierEmailList, settings?.SendgridSettings?.QuoteOrderSubmitTemplateID, supplierTemplateData);
                await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, orderWorksheet.Order.FromUser.Email, settings?.SendgridSettings?.QuoteOrderSubmitTemplateID, buyerTemplateData);
			}
		}
		
		/// <summary>
		/// Public re-usable SendLineItemStatusChangeEmail task method
		/// </summary>
		/// <param name="lineItemStatusChange"></param>
		/// <param name="lineItems"></param>
		/// <param name="firstName"></param>
		/// <param name="lastName"></param>
		/// <param name="email"></param>
		/// <param name="lineItemEmailDisplayText"></param>
		/// <returns></returns>
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
            await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, email, settings?.SendgridSettings?.LineItemStatusChangeTemplateID, templateData);
		}

		/// <summary>
		/// Public re-usable SendContactSupplierAboutProductEmail task method
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
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
            await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, supplierEmail, settings?.SendgridSettings?.ProductInformationRequestTemplateID, templateData);
            var sellerUsers = await oc.AdminUsers.ListAllAsync<HSUser>(filters: $"xp.RequestInfoEmails=true");
			foreach (var sellerUser in sellerUsers)
			{
                await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, sellerUser.Email, settings?.SendgridSettings?.ProductInformationRequestTemplateID, templateData);
				if (sellerUser.xp.AddtlRcpts.Any())
				{
					foreach (var rcpt in sellerUser.xp.AddtlRcpts)
					{
                        await SendSingleTemplateEmail(settings?.SendgridSettings?.FromEmail, rcpt, settings?.SendgridSettings?.ProductInformationRequestTemplateID, templateData);
					}
				}
			}
		}

		/// <summary>
		/// Public re-usable EmailVoidAuthorizationFailedAsync task method
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="transactionID"></param>
		/// <param name="order"></param>
		/// <param name="ex"></param>
		/// <returns></returns>
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
                    ErrorJsonString = JsonConvert.SerializeObject(ex.ApiError),
				},
				Message = new EmailDisplayText()
				{
					EmailSubject = "Manual intervention required for this order",
                    DynamicText = "Error encountered while trying to void authorization on this order. Please contact customer and help them manually void authorization",
                },
			};
			var toList = new List<EmailAddress>();
            var supportEmails = settings?.SendgridSettings?.CriticalSupportEmails.Split(",");
			foreach (var email in supportEmails)
			{
				toList.Add(new EmailAddress { Email = email });
			}

            await SendSingleTemplateEmailMultipleRcpts(settings?.SendgridSettings?.FromEmail, toList, settings?.SendgridSettings?.CriticalSupportTemplateID, templateData);
		}

		/// <summary>
		/// Public re-usable EmailGeneralSupportQueue task method
		/// </summary>
		/// <param name="supportCase"></param>
		/// <returns></returns>
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
            var recipient = SendgridMappers.DetermineRecipient(settings, supportCase.Subject);
            await SendSingleTemplateEmailSingleRcptAttachment(settings?.SendgridSettings?.FromEmail, recipient, settings?.SendgridSettings?.CriticalSupportTemplateID, templateData, supportCase.File);
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
		
		/// <summary>
		/// Public re-usable SendSupplierOrderSubmitEmails task method
		/// </summary>
		/// <param name="orderWorksheet"></param>
		/// <returns></returns>
		private async Task SendSupplierOrderSubmitEmails(HSOrderWorksheet orderWorksheet)
		{
			ListPage<HSSupplier> suppliers = null;
			if (orderWorksheet.Order.xp.SupplierIDs != null)
			{
                var filterString = string.Join("|", orderWorksheet.Order.xp.SupplierIDs);
                suppliers = await oc.Suppliers.ListAsync<HSSupplier>(filters: $"ID={filterString}");
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
                        Message = OrderSubmitEmailConstants.GetOrderSubmitText(orderWorksheet.Order.ID, supplierOrderWorksheet.Order.FromUser.FirstName, supplierOrderWorksheet.Order.FromUser.LastName, VerifiedUserType.supplier),
					};

					var supplierTos = new List<EmailAddress>();
					foreach (var rcpt in supplier.xp.NotificationRcpts)
					{
						supplierTos.Add(new EmailAddress(rcpt));
                    }

                    await SendSingleTemplateEmailMultipleRcpts(settings?.SendgridSettings?.FromEmail, supplierTos, settings?.SendgridSettings?.OrderSubmitTemplateID, supplierTemplateData);
				}   
			}
		}

		/// <summary>
		/// Public re-usable BuildSupplierOrderWorksheet task method
		/// </summary>
		/// <param name="orderWorksheet"></param>
		/// <param name="supplierID"></param>
		/// <returns>The HSOrderWorksheet object from the SendgridService.BuildSupplierOrderWorksheet process</returns>
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

		/// <summary>
		/// Public re-usable GetSellerEmails task method
		/// </summary>
		/// <returns>The list of the EmailAddress objects from the SendgridService.GetSellerEmails process</returns>
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
