import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { StorefrontsService } from '../storefronts.service'
import { Router, ActivatedRoute } from '@angular/router'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { ApiClient } from '@ordercloud/angular-sdk'

@Component({
  selector: 'app-storefront-table',
  templateUrl: './storefront-table.component.html',
  styleUrls: ['./storefront-table.component.scss'],
})
export class StorefrontTableComponent extends ResourceCrudComponent<ApiClient> {
  route = 'storefronts'
  constructor(
    private storefrontsService: StorefrontsService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedRoute: ActivatedRoute,
    ngZone: NgZone
  ) {
    super(changeDetectorRef, storefrontsService, router, activatedRoute, ngZone)
  }
}
