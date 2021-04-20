import { ListPage } from '../models/ListPage';
import { OrderCloudIntegrationsConversionRate } from '../models/OrderCloudIntegrationsConversionRate';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class ExchangeRates {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.Get = this.Get.bind(this);
        this.GetRateList = this.GetRateList.bind(this);
    }

   /**
    * @param currency Currency of the order cloud integrations conversion rate. Possible values: CAD, HKD, ISK, PHP, DKK, HUF, CZK, GBP, RON, SEK, IDR, INR, BRL, RUB, HRK, JPY, THB, CHF, EUR, MYR, BGN, TRY, CNY, NOK, NZD, ZAR, USD, MXN, SGD, AUD, ILS, KRW, PLN.
    * @param options.search Word or phrase to search for.
    * @param options.searchOn Comma-delimited list of fields to search on.
    * @param options.sortBy Comma-delimited list of fields to sort by.
    * @param options.page Page of results to return. Default: 1
    * @param options.pageSize Number of results to return per page. Default: 20, max: 100.
    * @param options.filters An object whose keys match the model, and the values are the values to filter by
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Get(currency: 'CAD' | 'HKD' | 'ISK' | 'PHP' | 'DKK' | 'HUF' | 'CZK' | 'GBP' | 'RON' | 'SEK' | 'IDR' | 'INR' | 'BRL' | 'RUB' | 'HRK' | 'JPY' | 'THB' | 'CHF' | 'EUR' | 'MYR' | 'BGN' | 'TRY' | 'CNY' | 'NOK' | 'NZD' | 'ZAR' | 'USD' | 'MXN' | 'SGD' | 'AUD' | 'ILS' | 'KRW' | 'PLN',  options: ListArgs<OrderCloudIntegrationsConversionRate> = {}, accessToken?: string ): Promise<RequiredDeep<ListPage<OrderCloudIntegrationsConversionRate>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/exchangerates/${currency}`, { params: { ...options,  filters: options.filters, accessToken, impersonating } } );
    }

   /**
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async GetRateList( accessToken?: string ): Promise<RequiredDeep<ListPage<OrderCloudIntegrationsConversionRate>>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/exchangerates/supportedrates`, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * ExchangeRates.As().List() // lists ExchangeRates using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
