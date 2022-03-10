using System;
using SendGrid;
using System.Linq;
using OrderCloud.SDK;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Headstart.Common.Mappers;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Headstart.Common.Constants;
using Headstart.Models.Headstart;
using Headstart.Common.Models.Misc;
using Headstart.Common.Models.Headstart;
using Microsoft.WindowsAzure.Storage.Blob;
using ordercloud.integrations.cardconnect;
using static Headstart.Common.Models.SendGridModels;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.Common.Services
{
	public interface ISendgridService
	{
		Task SendSingleTemplateEmail(string from, string to, string templateID, object templateData);
		Task SendSingleTemplateEmailMultipleRcpts(string from, List<EmailAddress> tos, string templateID, object templateData);
		Task SendSingleTemplateEmailMultipleRcptsAttachment(string from, List<EmailAddress> tos, string templateID, object templateData, CloudAppendBlob fileReference, string fileName);
		Task SendSingleTemplateEmailSingleRcptAttachment(string from, string to, string templateID, object templateData, IFormFile fileReference);
		Task SendOrderSubmitEmail(HsOrderWorksheet orderData);
		Task SendNewUserEmail(MessageNotification<PasswordResetEventBody> payload);
		Task SendPasswordResetEmail(MessageNotification<PasswordResetEventBody> messageNotification);
		Task SendOrderRequiresApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification);
		Task SendOrderSubmittedForApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification);
		Task SendOrderApprovedEmail(MessageNotification<OrderSubmitEventBody> messageNotification);
		Task SendOrderDeclinedEmail(MessageNotification<OrderSubmitEventBody> messageNotification);
		Task SendLineItemStatusChangeEmail(HsOrder order, LineItemStatusChanges lineItemStatusChanges, List<HsLineItem> lineItems, string firstName, string lastName, string email, EmailDisplayText lineItemEmailDisplayText);
		Task SendLineItemStatusChangeEmailMultipleRcpts(HsOrder order, LineItemStatusChanges lineItemStatusChanges, List<HsLineItem> lineItems, List<EmailAddress> tos, EmailDisplayText lineItemEmailDisplayText);
		Task SendContactSupplierAboutProductEmail(ContactSupplierBody template);
		Task EmailVoidAuthorizationFailedAsync(HsPayment payment, string transactionID, HsOrder order, CreditCardVoidException ex);
		Task EmailGeneralSupportQueue(SupportCase supportCase);
		Task SendQuotePriceConfirmationEmail(HsOrder order, HsLineItem LineItem, string buyerEmail);
		Task SendQuoteRequestConfirmationEmail(HsOrder order, HsLineItem lineItem, string buyerEmail);
	}


	public class SendgridService : ISendgridService
	{
		private readonly AppSettings _settings;
		private readonly IOrderCloudClient _oc;
		private readonly ISendGridClient _client;
		private readonly WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

		public SendgridService(AppSettings settings, IOrderCloudClient ocClient, ISendGridClient client)
		{
			try
			{
				_oc = ocClient;
				_client = client;
				_settings = settings;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public async Task SendQuoteRequestConfirmationEmail(HsOrder order, HsLineItem lineItem, string buyerEmail)
		{
			try
			{
				var buyerTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
				{
					Data = SendgridMappers.GetQuoteOrderTemplateData(order, new List<HsLineItem> { lineItem }),
					Message = OrderSubmitEmailConstants.GetQuoteRequestConfirmationText()
				};
				await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, buyerEmail, _settings?.SendgridSettings?.QuoteOrderSubmitTemplateId, buyerTemplateData);
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public async Task SendQuotePriceConfirmationEmail(HsOrder order, HsLineItem lineItem, string buyerEmail)
		{
			try
			{
				var buyerTemplateData = new EmailTemplate<QuoteOrderTemplateData>()
				{
					Data = SendgridMappers.GetQuoteOrderTemplateData(order, new List<HsLineItem> { lineItem }),
					Message = OrderSubmitEmailConstants.GetQuotePriceConfirmationText()
				};
				await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, buyerEmail, _settings?.SendgridSettings?.QuoteOrderSubmitTemplateId, buyerTemplateData);
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public virtual async Task SendSingleTemplateEmail(string from, string to, string templateID, object templateData)
		{
			Require.That(templateID != null, new ErrorCode(@"SendgridError", @"The required Sengrid template ID not configured in app settings.", 501));
			{
				var fromEmail = new EmailAddress(from);
				var toEmail = new EmailAddress(to);
				var msg = MailHelper.CreateSingleTemplateEmail(fromEmail, toEmail, templateID, templateData);
				var response = await _client.SendEmailAsync(msg);
				if (!response.IsSuccessStatusCode)
				{
					throw new Exception(@"Error sending sendgrid email.");
				}
			}
		}

		public virtual async Task SendSingleTemplateEmailMultipleRcpts(string from, List<EmailAddress> tos, string templateID, object templateData)
		{
			Require.That(templateID != null, new ErrorCode(@"SendgridError", @"The required Sengrid template ID not configured in app settings.", 501));
			{
				var fromEmail = new EmailAddress(from);
				var msg = MailHelper.CreateSingleTemplateEmailToMultipleRecipients(fromEmail, tos, templateID, templateData);
				var response = await _client.SendEmailAsync(msg);
				if (!response.IsSuccessStatusCode)
				{
					throw new Exception(@"Error sending sendgrid email.");
				}
			}
		}

		public async Task SendSingleTemplateEmailMultipleRcptsAttachment(string from, List<EmailAddress> tos, string templateID, object templateData, CloudAppendBlob fileReference, string fileName)
		{
			Require.That(templateID != null, new ErrorCode(@"SendgridError", @"The required Sengrid template ID not configured in app settings.", 501));
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
					throw new Exception(@"Error sending sendgrid email.");
				}
			}
		}

		public async Task SendSingleTemplateEmailSingleRcptAttachment(string from, string to, string templateID, object templateData, IFormFile fileReference)
		{
			Require.That(templateID != null, new ErrorCode(@"SendgridError", @"The required Sengrid template ID not configured in app settings.", 501));
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
					throw new Exception(@"Error sending sendgrid email.");
				}
			}
		}

		public async Task SendPasswordResetEmail(MessageNotification<PasswordResetEventBody> messageNotification)
		{
			try
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
				await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.PasswordResetTemplateId, templateData);
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		private List<LineItemProductData> CreateTemplateProductList(List<HsLineItem> lineItems, LineItemStatusChanges lineItemStatusChanges)
		{
			//  First get line items that actually had a change
			var changedLiIds = lineItemStatusChanges.Changes.Where(change => change.Quantity > 0).Select(change => change.Id);
			var changedLineItems = changedLiIds.Select(i => lineItems.Single(l => l.ID == i));
			//  Now map to template data
			return changedLineItems.Select(lineItem =>
			{
				var lineItemStatusChange = lineItemStatusChanges.Changes.First(li => li.Id == lineItem.ID);
				return SendgridMappers.MapToTemplateProduct(lineItem, lineItemStatusChange, lineItemStatusChanges.Status);
			}).ToList();
		}

		public async Task SendLineItemStatusChangeEmail(HsOrder order, LineItemStatusChanges lineItemStatusChanges, List<HsLineItem> lineItems, string firstName, string lastName, string email, EmailDisplayText lineItemEmailDisplayText)
		{
			try
			{
				var productsList = CreateTemplateProductList(lineItems, lineItemStatusChanges);
				var templateData = new EmailTemplate<LineItemStatusChangeData>()
				{
					Data = new LineItemStatusChangeData
					{
						FirstName = firstName,
						LastName = lastName,
						Products = productsList,
						DateSubmitted = order?.DateSubmitted?.ToString(),
						OrderId = order.ID,
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
				await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, email, _settings?.SendgridSettings?.LineItemStatusChangeTemplateId, templateData);
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public async Task SendLineItemStatusChangeEmailMultipleRcpts(HsOrder order, LineItemStatusChanges lineItemStatusChanges, List<HsLineItem> lineItems, List<EmailAddress> tos, EmailDisplayText lineItemEmailDisplayText)
		{
			try
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
						OrderId = order.ID,
						Comments = order.Comments
					},
					Message = new EmailDisplayText
					{
						EmailSubject = lineItemEmailDisplayText?.EmailSubject,
						DynamicText = lineItemEmailDisplayText?.DynamicText,
						DynamicText2 = lineItemEmailDisplayText?.DynamicText2,
					}
				};
				await SendSingleTemplateEmailMultipleRcpts(_settings?.SendgridSettings?.FromEmail, tos, _settings?.SendgridSettings?.LineItemStatusChangeTemplateId, templateData);
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public async Task SendOrderSubmittedForApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
		{
			try
			{
				var order = messageNotification.EventBody.Order;
				var templateData = new EmailTemplate<OrderTemplateData>()
				{
					Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
					Message = OrderSubmitEmailConstants.GetRequestedApprovalText()
				};
				await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.OrderApprovalTemplateId, templateData);
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public async Task SendOrderRequiresApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
		{
			try
			{
				var order = messageNotification.EventBody.Order;
				var templateData = new EmailTemplate<OrderTemplateData>()
				{
					Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
					Message = OrderSubmitEmailConstants.GetOrderRequiresApprovalText()
				};
				await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.OrderApprovalTemplateId, templateData);
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public async Task SendNewUserEmail(MessageNotification<PasswordResetEventBody> messageNotification)
		{
			try
			{
				var templateData = new EmailTemplate<NewUserData>()
				{
					Data = new NewUserData
					{
						FirstName = messageNotification?.Recipient?.FirstName,
						LastName = messageNotification?.Recipient?.LastName,
						PasswordRenewalAccessToken = messageNotification?.EventBody?.PasswordRenewalAccessToken,
						BaseAppUrl = _settings?.UI?.BaseAdminUrl,
						Username = messageNotification?.EventBody?.Username
					}
				};
				await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.NewUserTemplateId, templateData);
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public async Task SendOrderApprovedEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
		{
			try
			{
				var order = messageNotification.EventBody.Order;
				var approval = messageNotification.EventBody.Approvals.FirstOrDefault();
				var templateData = new EmailTemplate<OrderTemplateData>()
				{
					Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
					Message = OrderSubmitEmailConstants.GetOrderApprovedText()
				};
				templateData.Data.Comments = approval.Comments;
				await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.OrderApprovalTemplateId, templateData);
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public async Task SendOrderDeclinedEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
		{
			try
			{
				var order = messageNotification.EventBody.Order;
				var approval = messageNotification.EventBody.Approvals.FirstOrDefault();
				var templateData = new EmailTemplate<OrderTemplateData>()
				{
					Data = SendgridMappers.GetOrderTemplateData(order, messageNotification.EventBody.LineItems),
					Message = OrderSubmitEmailConstants.GetOrderDeclinedText()
				};

				templateData.Data.Comments = approval.Comments;
				await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, messageNotification?.Recipient?.Email, _settings?.SendgridSettings?.OrderApprovalTemplateId, templateData);
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public async Task SendOrderSubmitEmail(HsOrderWorksheet orderWorksheet)
		{
			try
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
					await SendSingleTemplateEmailMultipleRcpts(_settings?.SendgridSettings?.FromEmail, sellerEmailList, _settings?.SendgridSettings?.OrderSubmitTemplateId, sellerTemplateData);
					await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, orderWorksheet.Order.FromUser.Email, _settings?.SendgridSettings?.OrderSubmitTemplateId, buyerTemplateData);
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

					await SendSingleTemplateEmailMultipleRcpts(_settings?.SendgridSettings?.FromEmail, supplierEmailList, _settings?.SendgridSettings?.QuoteOrderSubmitTemplateId, supplierTemplateData);
					await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, orderWorksheet.Order.FromUser.Email, _settings?.SendgridSettings?.QuoteOrderSubmitTemplateId, buyerTemplateData);
				}
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		private async Task SendSupplierOrderSubmitEmails(HsOrderWorksheet orderWorksheet)
		{
			try
			{
				ListPage<HsSupplier> suppliers = null;
				if (orderWorksheet.Order.xp.SupplierIds != null)
				{
					var filterString = string.Join(@"|", orderWorksheet.Order.xp.SupplierIds);
					suppliers = await _oc.Suppliers.ListAsync<HsSupplier>(filters: $@"ID={filterString}");
				}
				foreach (var supplier in suppliers.Items)
				{
					if (supplier?.xp?.NotificationRcpts?.Count() > 0)
					{
						// Get orderworksheet for supplier order and fill in some information from buyer order worksheet
						var supplierOrderWorksheet = await BuildSupplierOrderWorksheet(orderWorksheet, supplier.ID);
						var supplierTemplateData = new EmailTemplate<OrderTemplateData>()
						{
							Data = SendgridMappers.GetOrderTemplateData(supplierOrderWorksheet.Order, supplierOrderWorksheet.LineItems),
							Message = OrderSubmitEmailConstants.GetOrderSubmitText(orderWorksheet.Order.ID, supplierOrderWorksheet.Order.FromUser.FirstName, supplierOrderWorksheet.Order.FromUser.LastName, VerifiedUserType.supplier)
						};

						// SEB-Specific Data
						supplierTemplateData.Data.BillTo = new Address()
						{
							CompanyName = @"SEB Vendor Portal - BES",
							Street1 = @"8646 Eagle Creek Circle",
							Street2 = @"Suite 107",
							City = @"Savage",
							State = @"MN",
							Zip = @"55378",
							Phone = @"877-771-9123",
							xp =
							{
								Email = @"accounting@sebvendorportal.com"
							}
						};

						var supplierTos = (from rcpt in supplier.xp.NotificationRcpts
							select new EmailAddress(rcpt)).ToList();
						await SendSingleTemplateEmailMultipleRcpts(_settings?.SendgridSettings?.FromEmail, supplierTos, _settings?.SendgridSettings?.OrderSubmitTemplateId, supplierTemplateData);
					}
				}
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		private async Task<HsOrderWorksheet> BuildSupplierOrderWorksheet(HsOrderWorksheet orderWorksheet, string supplierID)
		{
			var supplierOrderWorksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Outgoing, $"{orderWorksheet.Order.ID}-{supplierID}");
			supplierOrderWorksheet.Order.BillingAddress = orderWorksheet.Order.BillingAddress;
			supplierOrderWorksheet.Order.FromUser = orderWorksheet.Order.FromUser;
			return supplierOrderWorksheet;
		}

		private async Task<List<EmailAddress>> GetSupplierEmails(HsOrderWorksheet orderWorksheet)
		{
			var supplierTos = new List<EmailAddress>();
			try
			{
				ListPage<HsSupplier> suppliers = null;
				if (orderWorksheet.Order.xp.SupplierIds != null)
				{
					var filterString = string.Join(@"|", orderWorksheet.Order.xp.SupplierIds);
					suppliers = await _oc.Suppliers.ListAsync<HsSupplier>(filters: $@"ID={filterString}");
				}

				supplierTos.AddRange(from HsSupplier supplier in suppliers.Items
					where supplier?.xp?.NotificationRcpts?.Count() > 0
					from string rcpt in supplier.xp.NotificationRcpts
					select new EmailAddress(rcpt));
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
			return supplierTos;
		}

		private async Task<List<EmailAddress>> GetSellerEmails()
		{
			var sellerTos = new List<EmailAddress>();
			try
			{
				var sellerUsers = await _oc.AdminUsers.ListAsync<HsSellerUser>();
				foreach (var seller in sellerUsers.Items)
				{
					if (seller?.xp?.OrderEmails ?? false)
					{
						sellerTos.Add(new EmailAddress(seller.Email));
					}
					if (seller?.xp?.AddtlRcpts?.Any() ?? false)
					{
						sellerTos.AddRange(from string rcpt in seller.xp.AddtlRcpts
							select new EmailAddress(rcpt));
					}
				}
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
			return sellerTos;
		}

		public async Task SendLineItemStatusChangeEmail(LineItemStatusChange lineItemStatusChange, List<HsLineItem> lineItems, string firstName, string lastName, string email, EmailDisplayText lineItemEmailDisplayText)
		{
			try
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
				await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, email, _settings?.SendgridSettings?.LineItemStatusChangeTemplateId, templateData);
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public async Task SendContactSupplierAboutProductEmail(ContactSupplierBody template)
		{
			try
			{
				var supplier = await _oc.Suppliers.GetAsync<HsSupplier>(template.Product.DefaultSupplierID);
				var supplierEmail = supplier.xp.SupportContact.Email;
				var templateData = new EmailTemplate<ProductInformationRequestData>()
				{
					Data = new ProductInformationRequestData
					{
						ProductId = template?.Product?.ID,
						ProductName = template?.Product?.Name,
						FirstName = template?.BuyerRequest?.FirstName,
						LastName = template?.BuyerRequest?.LastName,
						Location = template?.BuyerRequest?.BuyerLocation,
						Phone = template?.BuyerRequest?.Phone,
						Email = template?.BuyerRequest?.Email,
						Note = template?.BuyerRequest?.Comments
					}
				};
				await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, supplierEmail, _settings?.SendgridSettings?.ProductInformationRequestTemplateId, templateData);

				var sellerUsers = await _oc.AdminUsers.ListAllAsync<HsUser>(filters: @"xp.RequestInfoEmails=true");
				foreach (var sellerUser in sellerUsers)
				{
					await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, sellerUser.Email, _settings?.SendgridSettings?.ProductInformationRequestTemplateId, templateData);
					if (!sellerUser.xp.AddtlRcpts.Any())
					{
						continue;
					}
					foreach (var rcpt in sellerUser.xp.AddtlRcpts)
					{
						await SendSingleTemplateEmail(_settings?.SendgridSettings?.FromEmail, rcpt, _settings?.SendgridSettings?.ProductInformationRequestTemplateId, templateData);
					}
				}
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public async Task EmailVoidAuthorizationFailedAsync(HsPayment payment, string transactionID, HsOrder order, CreditCardVoidException ex)
		{
			try
			{
				var templateData = new EmailTemplate<SupportTemplateData>()
				{
					Data = new SupportTemplateData
					{
						OrderId = order.ID,
						DynamicPropertyName1 = @"BuyerID",
						DynamicPropertyValue1 = order.FromCompanyID,
						DynamicPropertyName2 = @"Username",
						DynamicPropertyValue2 = order.FromUser.Username,
						DynamicPropertyName3 = @"PaymentID",
						DynamicPropertyValue3 = payment.ID,
						DynamicPropertyName4 = @"TransactionID",
						DynamicPropertyValue4 = transactionID,
						ErrorJsonString = JsonConvert.SerializeObject(ex.ApiError)
					},
					Message = new EmailDisplayText()
					{
						EmailSubject = @"Manual intervention required for this order.",
						DynamicText = @"An Error was encountered while trying to void authorization on this order. Please contact customer and help them manually void authorization."
					}
				};

				var toList = new List<EmailAddress>();
				var supportEmails = _settings?.SendgridSettings?.CriticalSupportEmails.Split(",");
				toList.AddRange(from email in supportEmails
					select new EmailAddress { Email = email });
				await SendSingleTemplateEmailMultipleRcpts(_settings?.SendgridSettings?.FromEmail, toList, _settings?.SendgridSettings?.CriticalSupportTemplateId, templateData);
			}
			catch (Exception ex1)
			{
				var ResponseBody = ex != null ? JsonConvert.SerializeObject(ex) : string.Empty;
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), ResponseBody,
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex1.Message, ex1.StackTrace);
			}
		}

		public async Task EmailGeneralSupportQueue(SupportCase supportCase)
		{
			try
			{
				var templateData = new EmailTemplate<SupportTemplateData>()
				{
					Data = new SupportTemplateData
					{
						DynamicPropertyName1 = @"FirstName",
						DynamicPropertyValue1 = supportCase.FirstName,
						DynamicPropertyName2 = @"LastName",
						DynamicPropertyValue2 = supportCase.LastName,
						DynamicPropertyName3 = @"Email",
						DynamicPropertyValue3 = supportCase.Email,
						DynamicPropertyName4 = @"Vendor",
						DynamicPropertyValue4 = supportCase.Vendor ?? @"N/A",
					},
					Message = new EmailDisplayText()
					{
						EmailSubject = supportCase.Subject,
						DynamicText = supportCase.Message
					}
				};
				var recipient = SendgridMappers.DetermineRecipient(_settings, supportCase.Subject);
				await SendSingleTemplateEmailSingleRcptAttachment(_settings?.SendgridSettings?.FromEmail, recipient, _settings?.SendgridSettings?.CriticalSupportTemplateId, templateData, supportCase.File);
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}
	}
}