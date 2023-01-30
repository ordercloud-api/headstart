import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Category } from 'ordercloud-javascript-sdk'
import { Router, ActivatedRoute } from '@angular/router'
import { BuyerService } from '../../buyers/buyer.service'
import { BuyerCatalogService } from '../buyer-catalog.service'
import { UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms'

function createBuyerCatalogForm(userGroup: any) {
  return new UntypedFormGroup({
    Name: new UntypedFormControl(userGroup?.Name, Validators.required),
  })
}

@Component({
  selector: 'app-buyer-catalog-table',
  templateUrl: './buyer-catalog-table.component.html',
  styleUrls: ['./buyer-catalog-table.component.scss'],
})
export class BuyerCatalogTableComponent extends ResourceCrudComponent<Category> {
  constructor(
    private buyerCatalogService: BuyerCatalogService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedroute: ActivatedRoute,
    private buyerService: BuyerService,
    ngZone: NgZone
  ) {
    super(
      changeDetectorRef,
      buyerCatalogService,
      router,
      activatedroute,
      ngZone,
      createBuyerCatalogForm
    )
  }
}
