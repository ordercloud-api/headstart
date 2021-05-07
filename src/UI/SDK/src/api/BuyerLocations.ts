import { HSBuyerLocation } from '../models/HSBuyerLocation';
import { LocationApprovalThresholdUpdate } from '../models/LocationApprovalThresholdUpdate';
import { LocationPermissionUpdate } from '../models/LocationPermissionUpdate';
import { ListPage } from '../models/ListPage';
import { HSUser } from '../models/HSUser';
import { HSLocationUserGroup } from '../models/HSLocationUserGroup';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class BuyerLocations {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.Create = this.Create.bind(this);
        this.Get = this.Get.bind(this);
        this.Save = this.Save.bind(this);
        this.Delete = this.Delete.bind(this);
        this.ListLocationApprovalPermissionAsssignments = this.ListLocationApprovalPermissionAsssignments.bind(this);
        this.GetApprovalThreshold = this.GetApprovalThreshold.bind(this);
        this.SetLocationApprovalThreshold = this.SetLocationApprovalThreshold.bind(this);
        this.ListLocationPermissionUserGroups = this.ListLocationPermissionUserGroups.bind(this);
        this.UpdateLocationPermissions = this.UpdateLocationPermissions.bind(this);
        this.ListLocationUsers = this.ListLocationUsers.bind(this);
        this.ListUserGroupsForNewUser = this.ListUserGroupsForNewUser.bind(this);
        this.ListUserGroupsByCountry = this.ListUserGroupsByCountry.bind(this);
        this.ListUserUserGroupAssignments = this.ListUserUserGroupAssignments.bind(this);
        this.ReassignUserGroups = this.ReassignUserGroups.bind(this);
    }

   /**
    * @param buyerID ID of the buyer.
    * @param hSBuyerLocation 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Create(buyerID: string, hSBuyerLocation: HSBuyerLocation, accessToken?: string ): Promise<RequiredDeep<HSBuyerLocation>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/buyerlocations/${buyerID}`, hSBuyerLocation, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param buyerLocationID ID of the buyer location.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Get(buyerID: string, buyerLocationID: string,  accessToken?: string ): Promise<RequiredDeep<HSBuyerLocation>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/buyerlocations/${buyerID}/${buyerLocationID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param buyerLocationID ID of the buyer location.
    * @param hSBuyerLocation 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Save(buyerID: string, buyerLocationID: string, hSBuyerLocation: HSBuyerLocation, accessToken?: string ): Promise<RequiredDeep<HSBuyerLocation>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/buyerlocations/${buyerID}/${buyerLocationID}`, hSBuyerLocation, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param buyerLocationID ID of the buyer location.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Delete(buyerID: string, buyerLocationID: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.delete(`/buyerlocations/${buyerID}/${buyerLocationID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param buyerLocationID ID of the buyer location.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ListLocationApprovalPermissionAsssignments(buyerID: string, buyerLocationID: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/buyerlocations/${buyerID}/${buyerLocationID}/approvalpermissions`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param buyerLocationID ID of the buyer location.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async GetApprovalThreshold(buyerID: string, buyerLocationID: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/buyerlocations/${buyerID}/${buyerLocationID}/approvalthreshold`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param buyerLocationID ID of the buyer location.
    * @param locationApprovalThresholdUpdate 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async SetLocationApprovalThreshold(buyerID: string, buyerLocationID: string, locationApprovalThresholdUpdate: LocationApprovalThresholdUpdate, accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/buyerlocations/${buyerID}/${buyerLocationID}/approvalthreshold`, locationApprovalThresholdUpdate, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param buyerLocationID ID of the buyer location.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ListLocationPermissionUserGroups(buyerID: string, buyerLocationID: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/buyerlocations/${buyerID}/${buyerLocationID}/permissions`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param buyerLocationID ID of the buyer location.
    * @param locationPermissionUpdate 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async UpdateLocationPermissions(buyerID: string, buyerLocationID: string, locationPermissionUpdate: LocationPermissionUpdate, accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/buyerlocations/${buyerID}/${buyerLocationID}/permissions`, locationPermissionUpdate, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param buyerLocationID ID of the buyer location.
    * @param options.search Word or phrase to search for.
    * @param options.searchOn Comma-delimited list of fields to search on.
    * @param options.sortBy Comma-delimited list of fields to sort by.
    * @param options.page Page of results to return. Default: 1
    * @param options.pageSize Number of results to return per page. Default: 20, max: 100.
    * @param options.filters An object whose keys match the model, and the values are the values to filter by
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ListLocationUsers(buyerID: string, buyerLocationID: string,  options: ListArgs<HSUser> = {}, accessToken?: string ): Promise<RequiredDeep<ListPage<HSUser>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/buyerlocations/${buyerID}/${buyerLocationID}/users`, { params: { ...options,  filters: options.filters, accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param homeCountry Home country of the hs location user group.
    * @param options.search Word or phrase to search for.
    * @param options.searchOn Comma-delimited list of fields to search on.
    * @param options.sortBy Comma-delimited list of fields to sort by.
    * @param options.page Page of results to return. Default: 1
    * @param options.pageSize Number of results to return per page. Default: 20, max: 100.
    * @param options.filters An object whose keys match the model, and the values are the values to filter by
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ListUserGroupsForNewUser(buyerID: string, homeCountry: string,  options: ListArgs<HSLocationUserGroup> = {}, accessToken?: string ): Promise<RequiredDeep<ListPage<HSLocationUserGroup>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/buyerlocations/${buyerID}/${homeCountry}/usergroups`, { params: { ...options,  filters: options.filters, accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param userID ID of the user.
    * @param options.search Word or phrase to search for.
    * @param options.searchOn Comma-delimited list of fields to search on.
    * @param options.sortBy Comma-delimited list of fields to sort by.
    * @param options.page Page of results to return. Default: 1
    * @param options.pageSize Number of results to return per page. Default: 20, max: 100.
    * @param options.filters An object whose keys match the model, and the values are the values to filter by
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ListUserGroupsByCountry(buyerID: string, userID: string,  options: ListArgs<HSLocationUserGroup> = {}, accessToken?: string ): Promise<RequiredDeep<ListPage<HSLocationUserGroup>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/buyerlocations/${buyerID}/usergroups/${userID}`, { params: { ...options,  filters: options.filters, accessToken, impersonating } } );
    }

   /**
    * @param userGroupType User group type of the user group assignment.
    * @param parentID ID of the parent.
    * @param userID ID of the user.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ListUserUserGroupAssignments(userGroupType: string, parentID: string, userID: string,  accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/buyerlocations/${userGroupType}/${parentID}/usergroupassignments/${userID}`, { params: {  accessToken, impersonating } } );
    }

    public async ReassignUserGroups(buyerID: string, newUserID: string, accessToken?: string): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/buyerlocations/${buyerID}/reassignusergroups/${newUserID}`, undefined, { params: { accessToken, impersonating } } )
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * BuyerLocations.As().List() // lists BuyerLocations using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
