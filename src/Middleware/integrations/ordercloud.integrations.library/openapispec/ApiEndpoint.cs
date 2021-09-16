using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ordercloud.integrations.library
{
    public class ApiEndpoint
    {
        public MethodInfo MethodInfo { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Used by docs (but not SDKs) for "Me", so ListCostCenters is under "Cost Centers" sub-resource, for example
        /// </summary>
        public string SubResource { get; set; }
        public string Description { get; set; }
        public System.Net.Http.HttpMethod HttpVerb { get; set; }
        public string Route { get; set; }
        public IList<ApiParameter> PathArgs { get; set; }
        public IList<ApiParameter> QueryArgs { get; set; }
        public ApiModel RequestModel { get; set; }
        public ApiModel ResponseModel { get; set; }
        public int HttpStatus { get; set; }
        public IList<string> RequiredRoles { get; set; }
        public IList<string> Comments { get; set; }
        public bool IsList { get; set; }
        public bool HasListArgs { get; set; }
    }
}
