import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Category } from '@ordercloud/angular-sdk'
import { Router, ActivatedRoute } from '@angular/router'
import { BuyerCategoryService } from '@app-seller/shared/services/buyer/buyer-category-service'
import { BuyerService } from '../../buyers/buyer.service'

@Component({
  selector: 'app-buyer-category-table',
  templateUrl: './buyer-category-table.component.html',
  styleUrls: ['./buyer-category-table.component.scss'],
})
export class BuyerCategoryTableComponent extends ResourceCrudComponent<Category> {
  constructor(
    private buyerCategoryService: BuyerCategoryService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedroute: ActivatedRoute,
    private buyerService: BuyerService,
    ngZone: NgZone
  ) {
    super(
      changeDetectorRef,
      buyerCategoryService,
      router,
      activatedroute,
      ngZone
    )
  }

  async createNewResource(): Promise<void> {
    // dataIsSaving indicator is used in the resource table to conditionally tell the
    // submit button to disable
    const routeUrl = this.router.routerState.snapshot.url
    if (!this.updatedResource?.ParentID && routeUrl.includes('?')) {
      const splitUrl = routeUrl.split('=')
      const endUrl = splitUrl[splitUrl.length - 1]
      this.updatedResource.ParentID = endUrl
    }
    super.createNewResource()
  }
}
