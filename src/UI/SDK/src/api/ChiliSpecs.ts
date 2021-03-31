import { ListPage } from '../models/ListPage';
import { ChiliSpec } from '../models/ChiliSpec';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class ChiliSpecs {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.List = this.List.bind(this);
        this.Get = this.Get.bind(this);
        this.Update = this.Update.bind(this);
        this.Delete = this.Delete.bind(this);
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
    public async List( options: ListArgs<ChiliSpec> = {}, accessToken?: string ): Promise<RequiredDeep<ListPage<ChiliSpec>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/chili/specs`, { params: { ...options,  filters: options.filters, accessToken, impersonating } } );
    }

   /**
    * @param specID ID of the spec.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Get(specID: string,  accessToken?: string ): Promise<RequiredDeep<ChiliSpec>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/chili/specs/${specID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param specID ID of the spec.
    * @param chiliSpec Required fields: Name
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Update(specID: string, chiliSpec: ChiliSpec, accessToken?: string ): Promise<RequiredDeep<ChiliSpec>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/chili/specs/${specID}`, chiliSpec, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param specID ID of the spec.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Delete(specID: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.delete(`/chili/specs/${specID}`, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * ChiliSpecs.As().List() // lists ChiliSpecs using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
