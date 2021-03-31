import { PaymentUpdateRequest } from '../models/PaymentUpdateRequest';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class Payments {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.SavePayments = this.SavePayments.bind(this);
    }

   /**
    * @param orderID ID of the order.
    * @param paymentUpdateRequest 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async SavePayments(orderID: string, paymentUpdateRequest: PaymentUpdateRequest, accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/payments/${orderID}/update`, paymentUpdateRequest, { params: {  accessToken, impersonating } } );
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
