import { Component, Input, OnInit } from '@angular/core'
import {
  OrderDetails,
  HSOrder,
  HSLineItem,
  HSAddressBuyer,
} from '@ordercloud/headstart-sdk'
import {
  OrderApproval,
  Payment,
  OrderPromotion,
} from 'ordercloud-javascript-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { isQuoteOrder } from '../../../services/orderType.helper'

@Component({
  templateUrl: './order-historical.component.html',
  styleUrls: ['./order-historical.component.scss'],
})
export class OCMOrderHistorical implements OnInit {
  @Input() isOrderToApprove = false
  @Input() set orderDetails(value: OrderDetails) {
    this.order = value.Order
    this.lineItems = value.LineItems
    this.promotions = value.Promotions
    this.payments = value.Payments as any
    this.approvals = value.Approvals.filter((a) => a.Approver) as any
    this.getBuyerLocation(this.order.BillingAddressID)
  }
  order: HSOrder
  lineItems: HSLineItem[] = []
  promotions: OrderPromotion[] = []
  payments: Payment[] = []
  approvals: OrderApproval[] = []
  isQuoteOrder = isQuoteOrder
  buyerLocation: HSAddressBuyer
  _userCurrency: string

  constructor(private context: ShopperContextService) {}

  ngOnInit(): void {
    this._userCurrency = this.context.currentUser.get().Currency
  }

  async getBuyerLocation(addressID: string): Promise<void> {
    if (!this.isQuoteOrder(this.order)) {
      const buyerLocation = await this.context.addresses.get(addressID)
      this.buyerLocation = buyerLocation
    } else this.buyerLocation = null
  }
}
