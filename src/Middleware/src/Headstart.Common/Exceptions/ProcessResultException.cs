using System;
using System.Collections.Generic;
using System.Text;
using Flurl.Http;
using ordercloud.integrations.library;
using Newtonsoft.Json;

namespace Headstart.Common.Exceptions
{
    public class ProcessResultException
    {
        public ProcessResultException(Exception ex)
        {
            this.Message = ex.Message;
            this.ResponseBody = "";
        }

        public ProcessResultException(OrderCloudIntegrationException ex)
        {
            this.Message = ex.ApiError.Message;
            try
            {
                this.ResponseBody = JsonConvert.SerializeObject(ex.ApiError);
            } catch(Exception)
            {
                this.ResponseBody = "Error while trying to parse response body";
            }
        }

        public ProcessResultException(FlurlHttpException ex)
        {
            this.Message = ex.Message;
            try
            {
                this.ResponseBody = ex.GetResponseJsonAsync().Result;
            } catch(Exception)
            {
                this.ResponseBody = "Error while trying to parse response body";
            }
            
        }

        public string Message { get; set; }
        public dynamic ResponseBody { get; set; }
    }
}
