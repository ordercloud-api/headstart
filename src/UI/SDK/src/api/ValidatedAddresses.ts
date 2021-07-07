import { Address } from 'ordercloud-javascript-sdk';
import { BuyerAddress, Order } from 'ordercloud-javascript-sdk';
import { RequiredDeep } from '../models/RequiredDeep';
import httpClient from '../utils/HttpClient';

export default class ValidatedAddresses {
    private impersonating: boolean = false;

    /**
    * @ignore
    * not part of public api, don't include in generated docs
    */
    constructor() {
        this.CreateAdminAddress = this.CreateAdminAddress.bind(this);
        this.SaveAdminAddress = this.SaveAdminAddress.bind(this);
        this.PatchAdminAddress = this.PatchAdminAddress.bind(this);
        this.CreateBuyerAddress = this.CreateBuyerAddress.bind(this);
        this.SaveBuyerAddress = this.SaveBuyerAddress.bind(this);
        this.PatchBuyerAddress = this.PatchBuyerAddress.bind(this);
        this.CreateMeAddress = this.CreateMeAddress.bind(this);
        this.SaveMeAddress = this.SaveMeAddress.bind(this);
        this.PatchMeAddress = this.PatchMeAddress.bind(this);
        this.SetBillingAddress = this.SetBillingAddress.bind(this);
        this.SetShippingAddress = this.SetShippingAddress.bind(this);
        this.CreateSupplierAddress = this.CreateSupplierAddress.bind(this);
        this.SaveSupplierAddress = this.SaveSupplierAddress.bind(this);
        this.PatchSupplierAddress = this.PatchSupplierAddress.bind(this);
    }

    /**
     * @param address Required fields: Street1, City, State, Zip, Country
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async CreateAdminAddress(address: Address, accessToken?: string): Promise<RequiredDeep<Address>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/addresses`, address, { params: { accessToken, impersonating } });
    }

    /**
     * @param addressID ID of the address.
     * @param address Required fields: Street1, City, State, Zip, Country
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async SaveAdminAddress(addressID: string, address: Address, accessToken?: string): Promise<RequiredDeep<Address>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/addresses/${addressID}`, address, { params: { accessToken, impersonating } });
    }

    /**
     * @param addressID ID of the address.
     * @param address 
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async PatchAdminAddress(addressID: string, address: Address, accessToken?: string): Promise<RequiredDeep<Address>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.patch(`/addresses/${addressID}`, address, { params: { accessToken, impersonating } });
    }

    /**
     * @param buyerID ID of the buyer.
     * @param address Required fields: Street1, City, State, Zip, Country
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async CreateBuyerAddress(buyerID: string, address: Address, accessToken?: string): Promise<RequiredDeep<Address>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/buyers/${buyerID}/addresses`, address, { params: { accessToken, impersonating } });
    }

    /**
     * @param buyerID ID of the buyer.
     * @param addressID ID of the address.
     * @param address Required fields: Street1, City, State, Zip, Country
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async SaveBuyerAddress(buyerID: string, addressID: string, address: Address, accessToken?: string): Promise<RequiredDeep<Address>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/buyers/${buyerID}/addresses/${addressID}`, address, { params: { accessToken, impersonating } });
    }

    /**
     * @param buyerID ID of the buyer.
     * @param addressID ID of the address.
     * @param address 
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async PatchBuyerAddress(buyerID: string, addressID: string, address: Address, accessToken?: string): Promise<RequiredDeep<Address>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.patch(`/buyers/${buyerID}/addresses/${addressID}`, address, { params: { accessToken, impersonating } });
    }

    /**
     * @param buyerAddress Required fields: Street1, City, State, Zip, Country
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async CreateMeAddress(buyerAddress: BuyerAddress, accessToken?: string): Promise<RequiredDeep<BuyerAddress>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/me/addresses`, buyerAddress, { params: { accessToken, impersonating } });
    }

    /**
     * @param addressID ID of the address.
     * @param buyerAddress Required fields: Street1, City, State, Zip, Country
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async SaveMeAddress(addressID: string, buyerAddress: BuyerAddress, accessToken?: string): Promise<RequiredDeep<BuyerAddress>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/me/addresses/${addressID}`, buyerAddress, { params: { accessToken, impersonating } });
    }

    /**
     * @param addressID ID of the address.
     * @param buyerAddress 
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async PatchMeAddress(addressID: string, buyerAddress: BuyerAddress, accessToken?: string): Promise<void> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.patch(`/me/addresses/${addressID}`, buyerAddress, { params: { accessToken, impersonating } });
    }

    /**
     * @param direction Direction of the address. Possible values: Incoming, Outgoing.
     * @param orderID ID of the order.
     * @param address Required fields: Street1, City, State, Zip, Country
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async SetBillingAddress(direction: 'Incoming' | 'Outgoing', orderID: string, address: Address, accessToken?: string): Promise<RequiredDeep<Order>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/order/${direction}/${orderID}/billto`, address, { params: { accessToken, impersonating } });
    }

    /**
     * @param direction Direction of the address. Possible values: Incoming, Outgoing.
     * @param orderID ID of the order.
     * @param address Required fields: Street1, City, State, Zip, Country
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async SetShippingAddress(direction: 'Incoming' | 'Outgoing', orderID: string, address: Address, accessToken?: string): Promise<RequiredDeep<Order>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/order/${direction}/${orderID}/shipto`, address, { params: { accessToken, impersonating } });
    }

    /**
     * @param supplierID ID of the supplier.
     * @param address Required fields: Street1, City, State, Zip, Country
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async CreateSupplierAddress(supplierID: string, address: Address, accessToken?: string): Promise<RequiredDeep<Address>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.post(`/suppliers/${supplierID}/addresses`, address, { params: { accessToken, impersonating } });
    }

    /**
     * @param supplierID ID of the supplier.
     * @param addressID ID of the address.
     * @param address Required fields: Street1, City, State, Zip, Country
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async SaveSupplierAddress(supplierID: string, addressID: string, address: Address, accessToken?: string): Promise<RequiredDeep<Address>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.put(`/suppliers/${supplierID}/addresses/${addressID}`, address, { params: { accessToken, impersonating } });
    }

    /**
     * @param supplierID ID of the supplier.
     * @param addressID ID of the address.
     * @param address 
     * @param accessToken Provide an alternative token to the one stored in the sdk instance (useful for impersonation).
     */
    public async PatchSupplierAddress(supplierID: string, addressID: string, address: Address, accessToken?: string): Promise<RequiredDeep<Address>> {
        const impersonating = this.impersonating;
        this.impersonating = false;
        return await httpClient.patch(`/suppliers/${supplierID}/addresses/${addressID}`, address, { params: { accessToken, impersonating } });
    }

    /**
     * @description 
     * enables impersonation by calling the subsequent method with the stored impersonation token
     * 
     * @example
     * ValidatedAddresses.As().List() // lists ValidatedAddresses using the impersonated users' token
     */
    public As(): this {
        this.impersonating = true;
        return this;
    }
}
