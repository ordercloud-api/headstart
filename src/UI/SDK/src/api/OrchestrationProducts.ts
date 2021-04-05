import { HSCatalog } from '../models/HSCatalog';
import { HSCatalogAssignment } from '../models/HSCatalogAssignment';
import { SuperHSProduct } from '../models/SuperHSProduct';
import { HSPriceSchedule } from '../models/HSPriceSchedule';
import { HSProduct } from '../models/HSProduct';
import { HSProductAssignment } from '../models/HSProductAssignment';
import { HSProductFacet } from '../models/HSProductFacet';
import { HSSpec } from '../models/HSSpec';
import { HSSpecOption } from '../models/HSSpecOption';
import { HSSpecProductAssignment } from '../models/HSSpecProductAssignment';
import { TemplateProductFlat } from '../models/TemplateProductFlat';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class OrchestrationProducts {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.PostCatalog = this.PostCatalog.bind(this);
        this.PostCatalogProductAssignment = this.PostCatalogProductAssignment.bind(this);
        this.PostHydratedProduct = this.PostHydratedProduct.bind(this);
        this.PostPriceSchedule = this.PostPriceSchedule.bind(this);
        this.PostProduct = this.PostProduct.bind(this);
        this.PostProductAssignment = this.PostProductAssignment.bind(this);
        this.PostProductFacet = this.PostProductFacet.bind(this);
        this.PostSpec = this.PostSpec.bind(this);
        this.PostSpecOption = this.PostSpecOption.bind(this);
        this.PostSpecProductAssignment = this.PostSpecProductAssignment.bind(this);
        this.PostTemplateFlatProduct = this.PostTemplateFlatProduct.bind(this);
    }

   /**
    * @param hSCatalog Required fields: Name
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostCatalog(hSCatalog: HSCatalog, accessToken?: string ): Promise<RequiredDeep<HSCatalog>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/catalog`, hSCatalog, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param hSCatalogAssignment 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostCatalogProductAssignment(hSCatalogAssignment: HSCatalogAssignment, accessToken?: string ): Promise<RequiredDeep<HSCatalogAssignment>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/catalogproductassignment`, hSCatalogAssignment, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param superHSProduct 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostHydratedProduct(superHSProduct: SuperHSProduct, accessToken?: string ): Promise<RequiredDeep<SuperHSProduct>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/hydrated`, superHSProduct, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param hSPriceSchedule Required fields: Name
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostPriceSchedule(hSPriceSchedule: HSPriceSchedule, accessToken?: string ): Promise<RequiredDeep<HSPriceSchedule>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/priceschedule`, hSPriceSchedule, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param hSProduct Required fields: Name, QuantityMultiplier
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostProduct(hSProduct: HSProduct, accessToken?: string ): Promise<RequiredDeep<HSProduct>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/product`, hSProduct, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param hSProductAssignment Required fields: ProductID, BuyerID
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostProductAssignment(hSProductAssignment: HSProductAssignment, accessToken?: string ): Promise<RequiredDeep<HSProductAssignment>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/productassignment`, hSProductAssignment, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param hSProductFacet Required fields: Name, MinCount
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostProductFacet(hSProductFacet: HSProductFacet, accessToken?: string ): Promise<RequiredDeep<HSProductFacet>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/productfacet`, hSProductFacet, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param hSSpec Required fields: Name
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostSpec(hSSpec: HSSpec, accessToken?: string ): Promise<RequiredDeep<HSSpec>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/spec`, hSSpec, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param hSSpecOption Required fields: Value
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostSpecOption(hSSpecOption: HSSpecOption, accessToken?: string ): Promise<RequiredDeep<HSSpecOption>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/specoption`, hSSpecOption, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param options.clientId Client id of the hs spec product assignment.
    * @param hSSpecProductAssignment 
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostSpecProductAssignment(hSSpecProductAssignment: HSSpecProductAssignment, clientId: string, accessToken?: string ): Promise<RequiredDeep<HSSpecProductAssignment>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/specproductassignment`, hSSpecProductAssignment, { params: { clientId,  accessToken, impersonating } } );
    }

   /**
    * @param templateProductFlat Required fields: ID, Name, TaxCategory, TaxCode, Price
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async PostTemplateFlatProduct(templateProductFlat: TemplateProductFlat, accessToken?: string ): Promise<RequiredDeep<TemplateProductFlat>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/orchestration/templateproduct`, templateProductFlat, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * OrchestrationProducts.As().List() // lists OrchestrationProducts using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
