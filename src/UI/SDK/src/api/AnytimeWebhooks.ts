import { ListNotification } from '../models/ListNotification';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class AnytimeWebhooks {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.HandleAnytimeEventNotification = this.HandleAnytimeEventNotification.bind(this);
    }

   /**
    * @param listNotification 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async HandleAnytimeEventNotification(listNotification: ListNotification, accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/api/af-webhooks`, listNotification, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * AnytimeWebhooks.As().List() // lists AnytimeWebhooks using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
