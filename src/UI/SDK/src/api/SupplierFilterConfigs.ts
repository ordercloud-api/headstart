import { ListPage } from '../models/ListPage';
import { SupplierFilterConfigDocument } from '../models/SupplierFilterConfigDocument';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class SupplierFilterConfigs {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.Get = this.Get.bind(this);
    }

   /**
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Get( accessToken?: string ): Promise<RequiredDeep<ListPage<SupplierFilterConfigDocument>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/supplierfilterconfig`, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * SupplierFilterConfigs.As().List() // lists SupplierFilterConfigs using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
