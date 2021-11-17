import { ReflektionAccessToken } from '..';
import httpClient from '../utils/HttpClient';

export default class Reflektion {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.GetToken = this.GetToken.bind(this);
    }

   /**
    * @param orderID ID of the order.
    * @param paymentUpdateRequest 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async GetToken(accessToken?: string ): Promise<ReflektionAccessToken> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/reflektion/token`, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * Payments.As().List() // lists Payments using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}