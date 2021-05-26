import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { Buyer } from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { BUYER_SUB_RESOURCE_LIST } from '../buyers/buyer.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { UserGroups } from 'ordercloud-javascript-sdk'
import { HSLocationUserGroup, ListArgs } from '@ordercloud/headstart-sdk'

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

  // overriding the normal delete resource method because
  // additionally we need to do some cleanup for any
  // location that references the catalog to be deleted
  async deleteResource(resourceID: string): Promise<null> {
    const buyerID = this.getBuyerID()
    const locationsWithCatalogAssigned = await UserGroups.List<HSLocationUserGroup>(
      buyerID,
      {
        filters: {
          'xp.Type': 'BuyerLocation',
          'xp.CatalogAssignments': resourceID,
        },
      }
    )
    const requests = locationsWithCatalogAssigned.Items.map((location) => {
      location.xp.CatalogAssignments = location.xp.CatalogAssignments.filter(
        (x) => x !== resourceID
      )
      return UserGroups.Patch(buyerID, location.ID, {
        xp: {
          CatalogAssignments: location.xp.CatalogAssignments,
        },
      })
    })
    await Promise.all(requests)

    const args = await this.createListArgs([resourceID])
    await this.ocService.Delete(...args)
    this.resourceSubject.value.Items = this.resourceSubject.value.Items.filter(
      (i: any) => i.ID !== resourceID
    )
    this.resourceSubject.next(this.resourceSubject.value)
    return
  }

  getBuyerID(): string {
    const urlPieces = this.router.url.split('/')
    const indexOfParent = urlPieces.indexOf(`${this.primaryResourceLevel}`)
    return urlPieces[indexOfParent + 1]
  }
}
