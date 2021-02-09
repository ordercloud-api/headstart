import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { BuyerTempService } from '@app-seller/shared/services/middleware-api/buyer-temp.service'
import { Buyer, Buyers } from 'ordercloud-javascript-sdk'
import { ListArgs } from '@ordercloud/headstart-sdk'

export const BUYER_SUB_RESOURCE_LIST = [
  { route: 'users', display: 'ADMIN.NAV.USERS' },
  { route: 'locations', display: 'ALIAS.BUYER_LOCATIONS' },
  { route: 'payments', display: 'ADMIN.NAV.PAYMENTS' },
  { route: 'approvals', display: 'ADMIN.NAV.APPROVALS' },
  { route: 'catalogs', display: 'ADMIN.NAV.CATALOGS' },
  { route: 'categories', display: 'ADMIN.NAV.CATEGORIES' },
]

// TODO - this service is only relevent if you're already on the product details page. How can we enforce/inidcate that?
@Injectable({
  providedIn: 'root',
})
export class BuyerService extends ResourceCrudService<Buyer> {
  emptyResource = {
    Buyer: {
      Name: '',
      Active: true,
      xp: {
        ChiliPublishFolder: '',
      },
    },
    Markup: {
      Percent: 0,
    },
  }
  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    currentUserService: CurrentUserService,
    private buyerTempService: BuyerTempService
  ) {
    super(
      router,
      activatedRoute,
      Buyers,
      currentUserService,
      '/buyers',
      'buyers',
      BUYER_SUB_RESOURCE_LIST
    )
  }
  addIntrinsicListArgs(options: ListArgs): ListArgs {
    options.sortBy = ['NAME']
    return options
  }
}
