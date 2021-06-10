import httpClient from '../utils/HttpClient';
import { ApprovalRule } from 'ordercloud-javascript-sdk';

export default class ApprovalRules {
    private impersonating: boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.SaveApprovalRule = this.SaveApprovalRule.bind(this);
    }

    /**
     * @param buyerID ID of thebuyer
     * @param locationID ID of the location this approval rule will apply to
     * @param approval The approval to be created or updated
     */

    public async SaveApprovalRule(
        buyerID: string,
        locationID: string,
        approval: ApprovalRule,
        accessToken?: string): Promise<ApprovalRule> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/buyerlocations/${buyerID}/${locationID}/approval`, approval, { params: { accessToken, impersonating } })
    }

    public async DeleteApprovalRule(
        buyerID: string,
        locationID: string,
        approvalID: string,
        accessToken?: string): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/buyerlocations/${buyerID}/${locationID}/approval/${approvalID}`, undefined, { params: { accessToken, impersonating } })
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