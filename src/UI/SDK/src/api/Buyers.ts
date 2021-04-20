import { SuperHSBuyer } from '../models/SuperHSBuyer';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class Buyers {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.Create = this.Create.bind(this);
        this.Get = this.Get.bind(this);
        this.Put = this.Put.bind(this);
    }

   /**
    * @param superHSBuyer 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Create(superHSBuyer: SuperHSBuyer, accessToken?: string ): Promise<RequiredDeep<SuperHSBuyer>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/buyer`, superHSBuyer, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Get(buyerID: string,  accessToken?: string ): Promise<RequiredDeep<SuperHSBuyer>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/buyer/${buyerID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param buyerID ID of the buyer.
    * @param superHSBuyer 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Put(buyerID: string, superHSBuyer: SuperHSBuyer, accessToken?: string ): Promise<RequiredDeep<SuperHSBuyer>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/buyer/${buyerID}`, superHSBuyer, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * Buyers.As().List() // lists Buyers using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
