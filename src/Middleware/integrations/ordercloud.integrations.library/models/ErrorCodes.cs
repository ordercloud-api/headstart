using OrderCloud.Catalyst;
using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public static partial class ErrorCodes
    {
        public static IDictionary<string, ErrorCode> All { get; } = new Dictionary<string, ErrorCode>
        {
            { "NotFound", new ErrorCode("Not Found", "Resource requested was not found", 404) },
            { "Required", new ErrorCode("Required", "Field is required") },
            { "WriteFailure", new ErrorCode("Write Failure", "Failed to create record") },
            { "UnrecognizedType", new ErrorCode("UnrecognizedType", "Unrecognized type") },
            { "Blob.ConnectionString", new ErrorCode("InvalidConnectionString", "Invalid Connection String", 404)},
            { "Blob.Container", new ErrorCode("InvalidContainerString", "Invalid Container", 404)},
            { "Webhook.MissingHeader", new ErrorCode("MissingWebhookHeader", "Invalid Header", 401)},
        };

        public static partial class Auth
        {
            /// <summary>User does not have role(s) required to perform this action.</summary>
            public static readonly ErrorCode<InvalidHeaderError> InvalidHeader = All["Webhook.MissingHeader"] as ErrorCode<InvalidHeaderError>;
        }
    }
}
