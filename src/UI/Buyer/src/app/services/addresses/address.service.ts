import { Injectable } from '@angular/core'
import { Me, Tokens } from 'ordercloud-javascript-sdk'
import { ListArgs } from '@ordercloud/headstart-sdk'
import {
  TaxCertificate,
  HSAddressBuyer,
  HeadStartSDK,
  ListPage,
} from '@ordercloud/headstart-sdk'
import Axios, { AxiosRequestConfig } from 'axios'
import { AppConfig } from 'src/app/models/environment.types'

@Injectable({
  providedIn: 'root',
})
export class AddressService {
  constructor(
    private appConfig: AppConfig
  ) {}

  async get(addressID: string): Promise<HSAddressBuyer> {
    return Me.GetAddress(addressID)
  }

  async list(args: ListArgs): Promise<ListPage<HSAddressBuyer>> {
    return Me.ListAddresses(args as any)
  }

  async create(
    address: HSAddressBuyer
  ): Promise<HSAddressBuyer> {
    return HeadStartSDK.ValidatedAddresses.CreateMeAddress(address)
  }

  async edit(
    addressID: string,
    address: HSAddressBuyer
  ): Promise<HSAddressBuyer> {
    return HeadStartSDK.ValidatedAddresses.SaveMeAddress(addressID, address)
  }

  async delete(addressID: string): Promise<void> {
    return Me.DeleteAddress(addressID)
  }

  async listBuyerLocations(
    args: ListArgs = {}
  ): Promise<ListPage<HSAddressBuyer>> {
    args.filters = { ...args.filters, Editable: 'false' };
    return await this.list(args)
  }

  async listShippingAddresses(
    args: ListArgs = {}
  ): Promise<ListPage<HSAddressBuyer>> {
    args.filters = { ...args.filters, Shipping: 'true' };
    return await this.list(args)
  }

  async createCertificate(
    locationID: string,
    certificate: TaxCertificate
  ): Promise<TaxCertificate> {
    var url = `${this.appConfig.middlewareUrl}/avalara/certificate/${locationID}`;
    var response = await Axios.post(url, certificate, this.BuildConfig());
    return response.data;
  }

  async updateCertificate(
    locationID: string,
    certificate: TaxCertificate
  ): Promise<TaxCertificate> {
    var url = `${this.appConfig.middlewareUrl}/avalara/certificate/${locationID}`;
    var response = await Axios.put(url, certificate, this.BuildConfig());
    return response.data;
  }

  async getCertificate(locationID: string): Promise<TaxCertificate> {
    var url = `${this.appConfig.middlewareUrl}/avalara/certificate/${locationID}`;
    var response = await Axios.get(url, this.BuildConfig());
    return response.data;
  }

  BuildConfig(): AxiosRequestConfig {
    return { headers: { Authorization: `Bearer ${Tokens.GetAccessToken()}`}};
  }
}
