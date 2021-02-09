import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import {
  Supplier,
  OcSupplierService,
  OcMeService,
  OcSupplierUserGroupService,
} from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { HeadStartSDK, ListArgs } from '@ordercloud/headstart-sdk'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { MiddlewareAPIService } from '@app-seller/shared/services/middleware-api/middleware-api.service'
import { Suppliers } from 'ordercloud-javascript-sdk'

export const SUPPLIER_SUB_RESOURCE_LIST = [
  { route: 'users', display: 'ADMIN.NAV.USERS' },
  { route: 'locations', display: 'ALIAS.SUPPLIER_LOCATIONS' },
]

// TODO - this service is only relevent if you're already on the supplier details page. How can we enforce/inidcate that?
@Injectable({
  providedIn: 'root',
})
export class SupplierService extends ResourceCrudService<Supplier> {
  ocSupplierService: OcSupplierService

  emptyResource = {
    Name: '',
    xp: {
      Description: '',
      Currency: null,
      CountriesServicing: [],
      Images: [{ URL: '', Tag: null }],
      SupportContact: { Name: '', Email: '', Phone: '' },
      SyncFrieghtPop: false,
    },
  }

  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    private ocSupplierUserGroupService: OcSupplierUserGroupService,
    private ocMeService: OcMeService,
    public currentUserService: CurrentUserService,
    private middleware: MiddlewareAPIService
  ) {
    super(
      router,
      activatedRoute,
      Suppliers,
      currentUserService,
      '/suppliers',
      'suppliers',
      SUPPLIER_SUB_RESOURCE_LIST
    )
  }

  async createNewResource(resource: any): Promise<any> {
    resource.ID = '{supplierIncrementor}'
    if (!resource.xp?.Images[0]?.URL) resource.xp.Images = []
    const newSupplier = await HeadStartSDK.Suppliers.Create(resource)
    this.resourceSubject.value.Items = [
      ...this.resourceSubject.value.Items,
      newSupplier,
    ]
    this.resourceSubject.next(this.resourceSubject.value)
    return newSupplier
  }

  async updateResource(originalID: string, resource: any): Promise<any> {
    //  if supplier user updating supplier need to call route in middleware because they dont have required role.
    const newResource = await this.middleware.updateSupplier(
      originalID,
      resource
    )
    this.updateResourceSubject(newResource)
    return newResource
  }

  addIntrinsicListArgs(options: ListArgs): ListArgs {
    options.sortBy = ['NAME']
    return options
  }
}
