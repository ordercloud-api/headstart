using Headstart.Models;
using OrderCloud.SDK;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using System.Collections.Generic;
using Headstart.Common.Models;
using Headstart.Common.Queries;
using System;
using System.Linq;
using System.Reflection;
using ordercloud.integrations.library.helpers;
using Headstart.Models.Headstart;

namespace Headstart.API.Commands
{
    public interface IHSReportCommand
    {
        ListPage<ReportTypeResource> FetchAllReportTypes(VerifiedUserContext verifiedUser);
        Task<List<HSAddressBuyer>> BuyerLocation(string templateID, VerifiedUserContext verifiedUser);
        Task<List<HSOrder>> SalesOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args, VerifiedUserContext verifiedUser);
        Task<List<HSOrder>> PurchaseOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args, VerifiedUserContext verifiedUser);
        Task<List<HSLineItemOrder>> LineItemDetail(string templateID, ListArgs<ReportAdHocFilters> args, VerifiedUserContext verifiedUser);
        Task<List<ReportTemplate>> ListReportTemplatesByReportType(ReportTypeEnum reportType, VerifiedUserContext verifiedUser);
        Task<ReportTemplate> PostReportTemplate(ReportTemplate reportTemplate, VerifiedUserContext verifiedUser);
        Task<ReportTemplate> GetReportTemplate(string id, VerifiedUserContext verifiedUser);
        Task<ReportTemplate> UpdateReportTemplate(string id, ReportTemplate reportTemplate, VerifiedUserContext verifiedUser);
        Task DeleteReportTemplate(string id);
    }
    
    public class HSReportCommand : IHSReportCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly ReportTemplateQuery _template;

        public HSReportCommand(IOrderCloudClient oc, ReportTemplateQuery template)
        {
            _oc = oc;
            _template = template;
        }

        public ListPage<ReportTypeResource> FetchAllReportTypes(VerifiedUserContext verifiedUser)
        {
            var types = ReportTypeResource.ReportTypes.ToList();
            if (verifiedUser.UsrType == "supplier")
            {
                types = types.Where(type => type.AvailableToSuppliers).ToList();
            }
            var listPage = new ListPage<ReportTypeResource>
            {
                Items = types,
                Meta = new ListPageMeta
                {
                    Page = 1,
                    PageSize = 100,
                    TotalCount = types.Count,
                    TotalPages = 1
                }
            };
            return listPage;
        }

        public async Task<List<HSAddressBuyer>> BuyerLocation(string templateID, VerifiedUserContext verifiedUser)
        {
            //Get stored template from Cosmos DB container
            var template = await _template.Get(templateID, verifiedUser);
            var allBuyerLocations = new List<HSAddressBuyer>();

            //Logic if no Buyer ID is supplied
            if (template.Filters.BuyerID.Count == 0)
            {
                var buyers = await ListAllAsync.List((page) => _oc.Buyers.ListAsync<HSBuyer>(
                    filters: null,
                    page: page,
                    pageSize: 100
                 ));
                foreach (var buyer in buyers)
                {
                    template.Filters.BuyerID.Add(buyer.ID);
                }
            }

            foreach (var buyerID in template.Filters.BuyerID)
            {
                //For every buyer included in the template filters, grab all buyer locations (exceeding 100 maximum)
                var buyerLocations = await ListAllAsync.List((page) => _oc.Addresses.ListAsync<HSAddressBuyer>(
                    buyerID,
                    filters: null,
                    page: page,
                    pageSize: 100
                ));
                allBuyerLocations.AddRange(buyerLocations);
            }
            //Use reflection to determine available filters from model
            var filterClassProperties = template.Filters.GetType().GetProperties();
            //Create dictionary of key/value pairings of filters, where provided in the template
            var filtersToEvaluateMap = new Dictionary<PropertyInfo, List<string>>();
            foreach (var property in filterClassProperties)
            {
                //See if there are filters provided on the property.  If no values supplied, do not evaluate the filter.
                List<string> propertyFilters = (List<string>)property.GetValue(template.Filters);
                if (propertyFilters != null && propertyFilters.Count > 0 && property.Name != "BuyerID")
                {
                    filtersToEvaluateMap.Add(property, (List<string>) property.GetValue(template.Filters));
                }
            }
            //Filter through collected records, adding only those that pass the PassesFilters check.
            var filteredBuyerLocations = new List<HSAddressBuyer>();
            foreach (var location in allBuyerLocations)
            {

                if (PassesFilters(location, filtersToEvaluateMap))
                {
                    filteredBuyerLocations.Add(location);
                }
            }
            return filteredBuyerLocations;
        }

        public async Task<List<HSOrder>> SalesOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args, VerifiedUserContext verifiedUser)
        {
            var template = await _template.Get(templateID, verifiedUser);
            string dateLow = GetAdHocFilterValue(args, "DateLow");
            string timeLow = GetAdHocFilterValue(args, "TimeLow");
            string dateHigh = GetAdHocFilterValue(args, "DateHigh");
            string timeHigh = GetAdHocFilterValue(args, "TimeHigh");
            var orders = await ListAllAsync.List((page) => _oc.Orders.ListAsync<HSOrder>(
                OrderDirection.Incoming,
                filters: $"from={dateLow}&to={dateHigh}",
                page: page,
                pageSize: 100
                 ));
            var filterClassProperties = template.Filters.GetType().GetProperties();
            var filtersToEvaluateMap = new Dictionary<PropertyInfo, List<string>>();
            foreach (var property in filterClassProperties)
            {
                List<string> propertyFilters = (List<string>)property.GetValue(template.Filters);
                if (propertyFilters != null && propertyFilters.Count > 0)
                {
                    filtersToEvaluateMap.Add(property, (List<string>)property.GetValue(template.Filters));
                }
            }
            var filteredOrders = new List<HSOrder>();
            foreach (var order in orders)
            {

                if (PassesFilters(order, filtersToEvaluateMap) && PassesOrderTimeFilter(order, dateLow, timeLow, dateHigh, timeHigh))
                {
                    filteredOrders.Add(order);
                }
            }

            return filteredOrders;
        }

        public async Task<List<HSOrder>> PurchaseOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args, VerifiedUserContext verifiedUser)
        {
            var template = await _template.Get(templateID, verifiedUser);
            string dateLow = GetAdHocFilterValue(args, "DateLow");
            string timeLow = GetAdHocFilterValue(args, "TimeLow");
            string dateHigh = GetAdHocFilterValue(args, "DateHigh");
            string timeHigh = GetAdHocFilterValue(args, "TimeHigh");
            var orderDirection = verifiedUser.UsrType == "admin" ? OrderDirection.Outgoing : OrderDirection.Incoming;
            var orders = await ListAllAsync.List((page) => _oc.Orders.ListAsync<HSOrder>(
                orderDirection,
                filters: $"from={dateLow}&to={dateHigh}",
                page: page,
                pageSize: 100,
                accessToken: verifiedUser.AccessToken
                 ));

            // From User headers must pull from the Sales Order record
            var salesOrders = await GetSalesOrdersIfNeeded(template, dateLow, dateHigh, verifiedUser);
            var filterClassProperties = template.Filters.GetType().GetProperties();
            var filtersToEvaluateMap = new Dictionary<PropertyInfo, List<string>>();
            foreach (var property in filterClassProperties)
            {
                List<string> propertyFilters = (List<string>)property.GetValue(template.Filters);
                if (propertyFilters != null && propertyFilters.Count > 0)
                {
                    filtersToEvaluateMap.Add(property, (List<string>)property.GetValue(template.Filters));
                }
            }
            var filteredOrders = new List<HSOrder>();
            foreach (var order in orders)
            {

                if (PassesFilters(order, filtersToEvaluateMap) && PassesOrderTimeFilter(order, dateLow, timeLow, dateHigh, timeHigh))
                {
                    filteredOrders.Add(order);
                }
            }
            // If headers include shipping address info, run check that they have Shipping Address data
            // Orders before 01/2021 may not have this on Order XP.
            if (template.Headers.Any(header => header.Contains("xp.ShippingAddress") || header.Contains("FromUser")))
            {
                foreach (var order in filteredOrders)
                {
                    // If users are reporting on From User information, this must come from the sales order instead of the purchase order.
                    if (template.Headers.Any(header => header.Contains("FromUser")))
                    {
                        var matchingSalesOrder = salesOrders.Find(salesOrder => order.ID.Split('-')[0] == salesOrder.ID);
                        order.FromUser = matchingSalesOrder?.FromUser;
                    }
                    // If orders do not have shipping address data, pull that from the first line item.
                    // Orders after 01/2021 should have this information on Order XP already.
                    if (template.Headers.Any(header => header.Contains("xp.ShippingAddress") && order.xp.ShippingAddress == null))
                    {
                        var lineItems = await _oc.LineItems.ListAsync(
                        orderDirection,
                        order.ID,
                        pageSize: 1,
                        accessToken: verifiedUser.AccessToken
                        );
                        order.xp.ShippingAddress = new HSAddressBuyer()
                        {
                            FirstName = lineItems.Items[0].ShippingAddress?.FirstName,
                            LastName = lineItems.Items[0].ShippingAddress?.LastName,
                            Street1 = lineItems.Items[0].ShippingAddress?.Street1,
                            Street2 = lineItems.Items[0].ShippingAddress?.Street2,
                            City = lineItems.Items[0].ShippingAddress?.City,
                            State = lineItems.Items[0].ShippingAddress?.State,
                            Zip = lineItems.Items[0].ShippingAddress?.Zip,
                            Country = lineItems.Items[0].ShippingAddress?.Country,
                        };
                    }
                }
            }
            return filteredOrders;
        }

        public async Task<List<HSLineItemOrder>> LineItemDetail(string templateID, ListArgs<ReportAdHocFilters> args, VerifiedUserContext verifiedUser)
        {
            var template = await _template.Get(templateID, verifiedUser);
            string dateLow = GetAdHocFilterValue(args, "DateLow");
            string timeLow = GetAdHocFilterValue(args, "TimeLow");
            string dateHigh = GetAdHocFilterValue(args, "DateHigh");
            string timeHigh = GetAdHocFilterValue(args, "TimeHigh");
            var orders = await ListAllAsync.List((page) => _oc.Orders.ListAsync<HSOrder>(
                OrderDirection.Incoming,
                filters: $"from={dateLow}&to={dateHigh}",
                page: page,
                pageSize: 100,
                accessToken: verifiedUser.AccessToken
                 ));

            // From User headers must pull from the Sales Order record
            var salesOrders = await GetSalesOrdersIfNeeded(template, dateLow, dateHigh, verifiedUser);
            var filterClassProperties = template.Filters.GetType().GetProperties();
            var filtersToEvaluateMap = new Dictionary<PropertyInfo, List<string>>();
            foreach (var property in filterClassProperties)
            {
                List<string> propertyFilters = (List<string>)property.GetValue(template.Filters);
                if (propertyFilters != null && propertyFilters.Count > 0)
                {
                    filtersToEvaluateMap.Add(property, (List<string>)property.GetValue(template.Filters));
                }
            }
            var filteredOrders = new List<HSOrder>();
            foreach (var order in orders)
            {

                if (PassesFilters(order, filtersToEvaluateMap) && PassesOrderTimeFilter(order, dateLow, timeLow, dateHigh, timeHigh))
                {
                    filteredOrders.Add(order);
                }
            }
            var lineItemOrders = new List<HSLineItemOrder>();
            foreach (var order in filteredOrders)
            {
                // If suppliers are reporting on From User information, this must come from the seller order instead.
                if (template.Headers.Any(header => header.Contains("FromUser") && verifiedUser.UsrType == "supplier"))
                {
                    var matchingSalesOrder = salesOrders.Find(salesOrder => order.ID.Split('-')[0] == salesOrder.ID);
                    order.FromUser = matchingSalesOrder?.FromUser;
                }
                var lineItems = new List<HSLineItem>();
                lineItems.AddRange(await ListAllAsync.List((page) => _oc.LineItems.ListAsync<HSLineItem>(
                    OrderDirection.Incoming,
                    order.ID,
                    page: page,
                    pageSize: 100,
                    accessToken: verifiedUser.AccessToken
                    )));
                foreach (var lineItem in lineItems)
                {
                    lineItemOrders.Add(new HSLineItemOrder()
                    {
                        HSOrder = order,
                        HSLineItem = lineItem
                    });
                }
            }

            return lineItemOrders;
        }

        public async Task<List<ReportTemplate>> ListReportTemplatesByReportType(ReportTypeEnum reportType, VerifiedUserContext verifiedUser)
        {
            var template = await _template.List(reportType, verifiedUser);
            return template;
        }

        public async Task<ReportTemplate> PostReportTemplate(ReportTemplate reportTemplate, VerifiedUserContext verifiedUser)
        {
            var template = await _template.Post(reportTemplate, verifiedUser);
            return template;
        }
        public async Task<ReportTemplate> GetReportTemplate(string id, VerifiedUserContext verifiedUser)
        {
            return await _template.Get(id, verifiedUser);
        }

        public async Task<ReportTemplate> UpdateReportTemplate(string id, ReportTemplate reportTemplate, VerifiedUserContext verifiedUser)
        {
            var template = await _template.Put(id, reportTemplate, verifiedUser);
            return template;
        }

        public async Task DeleteReportTemplate(string id)
        {
            await _template.Delete(id);
        }
        private string GetAdHocFilterValue(ListArgs<ReportAdHocFilters> args, string propertyName)
        {
            return args.Filters.FirstOrDefault(Filter => Filter.Name == propertyName)?.QueryParams
                .FirstOrDefault(q => q?.Item1 == propertyName)?.Item2;
        }

        private bool PassesFilters(object data, Dictionary<PropertyInfo, List<string>> filtersToEvaluate)
        {
            foreach (var filterProps in filtersToEvaluate)
            {
                var filterKey = filterProps.Key.Name;
                var dataType = data.GetType();
                var dataProperties = new List<PropertyInfo>(dataType.GetProperties());
                var dataPropertyStrings = dataProperties.Select(property => property.Name).ToArray();
                if (!dataPropertyStrings.Contains(filterKey))
                {
                    filterKey = ReportFilters.NestedLocations[filterKey];
                }
                var filterValues = filterProps.Value;
                var dataValue = GetDataValue(filterKey, data);
                if (dataValue == null || !filterValues.Contains(dataValue.ToString()))
                {
                    return false;
                }
            }
            return true;
        }

        private bool PassesOrderTimeFilter(HSOrder order, string dateLow, string timeLow, string dateHigh, string timeHigh)
        {
            DateTime dt = DateTime.Parse(order.DateSubmitted.ToString());
            string date = dt.ToString("yyyy-MM-dd");
            string time = dt.ToString("HH:mm:ss");
            if (date == dateLow || date == dateHigh)
            {
                TimeSpan spanSubmittedTime = TimeSpan.Parse(time);
                TimeSpan spanTimeLow = (timeLow != null) ? TimeSpan.Parse(timeLow) : TimeSpan.MinValue;
                TimeSpan spanTimeHigh = (timeHigh != null) ? TimeSpan.Parse(timeHigh) : TimeSpan.MaxValue;
                if (date == dateLow && date == dateHigh && spanSubmittedTime < spanTimeLow || spanSubmittedTime > spanTimeHigh)
                {
                    return false;
                }
                if (
                    (date == dateLow && spanSubmittedTime > spanTimeLow)
                    || (date == dateHigh && spanSubmittedTime < spanTimeHigh)
                    )
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private object GetDataValue(string filterKey, object data)
        {
            if (data == null)
            {
                return null;
            }
            var filterKeys = filterKey.Split('.');
            for (var i = 0; i < filterKeys.Length; i++)
            {
                var properties = data.GetType().GetProperties();
                for (var j = 0; j < properties.Length; j++)
                {
                    var property = properties[j].Name;
                    if (property == filterKeys[i])
                    {
                       data = properties[j].GetValue(data);
                       if (i < filterKeys.Length - 1)
                        {
                            string[] remainingLevels = new string[filterKeys.Length - i - 1];
                            Array.Copy(filterKeys, i + 1, remainingLevels, 0, filterKeys.Length - i - 1);
                            string remainingKeys = string.Join(".", remainingLevels);
                            return GetDataValue(remainingKeys, data);
                        }
                       return data;
                    }
                }
            }
            return null;
        }

        private async Task<List<HSOrder>> GetSalesOrdersIfNeeded(ReportTemplate template, string dateLow, string dateHigh, VerifiedUserContext verifiedUser)
        {
            if (template.Headers.Any(header => header.Contains("FromUser")))
            {
                return await ListAllAsync.List((page) => _oc.Orders.ListAsync<HSOrder>(
                OrderDirection.Incoming,
                filters: verifiedUser.UsrType == "supplier" ? $"from={dateLow}&to={dateHigh}&xp.SupplierIDs={verifiedUser.SupplierID}" : $"from={dateLow}&to={dateHigh}",
                page: page,
                pageSize: 100
               ));
            }
            return null;
        }
    }
}
