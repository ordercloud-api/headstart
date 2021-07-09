import { OrderCloudIntegrationsCreditCardPayment } from '../models/OrderCloudIntegrationsCreditCardPayment';
import { HSOrder } from '../models/HSOrder';
import { LineItemStatusChanges } from '../models/LineItemStatusChanges';
import { OrderDetails } from '../models/OrderDetails';
import { HSLineItem } from '../models/HSLineItem';
import { ListPage } from '../models/ListPage';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';
import { Order } from 'ordercloud-javascript-sdk';
import { CosmosListPage, RMA } from '../models';

export default class Orders {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.Submit = this.Submit.bind(this);
        this.SellerSupplierUpdateLineItemStatusesWithNotification = this.SellerSupplierUpdateLineItemStatusesWithNotification.bind(this);
        this.ApplyAutomaticPromotions = this.ApplyAutomaticPromotions.bind(this);
        this.GetOrderDetails = this.GetOrderDetails.bind(this);
        this.BuyerUpdateLineItemStatusesWithNotification = this.BuyerUpdateLineItemStatusesWithNotification.bind(this);
        this.UpsertLineItem = this.UpsertLineItem.bind(this);
        this.DeleteLineItem = this.DeleteLineItem.bind(this);
        this.AddPromotion = this.AddPromotion.bind(this);
        this.ListShipmentsWithItems = this.ListShipmentsWithItems.bind(this);
        this.AcknowledgeQuoteOrder = this.AcknowledgeQuoteOrder.bind(this);
        this.ListLocationOrders = this.ListLocationOrders.bind(this);
        this.ListRMAsForOrder = this.ListRMAsForOrder.bind(this);
    }

   /**
    * @param direction Direction of the order cloud integrations credit card payment. Possible values: Incoming, Outgoing.
    * @param orderID ID of the order.
    * @param orderCloudIntegrationsCreditCardPayment Required fields: OrderID, PaymentID, Currency, MerchantID
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Submit(direction: 'Incoming' | 'Outgoing', orderID: string, orderCloudIntegrationsCreditCardPayment: OrderCloudIntegrationsCreditCardPayment, accessToken?: string ): Promise<RequiredDeep<HSOrder>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/order/${direction}/${orderID}/submit`, orderCloudIntegrationsCreditCardPayment, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param orderID ID of the order.
    * @param orderDirection Order direction of the line item status change. Possible values: Incoming, Outgoing.
    * @param lineItemStatusChanges 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async SellerSupplierUpdateLineItemStatusesWithNotification(orderID: string, orderDirection: 'Incoming' | 'Outgoing', lineItemStatusChanges: LineItemStatusChanges, accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/order/${orderID}/${orderDirection}/lineitem/status`, lineItemStatusChanges, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param orderID ID of the order.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ApplyAutomaticPromotions(orderID: string,  accessToken?: string ): Promise<RequiredDeep<HSOrder>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/order/${orderID}/applypromotions`, {}, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param orderID ID of the order.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async GetOrderDetails(orderID: string,  accessToken?: string ): Promise<RequiredDeep<OrderDetails>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/order/${orderID}/details`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param orderID ID of the order.
    * @param lineItemStatusChanges 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async BuyerUpdateLineItemStatusesWithNotification(orderID: string, lineItemStatusChanges: LineItemStatusChanges, accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/order/${orderID}/lineitem/status`, lineItemStatusChanges, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param orderID ID of the order.
    * @param hSLineItem Required fields: ProductID, Quantity
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async UpsertLineItem(orderID: string, hSLineItem: HSLineItem, accessToken?: string ): Promise<RequiredDeep<HSLineItem>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/order/${orderID}/lineitems`, hSLineItem, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param orderID ID of the order.
    * @param lineItemID ID of the line item.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async DeleteLineItem(orderID: string, lineItemID: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.delete(`/order/${orderID}/lineitems/${lineItemID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param orderID ID of the order.
    * @param promoCode Promo code of the hs order.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async AddPromotion(orderID: string, promoCode: string,  accessToken?: string ): Promise<RequiredDeep<HSOrder>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/order/${orderID}/promotions/${promoCode}`, {}, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param orderID ID of the order.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ListShipmentsWithItems(orderID: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/order/${orderID}/shipmentswithitems`, { params: {  accessToken, impersonating } } );
    }

 /**
    * @param orderID ID of the order.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
  public async ListRMAsForOrder(orderID: string,  accessToken?: string ): Promise<RequiredDeep<CosmosListPage<RMA>>> {
    const impersonating = this.impersonating;
    this.impersonating = false;
    return await httpClient.get(`/order/rma/list/${orderID}`, { params: {  accessToken, impersonating } } );
}

   /**
    * @param orderID ID of the order.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async AcknowledgeQuoteOrder(orderID: string,  accessToken?: string ): Promise<RequiredDeep<Order>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/order/acknowledgequote/${orderID}`, {}, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param locationID ID of the location.
    * @param options.search Word or phrase to search for.
    * @param options.searchOn Comma-delimited list of fields to search on.
    * @param options.sortBy Comma-delimited list of fields to sort by.
    * @param options.page Page of results to return. Default: 1
    * @param options.pageSize Number of results to return per page. Default: 20, max: 100.
    * @param options.filters An object whose keys match the model, and the values are the values to filter by
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ListLocationOrders(locationID: string,  options: ListArgs<HSOrder> = {}, accessToken?: string ): Promise<RequiredDeep<ListPage<HSOrder>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/order/location/${locationID}`, { params: { ...options,  filters: options.filters, accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * Orders.As().List() // lists Orders using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
