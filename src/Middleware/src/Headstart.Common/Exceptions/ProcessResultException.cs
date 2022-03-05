using System;
using Flurl.Http;
using Newtonsoft.Json;
using OrderCloud.Catalyst;

namespace Headstart.Common.Exceptions
{
    public class ProcessResultException
    {
        public string Message { get; set; } = string.Empty;
        public dynamic ResponseBody { get; set; }

        public ProcessResultException(Exception ex)
        {
            Message = ex.Message;
            ResponseBody = string.Empty;
        }

        public ProcessResultException(CatalystBaseException ex)
        {
            Message = ex.Errors[0].Message;
            try
            {
                ResponseBody = JsonConvert.SerializeObject(ex.Errors);
            }
            catch (Exception)
            {
                ResponseBody = $@"Error while trying to parse response body.";
            }
        }

        public ProcessResultException(FlurlHttpException ex)
        {
            Message = ex.Message;
            try
            {
                ResponseBody = ex.GetResponseJsonAsync().Result;
            }
            catch (Exception)
            {
                ResponseBody = $@"Error while trying to parse response body.";
            }
        }
    }
}