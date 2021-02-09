import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import {
  Buyer,
  ListPage,
  OcUserGroupService,
  UserGroup,
} from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { BUYER_SUB_RESOURCE_LIST } from '../buyers/buyer.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { UserGroups } from 'ordercloud-javascript-sdk'
import { ListArgs } from '@ordercloud/headstart-sdk'

@Injectable({
  providedIn: 'root',
})
export class BuyerCatalogService extends ResourceCrudService<Buyer> {
  emptyResource = {
    Name: '',
    xp: {
      Type: 'Catalog',
    },
  }

  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    currentUserService: CurrentUserService
  ) {
    super(
      router,
      activatedRoute,
      UserGroups,
      currentUserService,
      '/buyers',
      'buyers',
      BUYER_SUB_RESOURCE_LIST,
      'catalogs'
    )
  }

  // Overwritten from resource-crud.service.ts
  addIntrinsicListArgs(options: ListArgs): ListArgs {
    options.filters = { 'xp.Type': 'Catalog' }
    return options
  }
}
