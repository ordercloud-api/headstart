import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import {
  OcSupplierAddressService,
  Address,
  Supplier,
  ListPage,
} from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { SUPPLIER_SUB_RESOURCE_LIST } from '../suppliers/supplier.service'
import { HeadStartSDK } from '@ordercloud/headstart-sdk'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { SupplierAddresses } from 'ordercloud-javascript-sdk'

@Injectable({
  providedIn: 'root',
})
export class SupplierAddressService extends ResourceCrudService<Address> {
  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    private ocSupplierAddressService: OcSupplierAddressService,
    public currentUserService: CurrentUserService
  ) {
    super(
      router,
      activatedRoute,
      SupplierAddresses,
      currentUserService,
      '/suppliers',
      'suppliers',
      SUPPLIER_SUB_RESOURCE_LIST,
      'locations',
      '/my-supplier'
    )
  }

  async createNewResource(resource: any): Promise<any> {
    // special iding process for supplier addresses
    const parentResourceID = await this.getParentResourceID()
    const existingAddresses = await SupplierAddresses.List(parentResourceID)
    const newID = this.getIncrementedID(parentResourceID, existingAddresses)
    resource.ID = newID

    const newResource = await HeadStartSDK.ValidatedAddresses.CreateSupplierAddress(
      parentResourceID,
      resource
    )
    this.resourceSubject.value.Items = [
      ...this.resourceSubject.value.Items,
      newResource,
    ]
    this.resourceSubject.next(this.resourceSubject.value)
    return newResource
  }

  async updateResource(originalID: string, resource: any): Promise<any> {
    const parentResourceID = await this.getParentResourceID()
    const newResource = await HeadStartSDK.ValidatedAddresses.SaveSupplierAddress(
      parentResourceID,
      originalID,
      resource
    )
    const resourceIndex = this.resourceSubject.value.Items.findIndex(
      (i: any) => i.ID === newResource.ID
    )
    this.resourceSubject.value.Items[resourceIndex] = newResource
    this.resourceSubject.next(this.resourceSubject.value)
    return newResource
  }

  private getIncrementedID(
    supplierID: string,
    existingAddresses: ListPage<Address>
  ): string {
    const numbers = existingAddresses.Items.map((a) =>
      Number(a.ID.split('-')[1])
    )
    const highestNumber = Math.max(...numbers)
    const nextID = highestNumber === -Infinity ? 1 : highestNumber + 1
    return `${supplierID}-${nextID.toString().padStart(2, '0')}`
  }

  emptyResource = {
    CompanyName: '',
    FirstName: '',
    LastName: '',
    Street1: '',
    Street2: '',
    City: '',
    State: '',
    Zip: '',
    Country: '',
    Phone: '',
    AddressName: '',
    xp: null,
  }
}
