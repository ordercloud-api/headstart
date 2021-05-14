import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { ApiClients, ListPage } from 'ordercloud-javascript-sdk'
import { ListArgs, HSApiClient } from '@ordercloud/headstart-sdk'
import { listAll } from '@app-seller/shared/services/listAll'

export const STOREFRONTS_SUB_RESOURCE_LIST = [
  { route: 'pages', display: 'Pages' },
]

// TODO - this service is only relevent if you're already on the product details page. How can we enforce/inidcate that?
@Injectable({
  providedIn: 'root',
})
export class StorefrontsService extends ResourceCrudService<HSApiClient> {
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

  async list(args: any[]): Promise<ListPage<HSApiClient>> {
    // ApiClients xp can't be indexed so we must retrive all and then filter client-side
    const listResponse = await listAll<HSApiClient>(
      ApiClients,
      ApiClients.List.bind(this),
      {
        page: 1,
        pageSize: 100,
      }
    )
    listResponse.Items = listResponse.Items.filter(
      (apiClient) => apiClient?.xp?.IsStorefront == true
    )
    return listResponse
  }
}
