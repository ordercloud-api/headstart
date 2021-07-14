using System;
using System.Collections.Generic;
using System.Text;
using OrderCloud.SDK;

namespace ordercloud.integrations.cardconnect
{
    public class CreditCardAuthorizationException : Exception
    {
        public ApiError ApiError { get; }
        public CardConnectAuthorizationResponse Response { get; }

        public CreditCardAuthorizationException(ApiError error, CardConnectAuthorizationResponse response)
        {
            ApiError = error;
            Response = response;
        }

        public CreditCardAuthorizationException(string errorCode, string message, CardConnectAuthorizationResponse data)
        {
            ApiError = new ApiError()
            {
                Data = data,
                ErrorCode = errorCode,
                Message = message
            };
            Response = data;
        }
    }

    public class CardConnectInquireException : Exception
    {
        public ApiError ApiError { get; }
        public CardConnectInquireResponse Response { get; }

        public CardConnectInquireException(ApiError error, CardConnectInquireResponse response)
        {
            ApiError = error;
            Response = response;
        }

        public CardConnectInquireException(string errorCode, string message, CardConnectInquireResponse data)
        {
            ApiError = new ApiError()
            {
                Data = data,
                ErrorCode = errorCode,
                Message = message
            };
            Response = data;
        }
    }

    public class CreditCardVoidException : Exception
    {
        public ApiError ApiError { get; }
        public CardConnectVoidResponse Response { get; }

        public CreditCardVoidException(ApiError error, CardConnectVoidResponse response)
        {
            ApiError = error;
            Response = response;
        }

        public CreditCardVoidException(string errorCode, string message, CardConnectVoidResponse data)
        {
            ApiError = new ApiError()
            {
                Data = data,
                ErrorCode = errorCode,
                Message = message
            };
            Response = data;
        }
    }

    public class CreditCardRefundException : Exception
    {
        public ApiError ApiError { get; }
        public CardConnectRefundResponse Response { get; }

        public CreditCardRefundException(ApiError error, CardConnectRefundResponse response)
        {
            ApiError = error;
            Response = response;
        }

        public CreditCardRefundException(string errorCode, string message, CardConnectRefundResponse data)
        {
            ApiError = new ApiError()
            {
                Data = data,
                ErrorCode = errorCode,
                Message = message
            };
            Response = data;
        }
    }
}
