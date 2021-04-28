import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { ProductFacet } from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { ProductFacets } from 'ordercloud-javascript-sdk'

@Injectable({
  providedIn: 'root',
})

export class FacetService extends ResourceCrudService<ProductFacet> {
  emptyResource = {
    ID: '',
    Name: '',
    MinCount: 1, // Default
    xp: {
      Options: null,
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
      ProductFacets,
      currentUserService,
      '/facets',
      'facets'
    )
  }
}
