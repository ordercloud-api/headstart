using System;
using System.Collections.Generic;
using System.Text;
using OrderCloud.SDK;

namespace ordercloud.integrations.library
{
    
    public class OrderCloudIntegrationException : Exception
    {
        public ApiError ApiError { get; }

        public OrderCloudIntegrationException(ApiError error)
        {
            ApiError = error;
        }

        public OrderCloudIntegrationException(IErrorCode errorCode, object data)
        {
            ApiError = new ApiError
            {
                ErrorCode = errorCode.Code,
                Message = errorCode.DefaultMessage,
                Data = data
            };
        }

        public OrderCloudIntegrationException(string errorCode, string message, object data)
        {
            ApiError = new ApiError
            {
                ErrorCode = errorCode,
                Message = message,
                Data = data
            };
        }

        public class NotFoundException : OrderCloudIntegrationException
        {
            public NotFoundException(string thingType, string interopID) : base("NotFound", "Object not found.", new { ObjectType = thingType, ObjectID = interopID }) { }
        }

        public class UserErrorException : OrderCloudIntegrationException
        {
            public UserErrorException(string message) : base("InvalidRequest", message, null) { }
        }
    }
}
