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
using Headstart.Models.Headstart;
using OrderCloud.Catalyst;
using Headstart.Common.Repositories;
using Microsoft.Azure.Cosmos;
using Headstart.Models.Misc;
using Headstart.Common.Repositories.Models;

namespace Headstart.API.Commands
{
    public interface IHSReportCommand
    {
        ListPage<ReportTypeResource> FetchAllReportTypes(DecodedToken decodedToken);
        Task<List<Address>> BuyerLocation(string templateID, DecodedToken decodedToken);
        Task<List<OrderDetailData>> SalesOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args, DecodedToken decodedToken);
        Task<List<OrderDetailData>> PurchaseOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args, DecodedToken decodedToken);
        Task<List<HSLineItemOrder>> BuyerLineItemDetail(ListArgs<HSOrder> args, BuyerReportViewContext viewContext, string userID, string locationID, DecodedToken decodedToken);
        Task<List<HSLineItemOrder>> LineItemDetail(string templateID, ListArgs<ReportAdHocFilters> args, DecodedToken decodedToken);
        Task<List<RMAWithRMALineItem>> RMADetail(string templateID, ListArgs<ReportAdHocFilters> args, DecodedToken decodedToken);
        Task<List<OrderWithShipments>> ShipmentDetail(string templateID, ListArgs<ReportAdHocFilters> args, DecodedToken decodedToken);
        Task<List<ReportTemplate>> ListReportTemplatesByReportType(ReportTypeEnum reportType, DecodedToken decodedToken);
        Task<ReportTemplate> PostReportTemplate(ReportTemplate reportTemplate, DecodedToken decodedToken);
        Task<ReportTemplate> GetReportTemplate(string id, DecodedToken decodedToken);
        Task<ReportTemplate> UpdateReportTemplate(string id, ReportTemplate reportTemplate, DecodedToken decodedToken);
        Task DeleteReportTemplate(string id);
        Task<List<HSBuyer>> GetBuyerFilterValues(DecodedToken decodedToken);
        Task<List<ProductDetailData>> ProductDetail(string templateID, ListArgs<ReportAdHocFilters> args, DecodedToken decodedToken);
    }

    public class HSReportCommand : IHSReportCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly ISalesOrderDetailDataRepo _salesOrderDetail;
        private readonly IPurchaseOrderDetailDataRepo _purchaseOrderDetail;
        private readonly ILineItemDetailDataRepo _lineItemDetail;
        private readonly IRMARepo _rmaDetail;
        private readonly IOrdersAndShipmentsDataRepo _ordersAndShipments;
        private readonly IProductDetailDataRepo _productDetailRepository;
        private readonly ReportTemplateQuery _template;

        public HSReportCommand(IOrderCloudClient oc, ISalesOrderDetailDataRepo salesOrderDetail, IPurchaseOrderDetailDataRepo purchaseOrderDetail, ILineItemDetailDataRepo lineItemDetail, IRMARepo rmaDetail, IOrdersAndShipmentsDataRepo ordersAndShipments, IProductDetailDataRepo productDetailRepository, ReportTemplateQuery template)
        {
            _oc = oc;
            _salesOrderDetail = salesOrderDetail;
            _purchaseOrderDetail = purchaseOrderDetail;
            _lineItemDetail = lineItemDetail;
            _rmaDetail = rmaDetail;
            _ordersAndShipments = ordersAndShipments;
            _productDetailRepository = productDetailRepository;
            _template = template;
        }

        public ListPage<ReportTypeResource> FetchAllReportTypes(DecodedToken decodedToken)
        {
            var types = ReportTypeResource.ReportTypes.ToList();
            if (decodedToken.CommerceRole == CommerceRole.Supplier)
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

        public async Task<List<Address>> BuyerLocation(string templateID, DecodedToken decodedToken)
        {
            //Get stored template from Cosmos DB container
            var template = await _template.Get(templateID, decodedToken);
            var allBuyerLocations = new List<Address>();

            //Logic if no Buyer ID is supplied
            if (template.Filters.BuyerID.Count == 0)
            {
                var buyers = await _oc.Buyers.ListAllAsync<HSBuyer>();
                foreach (var buyer in buyers)
                {
                    template.Filters.BuyerID.Add(buyer.ID);
                }
            }

            foreach (var buyerID in template.Filters.BuyerID)
            {
                //For every buyer included in the template filters, grab all buyer locations (exceeding 100 maximum)
                var buyerLocations = await _oc.Addresses.ListAllAsync<Address>(
                    buyerID
                );
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
                    filtersToEvaluateMap.Add(property, (List<string>)property.GetValue(template.Filters));
                }
            }
            //Filter through collected records, adding only those that pass the PassesFilters check.
            var filteredBuyerLocations = new List<Address>();
            foreach (var location in allBuyerLocations)
            {

                if (PassesFilters(location, filtersToEvaluateMap))
                {
                    filteredBuyerLocations.Add(location);
                }
            }
            return filteredBuyerLocations;
        }

        public async Task<List<OrderDetailData>> SalesOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args, DecodedToken decodedToken)
        {
            IList<ListFilter> filters = await BuildFilters(templateID, args, decodedToken, "DateSubmitted", "xp.SupplierIDs", "FromCompanyID");

            CosmosListOptions listOptions = new CosmosListOptions()
            {
                PageSize = -1,
                Sort = "OrderID",
                SortDirection = SortDirection.ASC,
                Filters = filters,
            };

            IQueryable<OrderDetailData> queryable = _salesOrderDetail.GetQueryable()
                .Where(order =>
                order.PartitionKey == "PartitionValue");

            QueryRequestOptions requestOptions = new QueryRequestOptions
            {
                MaxItemCount = listOptions.PageSize,
                MaxConcurrency = -1
            };

            CosmosListPage<OrderDetailData> salesOrderDataResponse = await _salesOrderDetail.GetItemsAsync(queryable, requestOptions, listOptions);

            List<OrderDetailData> salesOrderData = salesOrderDataResponse.Items;

            listOptions.ContinuationToken = salesOrderDataResponse.Meta.ContinuationToken;

            while (listOptions.ContinuationToken != null)
            {
                CosmosListPage<OrderDetailData> responseWithToken = await _salesOrderDetail.GetItemsAsync(queryable, requestOptions, listOptions);
                salesOrderData.AddRange(responseWithToken.Items);
                listOptions.ContinuationToken = responseWithToken.Meta.ContinuationToken;
            }

            var salesOrders = new List<OrderDetailData>();

            foreach (OrderDetailData salesOrder in salesOrderData)
            {
                salesOrders.Add(salesOrder);
            }

            return salesOrders;
        }

        public async Task<List<OrderDetailData>> PurchaseOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args, DecodedToken decodedToken)
        {
            IList<ListFilter> filters = await BuildFilters(templateID, args, decodedToken, "DateSubmitted", "xp.SupplierIDs", "ShippingAddressID");

            CosmosListOptions listOptions = new CosmosListOptions()
            {
                PageSize = -1,
                Sort = "OrderID",
                SortDirection = SortDirection.ASC,
                Filters = filters,
            };

            IQueryable<OrderDetailData> queryable = _purchaseOrderDetail.GetQueryable()
                .Where(order =>
                order.PartitionKey == "PartitionValue");

            QueryRequestOptions requestOptions = new QueryRequestOptions
            {
                MaxItemCount = listOptions.PageSize,
                MaxConcurrency = -1
            };

            CosmosListPage<OrderDetailData> purchaseOrderDataResponse = await _purchaseOrderDetail.GetItemsAsync(queryable, requestOptions, listOptions);

            List<OrderDetailData> purchaseOrderData = purchaseOrderDataResponse.Items;

            listOptions.ContinuationToken = purchaseOrderDataResponse.Meta.ContinuationToken;

            while (listOptions.ContinuationToken != null)
            {
                CosmosListPage<OrderDetailData> responseWithToken = await _purchaseOrderDetail.GetItemsAsync(queryable, requestOptions, listOptions);
                purchaseOrderData.AddRange(responseWithToken.Items);
                listOptions.ContinuationToken = responseWithToken.Meta.ContinuationToken;
            }

            var purchaseOrders = new List<OrderDetailData>();

            foreach (OrderDetailData purchaseOrder in purchaseOrderData)
            {
                purchaseOrder.ShipFromAddressID = purchaseOrder.Data.xp.SelectedShipMethodsSupplierView != null ? purchaseOrder.Data.xp.SelectedShipMethodsSupplierView[0].ShipFromAddressID : null;
                purchaseOrder.ShipMethod = purchaseOrder.Data.xp.SelectedShipMethodsSupplierView != null ? purchaseOrder.Data.xp.SelectedShipMethodsSupplierView[0].Name : null;
                purchaseOrders.Add(purchaseOrder);
            }

            return purchaseOrders;
        }

        public async Task<List<HSLineItemOrder>> BuyerLineItemDetail(ListArgs<HSOrder> args, BuyerReportViewContext viewContext, string userID, string locationID, DecodedToken decodedToken)
        {
            IList<ListFilter> filters = new List<ListFilter>();

            filters.Add(ApplyBuyerLineContext(viewContext, userID, locationID, decodedToken));

            foreach (var filter in args.Filters)
            {
                filters.Add(ApplyBuyerLineFilter(filter));
            }

            CosmosListOptions listOptions = new CosmosListOptions()
            {
                PageSize = -1,
                Sort = "OrderID",
                SortDirection = SortDirection.ASC,
                Filters = filters,
            };

            IQueryable<LineItemDetailData> queryable = _lineItemDetail.GetQueryable()
                .Where(order =>
                order.PartitionKey == "PartitionValue");

            QueryRequestOptions requestOptions = new QueryRequestOptions
            {
                MaxItemCount = listOptions.PageSize,
                MaxConcurrency = -1
            };

            CosmosListPage<LineItemDetailData> lineItemDataResponse = await _lineItemDetail.GetItemsAsync(queryable, requestOptions, listOptions);

            List<LineItemDetailData> lineItemData = lineItemDataResponse.Items;

            listOptions.ContinuationToken = lineItemDataResponse.Meta.ContinuationToken;

            while (listOptions.ContinuationToken != null)
            {
                CosmosListPage<LineItemDetailData> responseWithToken = await _lineItemDetail.GetItemsAsync(queryable, requestOptions, listOptions);
                lineItemData.AddRange(responseWithToken.Items);
                listOptions.ContinuationToken = responseWithToken.Meta.ContinuationToken;
            }

            var lineItems = new List<HSLineItemOrder>();

            foreach (LineItemDetailData detailData in lineItemData)
            {
                foreach (HSLineItem lineDetail in detailData.Data.LineItems)
                {

                    lineItems.Add(new HSLineItemOrder
                    {
                        HSOrder = detailData.Data.Order,
                        HSLineItem = lineDetail
                    });
                }
            }

            return lineItems;
        }

        public async Task<List<HSLineItemOrder>> LineItemDetail(string templateID, ListArgs<ReportAdHocFilters> args, DecodedToken decodedToken)
        {
            IList<ListFilter> filters = await BuildFilters(templateID, args, decodedToken, "DateSubmitted", "xp.SupplierIDs", "FromCompanyID");

            CosmosListOptions listOptions = new CosmosListOptions()
            {
                PageSize = -1,
                Sort = "OrderID",
                SortDirection = SortDirection.ASC,
                Filters = filters,
            };

            IQueryable<LineItemDetailData> queryable = _lineItemDetail.GetQueryable()
                .Where(order =>
                order.PartitionKey == "PartitionValue");

            QueryRequestOptions requestOptions = new QueryRequestOptions
            {
                MaxItemCount = listOptions.PageSize,
                MaxConcurrency = -1
            };

            CosmosListPage<LineItemDetailData> lineItemDataResponse = await _lineItemDetail.GetItemsAsync(queryable, requestOptions, listOptions);

            List<LineItemDetailData> lineItemData = lineItemDataResponse.Items;

            listOptions.ContinuationToken = lineItemDataResponse.Meta.ContinuationToken;

            while (listOptions.ContinuationToken != null)
            {
                CosmosListPage<LineItemDetailData> responseWithToken = await _lineItemDetail.GetItemsAsync(queryable, requestOptions, listOptions);
                lineItemData.AddRange(responseWithToken.Items);
                listOptions.ContinuationToken = responseWithToken.Meta.ContinuationToken;
            }

            var lineItems = new List<HSLineItemOrder>();

            var supplierFilter = args.Filters.FirstOrDefault(filter => filter.PropertyName == "SupplierID");

            var me = await _oc.Me.GetAsync(decodedToken.AccessToken);

            foreach (LineItemDetailData detailData in lineItemData)
            {
                foreach (HSLineItem lineDetail in detailData.Data.LineItems)
                {
                    if (supplierFilter == null || supplierFilter.FilterExpression == lineDetail.SupplierID)
                    {
                        if (decodedToken.CommerceRole == CommerceRole.Supplier)
                        {
                            //filter down to only current supplierID
                            var lineWithPOOrderFields = detailData.Data.LineItemsWithPurchaseOrderFields != null ? detailData.Data.LineItemsWithPurchaseOrderFields.FirstOrDefault(line => line.SupplierID == me.Supplier.ID && line.ID == lineDetail.ID) : null;

                            if (lineWithPOOrderFields != null)
                            {
                                lineDetail.ID = lineWithPOOrderFields.ID;
                                detailData.Data.Order.Subtotal = lineWithPOOrderFields.Subtotal;
                                detailData.Data.Order.ID = lineWithPOOrderFields.OrderID;
                                detailData.Data.Order.Total = lineWithPOOrderFields.Total;
                                lineDetail.UnitPrice = lineWithPOOrderFields.UnitPrice;
                            }
                        }

                        if (decodedToken.CommerceRole == CommerceRole.Seller || me.Supplier.ID == lineDetail.SupplierID)
                        {
                            var lineWithMiscFields = detailData.Data.LineItemsWithMiscFields != null ? detailData.Data.LineItemsWithMiscFields.FirstOrDefault(line => line.ID == lineDetail.ID) : null;
                            lineItems.Add(new HSLineItemOrder
                            {
                                HSOrder = detailData.Data.Order,
                                HSLineItem = lineDetail
                            });
                        }
                    }
                }
            }

            return lineItems;
        }

        public async Task<List<RMAWithRMALineItem>> RMADetail(string templateID, ListArgs<ReportAdHocFilters> args, DecodedToken decodedToken)
        {
            IList<ListFilter> filters = await BuildFilters(templateID, args, decodedToken, "DateCreated", "SupplierID");

            CosmosListOptions listOptions = new CosmosListOptions()
            {
                PageSize = -1,
                Sort = "RMANumber",
                SortDirection = SortDirection.ASC,
                Filters = filters,
            };

            IQueryable<RMA> queryable = _rmaDetail.GetQueryable()
                .Where(order =>
                order.PartitionKey == "PartitionValue");

            QueryRequestOptions requestOptions = new QueryRequestOptions
            {
                MaxItemCount = listOptions.PageSize,
                MaxConcurrency = -1
            };

            CosmosListPage<RMA> rmaDataResponse = await _rmaDetail.GetItemsAsync(queryable, requestOptions, listOptions);

            List<RMA> rmaData = rmaDataResponse.Items;

            listOptions.ContinuationToken = rmaDataResponse.Meta.ContinuationToken;

            while (listOptions.ContinuationToken != null)
            {
                CosmosListPage<RMA> responseWithToken = await _rmaDetail.GetItemsAsync(queryable, requestOptions, listOptions);
                rmaData.AddRange(responseWithToken.Items);
                listOptions.ContinuationToken = responseWithToken.Meta.ContinuationToken;
            }

            var rmas = new List<RMAWithRMALineItem>();

            var supplierFilter = args.Filters.FirstOrDefault(filter => filter.PropertyName == "SupplierID");

            var me = await _oc.Me.GetAsync(decodedToken.AccessToken);

            foreach (RMA detailData in rmaData)
            {
                if (supplierFilter == null || supplierFilter.FilterExpression == detailData.SupplierID)
                {
                    if (decodedToken.CommerceRole == CommerceRole.Seller || me.Supplier.ID == detailData.SupplierID)
                    {
                        foreach (RMALineItem rmaLineItem in detailData.LineItems)
                        {
                            rmas.Add(new RMAWithRMALineItem
                            {
                                RMA = detailData,
                                RMALineItem = rmaLineItem
                            });
                        }
                    }
                }
            }

            return rmas;
        }


        public async Task<List<ProductDetailData>> ProductDetail(string templateID, ListArgs<ReportAdHocFilters> args, DecodedToken decodedToken)
        {
            IList<ListFilter> filters = await BuildFilters(templateID, args, decodedToken, "DateSubmitted", "SupplierID", statusPath: "Status");

            CosmosListOptions listOptions = new CosmosListOptions()
            {
                PageSize = -1,
                Sort = "ProductID",
                SortDirection = SortDirection.ASC,
                Filters = filters,
            };

            IQueryable<ProductDetailData> queryable = _productDetailRepository.GetQueryable()
               .Where(order =>
               order.PartitionKey == "PartitionValue");

            QueryRequestOptions requestOptions = new QueryRequestOptions
            {
                MaxItemCount = listOptions.PageSize,
                MaxConcurrency = -1
            };

            CosmosListPage<ProductDetailData> productDetailDataResponse = await _productDetailRepository.GetItemsAsync(queryable, requestOptions, listOptions);

            List<ProductDetailData> productDetailDataList = productDetailDataResponse.Items;

            listOptions.ContinuationToken = productDetailDataResponse.Meta.ContinuationToken;

            while (listOptions.ContinuationToken != null)
            {
                CosmosListPage<ProductDetailData> responseWithToken = await _productDetailRepository.GetItemsAsync(queryable, requestOptions, listOptions);
                productDetailDataList.AddRange(responseWithToken.Items);
                listOptions.ContinuationToken = responseWithToken.Meta.ContinuationToken;
            }

            var productDetailList = new List<ProductDetailData>();

            var supplierFilter = args.Filters.FirstOrDefault(filter => filter.PropertyName == "SupplierID");

            var me = await _oc.Me.GetAsync(decodedToken.AccessToken);

            foreach (ProductDetailData detailData in productDetailDataList)
            {
                if (supplierFilter == null || supplierFilter.FilterExpression == detailData.Data.SupplierID)
                {
                    if (decodedToken.CommerceRole == CommerceRole.Seller || me.Supplier.ID == detailData.Data.SupplierID)
                    {
                        productDetailList.Add(detailData);
                    }
                }
            }

            return productDetailList;
        }

        public async Task<List<OrderWithShipments>> ShipmentDetail(string templateID, ListArgs<ReportAdHocFilters> args, DecodedToken decodedToken)
        {
            IList<ListFilter> filters = await BuildFilters(templateID, args, decodedToken, "DateSubmitted", "SupplierID");

            CosmosListOptions listOptions = new CosmosListOptions()
            {
                PageSize = -1,
                Sort = "OrderID",
                SortDirection = SortDirection.ASC,
                Filters = filters,
            };

            IQueryable<OrderWithShipments> queryable = _ordersAndShipments.GetQueryable()
                .Where(order =>
                order.PartitionKey == "PartitionValue");

            QueryRequestOptions requestOptions = new QueryRequestOptions
            {
                MaxItemCount = listOptions.PageSize,
                MaxConcurrency = -1
            };

            CosmosListPage<OrderWithShipments> ordersWithShipmentsDataResponse = await _ordersAndShipments.GetItemsAsync(queryable, requestOptions, listOptions);

            List<OrderWithShipments> orderWithShipmentsData = ordersWithShipmentsDataResponse.Items;

            listOptions.ContinuationToken = ordersWithShipmentsDataResponse.Meta.ContinuationToken;

            while (listOptions.ContinuationToken != null)
            {
                CosmosListPage<OrderWithShipments> responseWithToken = await _ordersAndShipments.GetItemsAsync(queryable, requestOptions, listOptions);
                orderWithShipmentsData.AddRange(responseWithToken.Items);
                listOptions.ContinuationToken = responseWithToken.Meta.ContinuationToken;
            }

            var ordersWithShipments = new List<OrderWithShipments>();

            var supplierFilter = args.Filters.FirstOrDefault(filter => filter.PropertyName == "SupplierID");

            var me = await _oc.Me.GetAsync(decodedToken.AccessToken);

            foreach (OrderWithShipments detailData in orderWithShipmentsData)
            {
                if (supplierFilter == null || supplierFilter.FilterExpression == detailData.SupplierID)
                {
                    if (decodedToken.CommerceRole == CommerceRole.Seller || me.Supplier.ID == detailData.SupplierID)
                    {
                        ordersWithShipments.Add(detailData);
                    }
                }
            }

            return ordersWithShipments;
        }

        private async Task<IList<ListFilter>> BuildFilters(string templateID, ListArgs<ReportAdHocFilters> args, DecodedToken decodedToken, string datePath, string supplierIDPath, string brandIDPath = null, string statusPath = null)
        {
            IList<ListFilter> filters = new List<ListFilter>();

            ReportTemplate template = await _template.Get(templateID, decodedToken);

            string timeLow = GetAdHocFilterValue(args, "TimeLow");
            string timeHigh = GetAdHocFilterValue(args, "TimeHigh");

            string dataPathPrefix = GetDataPathPrefix(template.ReportType);

            var templateFilterProperties = template.Filters.GetType().GetProperties();

            foreach (var filterProperty in templateFilterProperties)
            {
                var propertyName = filterProperty.Name;
                var values = filterProperty.GetValue(template.Filters);
                if (values != null)
                {
                    var valuesList = (List<string>)values;
                    var filterExpression = "=";
                    var propertyLocation = ReportFilters.NestedLocations.ContainsKey(propertyName)
                        ? $"{dataPathPrefix}{ReportFilters.NestedLocations[propertyName]}"
                        : $"{dataPathPrefix}{propertyName}";
                    foreach (var value in valuesList)
                    {
                        filterExpression = filterExpression == "=" ? filterExpression + value : filterExpression + $"|{value}";
                    }
                    if (filterExpression != "=")
                    {
                        filters.Add(new ListFilter(propertyLocation, filterExpression));
                    }
                }
            }

            foreach (var filter in args.Filters)
            {
                var propertyName = filter.PropertyName;
                var filterExpression = filter.FilterExpression;

                if (propertyName == "TimeLow" || propertyName == "TimeHigh")
                {
                    continue;
                }
                if (propertyName == "DateLow")
                {
                    propertyName = $"{dataPathPrefix}{datePath}";
                    var timeLowExpression = timeLow != null ? $"T{timeLow}" : null;
                    filterExpression = $">={filterExpression}{timeLowExpression}";
                }
                else if (propertyName == "DateHigh")
                {
                    propertyName = $"{dataPathPrefix}{datePath}";
                    var timeHighExpression = timeHigh != null ? $"T{timeHigh}" : "T23:59:59.999+00:00";
                    filterExpression = $"<={filterExpression}{timeHighExpression}";
                }
                else if (propertyName == "SupplierID")
                {
                    propertyName = $"{dataPathPrefix}{supplierIDPath}";
                    filterExpression = $"={filterExpression}";
                }
                else if (propertyName == "BrandID" && template.ReportType == ReportTypeEnum.PurchaseOrderDetail)
                {
                    propertyName = $"{dataPathPrefix}{brandIDPath}";
                    filterExpression = $"={filterExpression}-*";
                }
                else if (propertyName == "BrandID")
                {
                    propertyName = $"{dataPathPrefix}{brandIDPath}";
                    filterExpression = $"={filterExpression}";
                }
                else if (propertyName == "Status")
                {
                    propertyName = $"{dataPathPrefix}{statusPath}";
                    filterExpression = $"={filterExpression}";
                }
                else
                {
                    filterExpression = $"={filterExpression}";
                }
                filters.Add(new ListFilter(propertyName, filterExpression));
            }

            if (decodedToken.CommerceRole == CommerceRole.Supplier)
            {
                var me = await _oc.Me.GetAsync(decodedToken.AccessToken);
                filters.Add(new ListFilter($"{dataPathPrefix}{supplierIDPath}", me.Supplier.ID));
            }

            return filters;
        }

        private ListFilter ApplyBuyerLineContext(BuyerReportViewContext viewContext, string userID, string locationID, DecodedToken decodedToken)
        {
            if (viewContext == BuyerReportViewContext.MyOrders)
            {
                return new ListFilter("Data.Order.FromUserID", $"={userID}");
            }
            else if (viewContext == BuyerReportViewContext.Location)
            {
                if (decodedToken.Roles.Contains(CustomRole.HSLocationViewAllOrders.ToString()))
                {
                    return new ListFilter("Data.Order.BillingAddress.ID", $"={locationID}");
                }
                else
                {
                    throw new Exception("You are not permitted to view all orders for this location.");
                }
            }
            else
            {
                throw new Exception("Please select a valid view context.");
            }
        }

        private ListFilter ApplyBuyerLineFilter(ListFilter filter)
        {
            var dataPathPrefix = "Data.Order";
            var propertyName = filter.PropertyName;
            var filterExpression = filter.FilterExpression;

            if (propertyName == "from")
            {
                DateTime dt = new DateTime();
                dt = Convert.ToDateTime(filterExpression);
                var expression = dt.ToString("yyyy-MM-dd");
                propertyName = $"{dataPathPrefix}.DateSubmitted";
                filterExpression = $">={expression}";
            }
            else if (propertyName == "to")
            {
                DateTime dt = new DateTime();
                dt = Convert.ToDateTime(filterExpression);
                var expression = dt.ToString("yyyy-MM-dd");
                propertyName = $"{dataPathPrefix}.DateSubmitted";
                var timeHighExpression = "T23:59:59.999+00:00";
                filterExpression = $"<={expression}{timeHighExpression}";
            }
            else
            {
                propertyName = $"{dataPathPrefix}.{filter.PropertyName}";
                filterExpression = $"={filterExpression}";
            }
            return new ListFilter(propertyName, filterExpression);
        }

        private string GetDataPathPrefix(ReportTypeEnum reportType)
        {
            switch (reportType)
            {
                case ReportTypeEnum.SalesOrderDetail:
                    return "Data.";
                case ReportTypeEnum.PurchaseOrderDetail:
                    return "Data.";
                case ReportTypeEnum.LineItemDetail:
                    return "Data.Order.";
                case ReportTypeEnum.ProductDetail:
                    return "Data.";
                default:
                    return null;
            }
        }

        public async Task<List<ReportTemplate>> ListReportTemplatesByReportType(ReportTypeEnum reportType, DecodedToken decodedToken)
        {
            var template = await _template.List(reportType, decodedToken);
            return template;
        }

        public async Task<ReportTemplate> PostReportTemplate(ReportTemplate reportTemplate, DecodedToken decodedToken)
        {
            var template = await _template.Post(reportTemplate, decodedToken);
            return template;
        }
        public async Task<ReportTemplate> GetReportTemplate(string id, DecodedToken decodedToken)
        {
            return await _template.Get(id, decodedToken);
        }

        public async Task<ReportTemplate> UpdateReportTemplate(string id, ReportTemplate reportTemplate, DecodedToken decodedToken)
        {
            var template = await _template.Put(id, reportTemplate, decodedToken);
            return template;
        }

        public async Task DeleteReportTemplate(string id)
        {
            await _template.Delete(id);
        }

        public async Task<List<HSBuyer>> GetBuyerFilterValues(DecodedToken decodedToken)
        {
            if (decodedToken.CommerceRole == CommerceRole.Seller)
            {
                return await _oc.Buyers.ListAllAsync<HSBuyer>();
            }

            var adminOcToken = _oc.TokenResponse?.AccessToken;
            if (adminOcToken == null || DateTime.UtcNow > _oc.TokenResponse.ExpiresUtc)
            {
                await _oc.AuthenticateAsync();
                adminOcToken = _oc.TokenResponse.AccessToken;
            }

            return await _oc.Buyers.ListAllAsync<HSBuyer>(adminOcToken);
        }

        private string GetAdHocFilterValue(ListArgs<ReportAdHocFilters> args, string propertyName)
        {
            return args.Filters.FirstOrDefault(Filter => Filter.PropertyName == propertyName)?.FilterExpression;
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
    }
}
