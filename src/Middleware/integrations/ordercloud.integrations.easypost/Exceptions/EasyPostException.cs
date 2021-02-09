using ordercloud.integrations.library;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.easypost.Exceptions
{
	public class EasyPostException : OrderCloudIntegrationException
	{
		public EasyPostException(EasyPostApiError error) : base(
				new ApiError { 
					ErrorCode = $"EasyPost.{error.error.code}",
					Message = error.error.message,
					Data = error.error
				}
			) { }
	}

	public class EasyPostApiError
    {
		public ErrorObject error { get; set; }
	}

	public class EasyPostError
    {
		public ErrorObject error { get; set; }
    }

	public class ErrorObject
    {
		public string code { get; set; } // https://www.easypost.com/errors-guide#error-codes
		public string message { get; set; }
		public FieldErrorObject[] errors { get; set; }
	}

	public class FieldErrorObject
    {
		public string field { get; set; }
		public string message { get; set; }
    }
}
