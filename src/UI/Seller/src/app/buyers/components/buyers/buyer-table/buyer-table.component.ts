import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { Buyer } from '@ordercloud/angular-sdk'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Router, ActivatedRoute } from '@angular/router'
import { BuyerService } from '../buyer.service'
import { HSBuyer } from '@ordercloud/headstart-sdk'
import { FormControl, FormGroup, Validators } from '@angular/forms'

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
