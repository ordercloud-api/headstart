import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class SsoLogins {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.RedirectToAnytimeStorefront = this.RedirectToAnytimeStorefront.bind(this);
        this.RedirectToAnytimeAuthorize = this.RedirectToAnytimeAuthorize.bind(this);
        this.RedirectToWaxingStorefront = this.RedirectToWaxingStorefront.bind(this);
        this.RedirectToWaxingAuthorize = this.RedirectToWaxingAuthorize.bind(this);
    }

   /**
    * @param options.code Code of the sso login.
    * @param options.state State of the sso login.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async RedirectToAnytimeStorefront( code: string, state: string, accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/api/anytime/authorize`, { params: { code, state,  accessToken, impersonating } } );
    }

   /**
    * @param options.path Path of the sso login.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async RedirectToAnytimeAuthorize( path: string, accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/api/anytime/sso`, { params: { path,  accessToken, impersonating } } );
    }

   /**
    * @param options.code Code of the sso login.
    * @param options.state State of the sso login.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async RedirectToWaxingStorefront( code: string, state: string, accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/api/waxing/authorize`, { params: { code, state,  accessToken, impersonating } } );
    }

   /**
    * @param options.path Path of the sso login.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async RedirectToWaxingAuthorize( path: string, accessToken?: string ): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/api/waxing/sso`, { params: { path,  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * SsoLogins.As().List() // lists SsoLogins using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
