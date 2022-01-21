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
import { NgbModal } from '@ng-bootstrap/ng-bootstrap'

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
    this.supplierMail = "mailto:" + this.order.xp.QuoteSellerContactEmail
  }
  order: HSOrder
  lineItems: HSLineItem[] = []
  promotions: OrderPromotion[] = []
  payments: Payment[] = []
  approvals: OrderApproval[] = []
  isQuoteOrder = isQuoteOrder
  buyerLocation: HSAddressBuyer
  _userCurrency: string
  supplierMail: string

  constructor(
    private context: ShopperContextService,
    private modalService: NgbModal) { }

  ngOnInit(): void {
    this._userCurrency = this.context.currentUser.get().Currency
  }

  open(content: HTMLTemplateElement): void {
    this.modalService.open(content, { ariaLabelledBy: 'confirm-modal' })
  }

  async getBuyerLocation(addressID: string): Promise<void> {
    if (!this.isQuoteOrder(this.order) && addressID !== null) {
      const buyerLocation = await this.context.addresses.get(addressID)
      this.buyerLocation = buyerLocation
    } else this.buyerLocation = null
  }

  async acceptQuote() :Promise<void>{
    const lineItems = this.lineItems.map(async (li) => {
      await this.context.order.cart.add({
        ProductID: li.ProductID,
        UnitPrice: li.UnitPrice,
        Product: li.Product,
        Quantity: li.Quantity,
        Specs: li.Specs,
        xp: li.xp,
      })
    })
    Promise.all(lineItems).finally(() => (
      this.context.order.delete(this.order.ID)
    ))
    this.context.router.toCart()
  }
}
