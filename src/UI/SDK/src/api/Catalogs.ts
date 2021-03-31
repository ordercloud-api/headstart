import { HSCatalogAssignmentRequest } from '../models/HSCatalogAssignmentRequest';
import { ListPage } from '../models/ListPage';
import { HSCatalog } from '../models/HSCatalog';
import { HSCatalogAssignment } from '../models/HSCatalogAssignment';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class Catalogs {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.SetAssignments = this.SetAssignments.bind(this);
        this.List = this.List.bind(this);
        this.Post = this.Post.bind(this);
        this.Get = this.Get.bind(this);
        this.Put = this.Put.bind(this);
        this.Delete = this.Delete.bind(this);
        this.GetAssignments = this.GetAssignments.bind(this);
        this.SyncOnAddToLocation = this.SyncOnAddToLocation.bind(this);
        this.SyncOnRemoveFromLocation = this.SyncOnRemoveFromLocation.bind(this);
    }

   /**
    * @param buyerID ID of the buyer.
    * @param locationID ID of the location.
    * @param hSCatalogAssignmentRequest 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async SetAssignments(buyerID: string, locationID: string, hSCatalogAssignmentRequest: HSCatalogAssignmentRequest, accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/buyers/${buyerID}/${locationID}/catalogs/assignments`, hSCatalogAssignmentRequest, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param options.search Word or phrase to search for.
    * @param options.searchOn Comma-delimited list of fields to search on.
    * @param options.sortBy Comma-delimited list of fields to sort by.
    * @param options.page Page of results to return. Default: 1
    * @param options.pageSize Number of results to return per page. Default: 20, max: 100.
    * @param options.filters An object whose keys match the model, and the values are the values to filter by
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async List(buyerID: string,  options: ListArgs<HSCatalog> = {}, accessToken?: string ): Promise<RequiredDeep<ListPage<HSCatalog>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/buyers/${buyerID}/catalogs`, { params: { ...options,  filters: options.filters, accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param hSCatalog Required fields: Name
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Post(buyerID: string, hSCatalog: HSCatalog, accessToken?: string ): Promise<RequiredDeep<HSCatalog>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/buyers/${buyerID}/catalogs`, hSCatalog, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param catalogID ID of the catalog.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Get(buyerID: string, catalogID: string,  accessToken?: string ): Promise<RequiredDeep<HSCatalog>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/buyers/${buyerID}/catalogs/${catalogID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param catalogID ID of the catalog.
    * @param hSCatalog Required fields: Name
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Put(buyerID: string, catalogID: string, hSCatalog: HSCatalog, accessToken?: string ): Promise<RequiredDeep<HSCatalog>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/buyers/${buyerID}/catalogs/${catalogID}`, hSCatalog, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param catalogID ID of the catalog.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Delete(buyerID: string, catalogID: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.delete(`/buyers/${buyerID}/catalogs/${catalogID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param options.catalogID ID of the catalog.
    * @param options.locationID ID of the location.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async GetAssignments(buyerID: string,  options: ListArgs<HSCatalogAssignment> = {}, accessToken?: string ): Promise<RequiredDeep<ListPage<HSCatalogAssignment>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/buyers/${buyerID}/catalogs/assignments`, { params: { ...options,  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param userID ID of the user.
    * @param locationID ID of the location.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async SyncOnAddToLocation(buyerID: string, userID: string, locationID: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/buyers/${buyerID}/catalogs/user/${userID}/location/${locationID}/Add`, {}, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param userID ID of the user.
    * @param locationID ID of the location.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async SyncOnRemoveFromLocation(buyerID: string, userID: string, locationID: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/buyers/${buyerID}/catalogs/user/${userID}/location/${locationID}/Remove`, {}, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * Catalogs.As().List() // lists Catalogs using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
