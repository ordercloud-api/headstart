using ordercloud.integrations.library;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Headstart.Common.Services
{
	public class ZohoException : Exception
	{
		public string ErrorCode { get; set; }
		public HttpStatusCode? HttpStatus { get; set; }

		public ZohoException(ZohoError error, HttpStatusCode httpStatus) : base(error.message)
        {
			ErrorCode = error.code;
			HttpStatus = httpStatus;
        }
	}

	public class ZohoError
    {
		public string code { get; set; }
		public string message { get; set; }
    }
}
