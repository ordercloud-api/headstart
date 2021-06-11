import { CreditCard } from 'ordercloud-javascript-sdk';
import { OrderCloudIntegrationsCreditCardToken } from '../models/OrderCloudIntegrationsCreditCardToken';
import { RequiredDeep } from '../models/RequiredDeep';
import httpClient from '../utils/HttpClient';

export default class CreditCards {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.Post = this.Post.bind(this);
    }

   /**
    * @param buyerID ID of the buyer.
    * @param orderCloudIntegrationsCreditCardToken Required fields: ExpirationDate, CCBillingAddress
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Post(buyerID: string, orderCloudIntegrationsCreditCardToken: OrderCloudIntegrationsCreditCardToken, accessToken?: string ): Promise<RequiredDeep<CreditCard>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/buyers/${buyerID}/creditcards`, orderCloudIntegrationsCreditCardToken, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * CreditCards.As().List() // lists CreditCards using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
