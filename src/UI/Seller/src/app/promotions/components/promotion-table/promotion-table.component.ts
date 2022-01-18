import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { Promotion } from '@ordercloud/angular-sdk'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Router, ActivatedRoute } from '@angular/router'
import { PromotionService } from '@app-seller/promotions/promotion.service'

@Component({
  selector: 'app-promotion-table',
  templateUrl: './promotion-table.component.html',
  styleUrls: ['./promotion-table.component.scss'],
})
export class PromotionTableComponent extends ResourceCrudComponent<Promotion> {
  constructor(
    private promotionService: PromotionService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedRoute: ActivatedRoute,
    ngZone: NgZone
  ) {
    super(changeDetectorRef, promotionService, router, activatedRoute, ngZone)
  }

  filterConfig = {
    Filters: [
      {
        Display: 'ADMIN.FILTERS.APPLICATION',
        Path: 'xp.Automatic',
        Type: 'Dropdown',
        Items: [
          { Text: 'ADMIN.FILTER_OPTIONS.AUTOMATIC', Value: 'true' },
          { Text: 'ADMIN.FILTER_OPTIONS.CODE', Value: 'false' },
        ],
      },
      {
        Display: 'ADMIN.FILTERS.APPLIES_TO',
        Path: 'xp.AppliesTo',
        Type: 'Dropdown',
        Items: [
          { Text: 'ADMIN.FILTER_OPTIONS.ENTIRE_ORDER', Value: 'EntireOrder' },
          {
            Text: 'ADMIN.FILTER_OPTIONS.SPECIFIC_SUPPLIER',
            Value: 'SpecificSupplier',
          },
          { Text: 'ADMIN.FILTER_OPTIONS.SPECIFIC_SKUS', Value: 'SpecificSKUs' },
        ],
      },
    ],
  }
}
