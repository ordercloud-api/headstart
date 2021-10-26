import { TaxCategorization } from ".";

export interface TaxCategorizationResponse {
    ProductsShouldHaveTaxCodes?: boolean;
    Categories?: TaxCategorization[];
}