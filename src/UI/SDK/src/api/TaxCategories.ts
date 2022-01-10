import { RequiredDeep } from '../models/RequiredDeep';
import httpClient from '../utils/HttpClient';
import { TaxCategorization, TaxCategorizationResponse } from '..';

export default class TaxCategories {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.ListTaxCategories = this.ListTaxCategories.bind(this);
    }

   /**
    * @param options.search Word or phrase to search for.
    * @param options.searchOn Comma-delimited list of fields to search on.
    * @param options.sortBy Comma-delimited list of fields to sort by.
    * @param options.page Page of results to return. Default: 1
    * @param options.pageSize Number of results to return per page. Default: 20, max: 100.
    * @param options.filters An object whose keys match the model, and the values are the values to filter by
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async ListTaxCategories(search: string = "", accessToken?: string ): Promise<RequiredDeep<TaxCategorizationResponse>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/tax-category`, { params: { search, accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * TaxCategories.As().List() // lists TaxCategories using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
