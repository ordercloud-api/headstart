using System.Linq;
using System.Net;
using Headstart.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using OrderCloud.SDK;

namespace Headstart.Common.Attributes
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new ValidationFailedResult(context.ModelState);
            }

            base.OnActionExecuting(context);
        }
    }

    public class ValidationFailedResult : ObjectResult
    {
        public ValidationFailedResult(ModelStateDictionary state)
            : base(new ApiValidationError(state))
        {
            StatusCode = HttpStatusCode.BadRequest.To<int>();
        }
    }

    public class ApiValidationError
    {
        public ApiValidationError(ModelStateDictionary dict)
        {
            this.ErrorCode = "400";
            this.Errors = dict.Keys.SelectMany(key => dict[key].Errors.Select(x => new ApiError { ErrorCode = key, Message = x.ErrorMessage }));
            this.Message = "Validation Failed";
        }

        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }

        public string ErrorCode { get; set; }

        public string Message { get; set; }

        public object Errors { get; set; }
    }
}
