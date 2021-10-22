import { TaxCategorization } from ".";

export interface TaxCategorizationResponse {
    IsImplemented?: boolean;
    Categories?: TaxCategorization[];
}