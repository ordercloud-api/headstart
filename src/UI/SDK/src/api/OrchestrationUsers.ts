import { HSAddressBuyer } from '../models/HSAddressBuyer';
import { HSAddressAssignment } from '../models/HSAddressAssignment';
import { HSCostCenter } from '../models/HSCostCenter';
import { HSUser } from '../models/HSUser';
import { HSUserGroup } from '../models/HSUserGroup';
import { HSUserGroupAssignment } from '../models/HSUserGroupAssignment';
import { HSBuyer } from '../models/HSBuyer';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class OrchestrationUsers {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.PostAddress = this.PostAddress.bind(this);
        this.PostAddressAssignment = this.PostAddressAssignment.bind(this);
        this.PostCostCenter = this.PostCostCenter.bind(this);
        this.PostUser = this.PostUser.bind(this);
        this.PostUserGroup = this.PostUserGroup.bind(this);
        this.PostUserGroupAssignment = this.PostUserGroupAssignment.bind(this);
        this.PostBuyer = this.PostBuyer.bind(this);
    }

   /**
    * @param buyerId Buyer id of the hs address buyer.
    * @param hSAddressBuyer Required fields: Street1, City, State, Zip, Country
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostAddress(buyerId: string, hSAddressBuyer: HSAddressBuyer, accessToken?: string ): Promise<RequiredDeep<HSAddressBuyer>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/${buyerId}/address`, hSAddressBuyer, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerId Buyer id of the hs address assignment.
    * @param hSAddressAssignment Required fields: AddressID
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostAddressAssignment(buyerId: string, hSAddressAssignment: HSAddressAssignment, accessToken?: string ): Promise<RequiredDeep<HSAddressAssignment>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/${buyerId}/addressassignment`, hSAddressAssignment, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerId Buyer id of the hs cost center.
    * @param hSCostCenter Required fields: Name
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostCostCenter(buyerId: string, hSCostCenter: HSCostCenter, accessToken?: string ): Promise<RequiredDeep<HSCostCenter>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/${buyerId}/costcenter`, hSCostCenter, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerId Buyer id of the hs user.
    * @param hSUser Required fields: Username, FirstName, LastName, Email, Active
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostUser(buyerId: string, hSUser: HSUser, accessToken?: string ): Promise<RequiredDeep<HSUser>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/${buyerId}/user`, hSUser, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerId Buyer id of the hs user group.
    * @param hSUserGroup Required fields: Name
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostUserGroup(buyerId: string, hSUserGroup: HSUserGroup, accessToken?: string ): Promise<RequiredDeep<HSUserGroup>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/${buyerId}/usergroup`, hSUserGroup, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerId Buyer id of the hs user group assignment.
    * @param hSUserGroupAssignment 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostUserGroupAssignment(buyerId: string, hSUserGroupAssignment: HSUserGroupAssignment, accessToken?: string ): Promise<RequiredDeep<HSUserGroupAssignment>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/${buyerId}/usergroupassignment`, hSUserGroupAssignment, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param hSBuyer Required fields: Name
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostBuyer(hSBuyer: HSBuyer, accessToken?: string ): Promise<RequiredDeep<HSBuyer>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/buyer`, hSBuyer, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * OrchestrationUsers.As().List() // lists OrchestrationUsers using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
