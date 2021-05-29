import { Payment } from 'ordercloud-javascript-sdk';
import { OrderCloudIntegrationsCreditCardPayment } from '../models/OrderCloudIntegrationsCreditCardPayment';
import { RequiredDeep } from '../models/RequiredDeep';
import httpClient from '../utils/HttpClient';

export default class MePayments {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.Post = this.Post.bind(this);
    }

   /**
    * @param orderCloudIntegrationsCreditCardPayment Required fields: OrderID, PaymentID, Currency, MerchantID
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Post(orderCloudIntegrationsCreditCardPayment: OrderCloudIntegrationsCreditCardPayment, accessToken?: string ): Promise<RequiredDeep<Payment>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/me/payments`, orderCloudIntegrationsCreditCardPayment, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * MePayments.As().List() // lists MePayments using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
