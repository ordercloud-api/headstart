import { HSSupplier } from '../models/HSSupplier';
import { HSSupplierOrderData } from '../models/HSSupplierOrderData';
import { RequiredDeep } from '../models/RequiredDeep';
import { ListArgs } from '../models/ListArgs'
import httpClient from '../utils/HttpClient';

export default class Suppliers {
    private impersonating:boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.Create = this.Create.bind(this);
        this.CanDeleteLocation = this.CanDeleteLocation.bind(this);
        this.GetMySupplier = this.GetMySupplier.bind(this);
        this.GetSupplierOrder = this.GetSupplierOrder.bind(this);
    }

   /**
    * @param hSSupplier Required fields: Name
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async Create(hSSupplier: HSSupplier, accessToken?: string ): Promise<RequiredDeep<HSSupplier>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/supplier`, hSSupplier, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param locationID ID of the location.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async CanDeleteLocation(locationID: string,  accessToken?: string ): Promise<boolean> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/supplier/candelete/${locationID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param supplierID ID of the supplier.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async GetMySupplier(supplierID: string,  accessToken?: string ): Promise<RequiredDeep<HSSupplier>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/supplier/me/${supplierID}`, { params: {  accessToken, impersonating } } );
    }

   /**
    * @param supplierOrderID ID of the supplier order.
    * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
    */
    public async GetSupplierOrder(supplierOrderID: string, orderType: string, accessToken?: string ): Promise<RequiredDeep<HSSupplierOrderData>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.get(`/supplier/orderdetails/${supplierOrderID}/${orderType}`, { params: {  accessToken, impersonating } } );
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * Suppliers.As().List() // lists Suppliers using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
