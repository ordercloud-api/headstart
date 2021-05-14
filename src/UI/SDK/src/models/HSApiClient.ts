import { ApiClient } from 'ordercloud-javascript-sdk';

export type HSApiClient = ApiClient<ApiClientXP>

interface ApiClientXP {
    IsStorefront?: boolean;
    AnonBuyerID?: string;
    WebsiteUrl?: string;
}