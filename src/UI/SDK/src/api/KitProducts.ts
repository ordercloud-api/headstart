import { ListPage } from '../models/ListPage';
import { HSKitProduct } from '../models/HSKitProduct';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class KitProducts {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.List = this.List.bind(this);
        this.Create = this.Create.bind(this);
        this.Get = this.Get.bind(this);
        this.Save = this.Save.bind(this);
        this.Delete = this.Delete.bind(this);
        this.ListMeKits = this.ListMeKits.bind(this);
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
    public async List( options: ListArgs<HSKitProduct> = {}, accessToken?: string ): Promise<RequiredDeep<ListPage<HSKitProduct>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/kitproducts`, { params: { ...options,  filters: options.filters, accessToken, impersonating } } );
    }

   /**
    * @param hSKitProduct 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Create(hSKitProduct: HSKitProduct, accessToken?: string ): Promise<RequiredDeep<HSKitProduct>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/kitproducts`, hSKitProduct, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param id Id of the hs kit product.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Get(id: string,  accessToken?: string ): Promise<RequiredDeep<HSKitProduct>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/kitproducts/${id}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param id Id of the hs kit product.
    * @param hSKitProduct 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Save(id: string, hSKitProduct: HSKitProduct, accessToken?: string ): Promise<RequiredDeep<HSKitProduct>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/kitproducts/${id}`, hSKitProduct, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param id Id of the kit product.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Delete(id: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.delete(`/kitproducts/${id}`, { params: {  accessToken, impersonating } } );
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
    public async ListMeKits( options: ListArgs<HSKitProduct> = {}, accessToken?: string ): Promise<RequiredDeep<ListPage<HSKitProduct>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/kitproducts/me`, { params: { ...options,  filters: options.filters, accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * KitProducts.As().List() // lists KitProducts using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
