import { OrderCloudIntegrationsCreditCardToken } from '../models/OrderCloudIntegrationsCreditCardToken';
import { RequiredDeep } from '../models/RequiredDeep';
import httpClient from '../utils/HttpClient';
import { BuyerCreditCard } from 'ordercloud-javascript-sdk';

export default class MeCreditCardAuthorizations {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.MePost = this.MePost.bind(this);
    }

   /**
    * @param orderCloudIntegrationsCreditCardToken Required fields: ExpirationDate, CCBillingAddress
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async MePost(orderCloudIntegrationsCreditCardToken: OrderCloudIntegrationsCreditCardToken, accessToken?: string ): Promise<RequiredDeep<BuyerCreditCard>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/me/creditcards`, orderCloudIntegrationsCreditCardToken, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * MeCreditCardAuthorizations.As().List() // lists MeCreditCardAuthorizations using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
