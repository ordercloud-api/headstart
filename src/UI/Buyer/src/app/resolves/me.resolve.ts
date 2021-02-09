import { Injectable } from '@angular/core'
import { Resolve } from '@angular/router'
import { Me, BuyerAddress } from 'ordercloud-javascript-sdk'
import { ListPage, HSAddressBuyer } from '@ordercloud/headstart-sdk'
import { AddressService } from '../services/addresses/address.service'

@Injectable()
export class MeListAddressResolver implements Resolve<ListPage<BuyerAddress>> {
  constructor() {}

  resolve(): Promise<ListPage<BuyerAddress>> {
    return Me.ListAddresses()
  }
}

@Injectable()
export class MeListBuyerLocationResolver
  implements Resolve<ListPage<HSAddressBuyer>> {
  constructor(private service: AddressService) {}

  resolve(): Promise<ListPage<HSAddressBuyer>> {
    return this.service.listBuyerLocations()
  }
}
