import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core'
import { HSLineItem } from '@ordercloud/headstart-sdk'
import { NgxSpinnerService } from 'ngx-spinner'
import { ToastrService } from 'ngx-toastr'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { OCMLineitemTable } from '../lineitem-table.component'


@Component({
    templateUrl: './kit-lineitem-table.component.html',
    styleUrls: ['../lineitem-table.component.scss'],
})

export class OCMKitLineitemTable extends OCMLineitemTable {
  showKitDetails = true
  constructor(
      context: ShopperContextService,
      spinner: NgxSpinnerService,
      toastrService: ToastrService
  ) {
      super(context, spinner, toastrService)
  }

  toggleKitDetails(): void {
    this.showKitDetails = !this.showKitDetails
  }

  async removeKit(kit: HSLineItem[]): Promise<void> {
    await this.context.order.cart.removeMany(kit)
  }
}