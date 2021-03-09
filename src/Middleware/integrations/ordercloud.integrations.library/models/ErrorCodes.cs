using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public static partial class ErrorCodes
    {
        public static IDictionary<string, ErrorCode> All { get; } = new Dictionary<string, ErrorCode>
        {
            { "NotFound", new ErrorCode("Not Found", 404, "Resource requested was not found") },
            { "Required", new ErrorCode("Required", 400, "Field is required") },
            { "WriteFailure", new ErrorCode("Write Failure", 400, "Failed to create record") },
            { "UnrecognizedType", new ErrorCode("UnrecognizedType", 400, "Unrecognized type") },
            { "Blob.ConnectionString", new ErrorCode("InvalidConnectionString", 404, "Invalid Connection String")},
            { "Blob.Container", new ErrorCode("InvalidContainerString", 404, "Invalid Container")},
            { "Webhook.MissingHeader", new ErrorCode("MissingWebhookHeader", 401, "Invalid Header")},
        };

        public static partial class Auth
        {
            /// <summary>User does not have role(s) required to perform this action.</summary>
            public static readonly ErrorCode<InvalidHeaderError> InvalidHeader = All["Webhook.MissingHeader"] as ErrorCode<InvalidHeaderError>;
        }
    }
}
