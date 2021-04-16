import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { OcAdminAddressService, Address } from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { HeadStartSDK } from '@ordercloud/headstart-sdk'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { AdminAddresses } from 'ordercloud-javascript-sdk'

@Injectable({
  providedIn: 'root',
})
export class SellerAddressService extends ResourceCrudService<Address> {
  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    private ocAdminAddressService: OcAdminAddressService,
    public currentUserService: CurrentUserService
  ) {
    super(
      router,
      activatedRoute,
      AdminAddresses,
      currentUserService,
      '/seller-admin/locations',
      'locations'
    )
  }

  async createNewResource(resource: any): Promise<any> {
    const newID = '{sellerLocationIncrementor}'
    resource.ID = newID

    const newResource = await HeadStartSDK.ValidatedAddresses.CreateAdminAddress(
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
    const newResource = await HeadStartSDK.ValidatedAddresses.SaveAdminAddress(
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
