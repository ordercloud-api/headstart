import { ListPage } from '../models/ListPage';
import { ChiliConfig } from '../models/ChiliConfig';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class ChiliConfigs {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.List = this.List.bind(this);
        this.Update = this.Update.bind(this);
        this.Get = this.Get.bind(this);
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
    public async List( options: ListArgs<ChiliConfig> = {}, accessToken?: string ): Promise<RequiredDeep<ListPage<ChiliConfig>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/chili/config`, { params: { ...options,  filters: options.filters, accessToken, impersonating } } );
    }

   /**
    * @param chiliConfig 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Update(chiliConfig: ChiliConfig, accessToken?: string ): Promise<RequiredDeep<ChiliConfig>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/chili/config`, chiliConfig, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param configID ID of the config.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Get(configID: string,  accessToken?: string ): Promise<RequiredDeep<ChiliConfig>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/chili/config/${configID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param configID ID of the config.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Delete(configID: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.delete(`/chili/config/${configID}`, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * ChiliConfigs.As().List() // lists ChiliConfigs using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
