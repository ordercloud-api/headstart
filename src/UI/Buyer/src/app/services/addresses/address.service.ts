import { Injectable } from '@angular/core'
import { Me, Tokens } from 'ordercloud-javascript-sdk'
import { ListArgs } from '@ordercloud/headstart-sdk'
import {
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

  async listAll(args: ListArgs): Promise<ListPage<HSAddressBuyer>> {
    return HeadStartSDK.Services.ListAll(Me, Me.ListAddresses, args)
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
    args: ListArgs = {}, 
    all = false
  ): Promise<ListPage<HSAddressBuyer>> {
    args.filters = { ...args.filters, Editable: 'false' };
    return all ? await this.listAll(args) : await this.list(args)
  }

  async listShippingAddresses(
    args: ListArgs = {}
  ): Promise<ListPage<HSAddressBuyer>> {
    args.filters = { ...args.filters, Shipping: 'true' };
    return await this.list(args)
  }

  // eventually replace with sdk
  async validateAddress(address: HSAddressBuyer): Promise<HSAddressBuyer>  {
    var url = `${this.appConfig.middlewareUrl}/me/addresses/validate`
    var response = await Axios.post(url, address, this.BuildConfig());
    return response.data;
  }

  BuildConfig(): AxiosRequestConfig {
    return { headers: { Authorization: `Bearer ${Tokens.GetAccessToken()}`}};
  }
}
