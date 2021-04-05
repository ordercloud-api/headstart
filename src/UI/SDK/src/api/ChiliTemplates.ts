import { ChiliTemplate } from '../models/ChiliTemplate';
import { MeChiliTemplate } from '../models/MeChiliTemplate';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class ChiliTemplates {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.Get = this.Get.bind(this);
        this.GetMe = this.GetMe.bind(this);
    }

   /**
    * @param templateID ID of the template.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Get(templateID: string,  accessToken?: string ): Promise<RequiredDeep<ChiliTemplate>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/chili/template/${templateID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param templateID ID of the template.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async GetMe(templateID: string,  accessToken?: string ): Promise<RequiredDeep<MeChiliTemplate>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/chili/template/me/${templateID}`, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * ChiliTemplates.As().List() // lists ChiliTemplates using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
