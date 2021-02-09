import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { ApiClient } from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { HeadStartSDK, ListArgs } from '@ordercloud/headstart-sdk'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { ApiClients } from 'ordercloud-javascript-sdk'

export const STOREFRONTS_SUB_RESOURCE_LIST = [
  { route: 'pages', display: 'Pages' },
]

// TODO - this service is only relevent if you're already on the product details page. How can we enforce/inidcate that?
@Injectable({
  providedIn: 'root',
})
export class StorefrontsService extends ResourceCrudService<ApiClient> {
  emptyResource = {
    ID: '',
    ClientSecret: null,
    AccessTokenDuration: 600,
    Active: false,
    AppName: '',
    RefreshTokenDuration: 43200,
    DefaultContextUserName: null,
    xp: null,
    AllowAnyBuyer: false,
    AllowAnySupplier: false,
    AllowSeller: false,
    IsAnonBuyer: false,
    AssignedBuyerCount: 0,
    AssignedSupplierCount: 0,
    OrderCheckoutIntegrationEventID: null,
    OrderCheckoutIntegrationEventName: null,
  }
  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    currentUserService: CurrentUserService
  ) {
    super(
      router,
      activatedRoute,
      ApiClients,
      currentUserService,
      '/storefronts',
      'storefronts',
      STOREFRONTS_SUB_RESOURCE_LIST
    )
  }

  addIntrinsicListArgs(options: ListArgs): ListArgs {
    options.filters = { AppName: 'Storefront*' }
    return options
  }
}
