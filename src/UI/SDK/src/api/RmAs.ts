import { RMA } from '../models/RMA';
import { CosmosListPage } from '../models/CosmosListPage';
import { CosmosListOptions } from '../models/CosmosListOptions';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';
import { Filters } from '../models';

export default class RmAs {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.Get = this.Get.bind(this);
        this.PostRMA = this.PostRMA.bind(this);
        this.ListRMAsByOrderID = this.ListRMAsByOrderID.bind(this);
        this.ListRMAs = this.ListRMAs.bind(this);
        this.ListBuyerRMAs = this.ListBuyerRMAs.bind(this);
        this.ProcessRMA = this.ProcessRMA.bind(this);
        this.ProcessRefund = this.ProcessRefund.bind(this);
    }

   /**
    * @param options.search Word or phrase to search for.
    * @param options.searchOn Comma-delimited list of fields to search on.
    * @param options.sortBy Comma-delimited list of fields to sort by.
    * @param options.page Page of results to return. Default: 1
    * @param options.pageSize Number of results to return per page. Default: 20, max: 100.
    * @param options.filters An object whose keys match the model, and the values are the values to filter by
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Get( search: string, searchOn: string[], sortBy: string[], page: number, pageSize: number, filters: Filters<Required<RMA>>, accessToken?: string ): Promise<RequiredDeep<RMA>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/rma`, { params: { search, searchOn, sortBy, page, pageSize, filters, accessToken, impersonating } } );
    }

   /**
    * @param rMA 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostRMA(rMA: RMA, accessToken?: string ): Promise<RequiredDeep<RMA>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/rma`, rMA, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param orderID ID of the order.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ListRMAsByOrderID(orderID: string,  accessToken?: string ): Promise<RequiredDeep<CosmosListPage<RMA>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/rma/${orderID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param cosmosListOptions 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ListRMAs(cosmosListOptions: CosmosListOptions, accessToken?: string ): Promise<RequiredDeep<CosmosListPage<RMA>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/rma/list`, cosmosListOptions, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param cosmosListOptions 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ListBuyerRMAs(cosmosListOptions: CosmosListOptions, accessToken?: string ): Promise<RequiredDeep<CosmosListPage<RMA>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/rma/list/buyer`, cosmosListOptions, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param rMA 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ProcessRMA(rMA: RMA, accessToken?: string ): Promise<RequiredDeep<RMA>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/rma/process-rma`, rMA, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param rmaNumber Rma number of the rma.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ProcessRefund(rmaNumber: string,  accessToken?: string ): Promise<RequiredDeep<RMA>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/rma/refund/${rmaNumber}`, {}, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * RmAs.As().List() // lists RmAs using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
