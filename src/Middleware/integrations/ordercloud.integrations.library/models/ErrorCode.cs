using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public interface IErrorCode
    {
        string Code { get; set; }
        int HttpStatus { get; set; }
        string DefaultMessage { get; set; }
    }
    public class ErrorCode : IErrorCode
    {
        public ErrorCode(string code, int httpStatus, string defaultMessage)
        {
            Code = code;
            HttpStatus = httpStatus;
            DefaultMessage = defaultMessage;
        }

        public string Code { get; set; }
        public int HttpStatus { get; set; }
        public string DefaultMessage { get; set; }
    }

    public class ErrorCode<TData> : ErrorCode
    {
        public ErrorCode(string code, int httpStatus, string defaultMessage) : base(code, httpStatus, defaultMessage) { }
    }
}
