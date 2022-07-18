import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Router, ActivatedRoute } from '@angular/router'
import { BuyerService } from '../buyer.service'
import { Buyer } from 'ordercloud-javascript-sdk'

@Component({
  selector: 'buyer-table',
  templateUrl: './buyer-table.component.html',
  styleUrls: ['./buyer-table.component.scss'],
})
export class BuyerTableComponent extends ResourceCrudComponent<Buyer> {
  route = 'buyer'

  constructor(
    private buyerService: BuyerService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedRoute: ActivatedRoute,
    ngZone: NgZone
  ) {
    super(changeDetectorRef, buyerService, router, activatedRoute, ngZone)
  }
}
