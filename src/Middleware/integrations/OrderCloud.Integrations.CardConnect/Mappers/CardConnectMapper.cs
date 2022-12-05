using System;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using OrderCloud.Integrations.CardConnect.Extensions;
using OrderCloud.Integrations.CardConnect.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.CardConnect.Mappers
{
    public static class CreditCardMapper
    {
        public static HSCreditCard Map(CCToken card, CardConnectAccountResponse response)
        {
            var cc = new HSCreditCard()
            {
                CardType = card.CardType,
                CardholderName = card.CardholderName,
                ExpirationDate = card.ExpirationDate.ToDateTime(),
                PartialAccountNumber = card.AccountNumber.ToCreditCardDisplay(),
                Token = response.token,
                xp = new HSCreditCardXP
                {
                    CCBillingAddress = card.CCBillingAddress,
                },
            };
            return cc;
        }
    }

    public static class BuyerCreditCardMapper
    {
        public static HSBuyerCreditCard Map(CCToken card, CardConnectAccountResponse response)
        {
            var cc = new HSBuyerCreditCard()
            {
                CardType = card.CardType,
                CardholderName = card.CardholderName,
                ExpirationDate = card.ExpirationDate.ToDateTime(),
                PartialAccountNumber = card.AccountNumber.ToCreditCardDisplay(),
                Token = response.token,
                Editable = true,
                xp = new HSCreditCardXP
                {
                    CCBillingAddress = card.CCBillingAddress,
                },
            };
            return cc;
        }
    }

    public static class CardConnectMapper
    {
        public static CardConnectAccountRequest Map(CCToken card, string currency)
        {
            var acct = new CardConnectAccountRequest()
            {
                currency = currency,
                account = card.AccountNumber,
            };
            return acct;
        }

        public static CardConnectAuthorizationRequest Map(BuyerCreditCard card, Order order, CCPayment payment, string merchantID, decimal amount)
        {
            var address = card.xp.CCBillingAddress;
            var req = new CardConnectAuthorizationRequest()
            {
                name = $"{card.CardholderName}",
                account = card.Token,
                address = address.Street1,
                amount = amount.ToString(),
                city = address.City,
                country = address.Country,
                currency = payment.Currency,
                cvv2 = payment.CVV,
                expiry = $"{card.ExpirationDate.Value:MMyyyy}",
                merchid = merchantID,
                orderid = order.ID,
                postal = address.Zip,
                region = address.State,
            };
            return req;
        }

        public static HSPaymentTransaction Map(Payment payment, CardConnectAuthorizationResponse response, Address ccBillingAddress = null)
        {
            var t = new HSPaymentTransaction()
            {
                Amount = payment.Amount,
                DateExecuted = DateTime.Now,
                ResultCode = response.authcode,
                ResultMessage = response.resptext,
                Succeeded = response.WasSuccessful(),
                Type = "CreditCard",
                xp = new TransactionXP
                {
                    CCTransactionResult = new CCTransactionResult
                    {
                        Succeeded = response.WasSuccessful(),
                        Amount = payment.Amount ?? 0,
                        TransactionID = response.retref, // retrieval reference number
                        ResponseCode = response.respcode,
                        AuthorizationCode = response.authcode,
                        AVSResponseCode = response.avsresp,
                        Message = response.resptext,
                        merchid = response.merchid,
                    },
                },
            };
            return t;
        }

        public static HSPaymentTransaction Map(Payment payment, CardConnectVoidResponse response)
        {
            var t = new HSPaymentTransaction()
            {
                Amount = payment.Amount,
                DateExecuted = DateTime.Now,
                ResultCode = response.authcode,
                ResultMessage = response.resptext,
                Succeeded = response.WasSuccessful(),
                Type = "CreditCardVoidAuthorization",
                xp = new TransactionXP
                {
                    CCTransactionResult = new CCTransactionResult
                    {
                        Succeeded = response.WasSuccessful(),
                        Amount = payment.Amount ?? 0,
                        TransactionID = response.retref, // retrieval reference number
                        ResponseCode = response.respcode,
                        AuthorizationCode = response.authcode,
                        AVSResponseCode = null,
                        Message = response.resptext,
                        merchid = response.merchid,
                    },
                },
            };
            return t;
        }

        public static HSPaymentTransaction Map(Payment payment, CardConnectRefundResponse response)
        {
            var t = new HSPaymentTransaction()
            {
                Amount = payment.Amount,
                DateExecuted = DateTime.Now,
                ResultCode = response.respstat,
                ResultMessage = response.resptext,
                Succeeded = response.WasSuccessful(),
                Type = "CreditCardRefund",
                xp = new TransactionXP
                {
                    CCTransactionResult = new CCTransactionResult
                    {
                        Succeeded = response.WasSuccessful(),
                        Amount = payment.Amount ?? 0,
                        TransactionID = response.retref, // retrieval reference number
                        ResponseCode = response.respcode,
                        AuthorizationCode = null,
                        AVSResponseCode = null,
                        Message = response.resptext,
                        merchid = response.merchid,
                    },
                },
            };
            return t;
        }

        public static HSPaymentTransaction Map(Payment payment, CardConnectCaptureResponse response)
        {
            var t = new HSPaymentTransaction()
            {
                Amount = payment.Amount,
                DateExecuted = DateTime.Now,
                ResultCode = response.respcode,
                ResultMessage = response.resptext,
                Succeeded = response.WasSuccessful(),
                Type = "CreditCardCapture",
                xp = new TransactionXP
                {
                    CCTransactionResult = new CCTransactionResult
                    {
                        Succeeded = response.WasSuccessful(),
                        Amount = payment.Amount ?? 0,
                        TransactionID = response.retref, // retrieval reference number
                        ResponseCode = response.respcode,
                        AuthorizationCode = null,
                        AVSResponseCode = null,
                        Message = response.resptext,
                        merchid = response.merchid,
                    },
                },
            };
            return t;
        }
    }

    public static class CreditCardAuthorizationExtensions
    {
        public static DateTime ToDateTime(this string value)
        {
            var month = value.Substring(0, 2).To<int>();
            switch (value.Length)
            {
                case 4:
                    {
                        var year = $"20{value.Substring(2, 2)}".To<int>();
                        return new DateTime(year, month, 1);
                    }

                case 6:
                    {
                        var year = value.Substring(2, 4).To<int>();
                        return new DateTime(year, month, 1);
                    }

                default:
                    throw new Exception("Invalid format: MMYY MMYYYY");
            }
        }

        public static ResponseStatus ToResponseStatus(this string value)
        {
            switch (value)
            {
                case "A":
                    return ResponseStatus.Approved;
                case "B":
                    return ResponseStatus.Retry;
                case "C":
                    return ResponseStatus.Declined;
                default:
                    throw new Exception($"Invalid response code: {value}");
            }
        }

        public static CVVResponse ToCvvResponse(this string value)
        {
            switch (value)
            {
                case "M":
                case "X": // not documented but returned on successful calls in testing
                    return CVVResponse.Valid;
                case "N":
                    return CVVResponse.Invalid;
                case "P":
                    return CVVResponse.NotProcessed;
                case "S":
                    return CVVResponse.NotPresent;
                case "U":
                    return CVVResponse.NotCertified;
                default:
                    throw new Exception($"Invalid response code: {value}");
            }
        }

        public static BinType ToBinType(this string value)
        {
            switch (value)
            {
                case "Corp": return BinType.Corporate;
                case "FSA+Prepaid": return BinType.FSAPrepaid;
                case "GSA+Purchase": return BinType.GSAPurchase;
                case "Prepaid": return BinType.Prepaid;
                case "Prepaid+Corp": return BinType.PrepaidCorporate;
                case "Prepaid+Purchase": return BinType.PrepaidPurchase;
                case "Purchase": return BinType.Purchase;
                default: return BinType.Invalid;
            }
        }
    }
}
