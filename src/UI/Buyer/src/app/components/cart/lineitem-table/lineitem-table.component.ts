import { Component } from '@angular/core'
import { groupBy as _groupBy } from 'lodash'
import { NgxSpinnerService } from 'ngx-spinner'
import { Address } from 'ordercloud-javascript-sdk'
import { CheckoutService } from 'src/app/services/order/checkout.service'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { OCMParentTableComponent } from './parent-table-component'

@Component({
  templateUrl: './lineitem-table.component.html',
  styleUrls: ['./lineitem-table.component.scss'],
})
export class OCMLineitemTable extends OCMParentTableComponent {
  constructor(
    context: ShopperContextService,
    spinner: NgxSpinnerService,
    checkoutService: CheckoutService
  ) {
    super(context, spinner, checkoutService);
  }
}
