using System;
using System.Linq;
using Headstart.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using OrderCloud.Catalyst;

namespace Headstart.Common.Models
{
    public static class WorkItemMethods
    {
        public static WorkItemAction DetermineAction(WorkItem wi)
        {
            // we want to ensure a condition is met before determining an action
            // so that if there is a case not compensated for it flows to an exception
            try
            {
                // first check if there is a cache object, if not it's a CREATE event
                if (wi.Cache == null)
                {
                    return WorkItemAction.Create;
                }

                // if cache does exists, and there is no difference ignore the action
                if (wi.Cache != null && wi.Diff == null)
                {
                    return (wi.RecordType == RecordType.SpecProductAssignment || wi.RecordType == RecordType.UserGroupAssignment || wi.RecordType == RecordType.CatalogAssignment)
                        ? WorkItemAction.Ignore
                        : WorkItemAction.Get;
                }

                // special case for SpecAssignment because there is no ID for the OC object
                // but we want one in orchestration to handle caching
                // in further retrospect I don't think we need to handle updating objects when only the ID is being changed
                // maybe in the future a true business case will be necessary to do this
                if ((wi.RecordType == RecordType.SpecProductAssignment || wi.RecordType == RecordType.UserGroupAssignment || wi.RecordType == RecordType.CatalogAssignment)
                    && wi.Diff.Children().Count() == 1 && wi.Diff.SelectToken("ID").Path == "ID")
                {
                    return WorkItemAction.Ignore;
                }

                if (wi.Cache != null && wi.Diff != null)
                {
                    // cache exists, we want to force a PUT when xp has deleted properties because
                    // it's the only way to delete the properties
                    // otherwise we want to PATCH the existing object
                    // TODO: figure this reference out
                    // return wi.Cache.HasDeletedXp(wi.Current) ? Action.Update : Action.Patch;
                    return WorkItemAction.Patch;
                }
            }
            catch (Exception ex)
            {
                throw new OrchestrationException(OrchestrationErrorType.ActionEvaluationError, wi, ex.Message);
            }

            throw new OrchestrationException(OrchestrationErrorType.ActionEvaluationError, wi, "Unable to determine action");
        }
    }

    public class WorkItem
    {
        public WorkItem()
        {
        }

        public WorkItem(string path)
        {
            var split = path.Split("/");
            this.ResourceId = split[0];
            this.RecordId = split[split.Length - 1].Replace(".json", string.Empty);
            this.RecordType = split[2] switch
            {
                "templateproductflat" => RecordType.TemplateProductFlat,
                "hydratedproduct" => RecordType.HydratedProduct,
                "product" => RecordType.Product,
                "priceschedule" => RecordType.PriceSchedule,
                "productfacet" => RecordType.ProductFacet,
                "specproductassignment" => RecordType.SpecProductAssignment,
                "specoption" => RecordType.SpecOption,
                "spec" => RecordType.Spec,
                "buyer" => RecordType.Buyer,
                "user" => RecordType.User,
                "usergroup" => RecordType.UserGroup,
                "address" => RecordType.Address,
                "usergroupassignment" => RecordType.UserGroupAssignment,
                "addressassignment" => RecordType.AddressAssignment,
                "costcenter" => RecordType.CostCenter,
                "catalogassignment" => RecordType.CatalogAssignment,
                "catalog" => RecordType.Catalog,
                _ => throw new OrchestrationException(OrchestrationErrorType.WorkItemDefinition, path)
            };
        }

        public string ResourceId { get; set; }

        public string RecordId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RecordType RecordType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WorkItemAction Action { get; set; }

        public JObject Current { get; set; } // not used for delete

        public JObject Cache { get; set; } // not used for create

        public JObject Diff { get; set; }

        public string Token { get; set; }

        public string ClientId { get; set; }

        public DecodedToken User { get; set; }
    }
}
