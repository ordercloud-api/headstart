import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { CreditCard } from '@ordercloud/angular-sdk'
import { Router, ActivatedRoute } from '@angular/router'
import { BuyerPaymentService } from '../buyer-payment.service'
import { BuyerService } from '../../buyers/buyer.service'

@Component({
  selector: 'app-buyer-payment-table',
  templateUrl: './buyer-payment-table.component.html',
  styleUrls: ['./buyer-payment-table.component.scss'],
})
export class BuyerPaymentTableComponent extends ResourceCrudComponent<CreditCard> {
  constructor(
    private buyerPaymentService: BuyerPaymentService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedroute: ActivatedRoute,
    private buyerService: BuyerService,
    ngZone: NgZone
  ) {
    super(
      changeDetectorRef,
      buyerPaymentService,
      router,
      activatedroute,
      ngZone
    )
  }
}
