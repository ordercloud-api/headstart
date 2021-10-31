import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { ApiClients, ListPage } from 'ordercloud-javascript-sdk'
import { HSApiClient } from '@ordercloud/headstart-sdk'
import { HeadStartSDK } from '@ordercloud/headstart-sdk'

export const STOREFRONTS_SUB_RESOURCE_LIST = []

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
    xp: {
      IsStorefront: true
    },
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
    const listResponse = await HeadStartSDK.Services.ListAll<HSApiClient>(
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
