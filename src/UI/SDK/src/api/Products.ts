import { ListPage } from '../models/ListPage';
import { SuperHSProduct } from '../models/SuperHSProduct';
import { HSPriceSchedule } from '../models/HSPriceSchedule';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';
import { Product } from 'ordercloud-javascript-sdk';

export default class Products {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.List = this.List.bind(this);
        this.Post = this.Post.bind(this);
        this.Get = this.Get.bind(this);
        this.Put = this.Put.bind(this);
        this.Delete = this.Delete.bind(this);
        this.GetPricingOverride = this.GetPricingOverride.bind(this);
        this.CreatePricingOverride = this.CreatePricingOverride.bind(this);
        this.UpdatePricingOverride = this.UpdatePricingOverride.bind(this);
        this.DeletePricingOverride = this.DeletePricingOverride.bind(this);
        this.FilterOptionOverride = this.FilterOptionOverride.bind(this);
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
    public async List( options: ListArgs<SuperHSProduct> = {}, accessToken?: string ): Promise<RequiredDeep<ListPage<SuperHSProduct>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/products`, { params: { ...options,  filters: options.filters, accessToken, impersonating } } );
    }

   /**
    * @param superHSProduct 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Post(superHSProduct: SuperHSProduct, accessToken?: string ): Promise<RequiredDeep<SuperHSProduct>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/products`, superHSProduct, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param id Id of the super hs product.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Get(id: string,  accessToken?: string ): Promise<RequiredDeep<SuperHSProduct>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/products/${id}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param id Id of the super hs product.
    * @param superHSProduct 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Put(id: string, superHSProduct: SuperHSProduct, accessToken?: string ): Promise<RequiredDeep<SuperHSProduct>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/products/${id}`, superHSProduct, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param id Id of the product.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Delete(id: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.delete(`/products/${id}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param id Id of the hs price schedule.
    * @param buyerID ID of the buyer.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async GetPricingOverride(id: string, buyerID: string,  accessToken?: string ): Promise<RequiredDeep<HSPriceSchedule>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/products/${id}/pricingoverride/buyer/${buyerID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param id Id of the hs price schedule.
    * @param buyerID ID of the buyer.
    * @param hSPriceSchedule Required fields: Name
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async CreatePricingOverride(id: string, buyerID: string, hSPriceSchedule: HSPriceSchedule, accessToken?: string ): Promise<RequiredDeep<HSPriceSchedule>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/products/${id}/pricingoverride/buyer/${buyerID}`, hSPriceSchedule, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param id Id of the hs price schedule.
    * @param buyerID ID of the buyer.
    * @param hSPriceSchedule Required fields: Name
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async UpdatePricingOverride(id: string, buyerID: string, hSPriceSchedule: HSPriceSchedule, accessToken?: string ): Promise<RequiredDeep<HSPriceSchedule>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/products/${id}/pricingoverride/buyer/${buyerID}`, hSPriceSchedule, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param id Id of the product.
    * @param buyerID ID of the buyer.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async DeletePricingOverride(id: string, buyerID: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.delete(`/products/${id}/pricingoverride/buyer/${buyerID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param id Id of the product.
    * @param product 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async FilterOptionOverride(id: string, product: Product, accessToken?: string ): Promise<RequiredDeep<Product>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.patch(`/products/filteroptionoverride/${id}`, product, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * Products.As().List() // lists Products using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
