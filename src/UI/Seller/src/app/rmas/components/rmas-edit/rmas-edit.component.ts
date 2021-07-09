import { Component, Input, OnInit } from '@angular/core'
import {
  HeadStartSDK,
  HSLineItem,
  HSOrder,
  RMA,
} from '@ordercloud/headstart-sdk'
import {
  MeUser,
  OcLineItemService,
  OcOrderService,
  Order,
} from '@ordercloud/angular-sdk'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { SELLER } from '@app-seller/shared'

@Component({
  selector: 'app-rmas-edit',
  templateUrl: './rmas-edit.component.html',
  styleUrls: ['./rmas-edit.component.scss'],
})
export class RMAEditComponent implements OnInit {
  @Input()
  set selectedRMA(rma: RMA) {
    this._rma = rma
    void this.handleSelectedRMAChange(rma)
  }

  currentUser: MeUser
  buyerOrderData: Order = {}
  supplierOrderData: Order = {}
  _relatedOrder: HSOrder
  _relatedLineItems: HSLineItem[] = []
  _rma: RMA
  isSellerUser: boolean

  constructor(
    private currentUserService: CurrentUserService,
    private ocOrderService: OcOrderService,
    private ocLineItemService: OcLineItemService,
    private appAuthService: AppAuthService
  ) {
    this.isSellerUser = this.appAuthService.getOrdercloudUserType() === SELLER
  }

  async ngOnInit(): Promise<void> {
    this.currentUser = await this.currentUserService.getUser()
  }

  async handleSelectedRMAChange(rma: RMA): Promise<void> {
    this._relatedLineItems = []
    await this.getCurrentUser()
    if (rma?.SourceOrderID) {
      this._relatedOrder = await this.getRelatedOrder(rma)
    }
    if (rma?.SourceOrderID && rma?.LineItems?.length) {
      this._relatedLineItems = await this.getRelatedLineItems(rma)
    }
  }

  async getCurrentUser(): Promise<void> {
    this.currentUser = await this.currentUserService.getUser()
  }

  async getRelatedOrder(rma: RMA): Promise<HSOrder> {
    let relatedOrderID = rma?.SourceOrderID

    if (this.currentUser?.Supplier?.ID) {
      relatedOrderID += `-${this.currentUser.Supplier.ID}`
    }

    if (!this.isSellerUser) {
      const fullOrderData = await HeadStartSDK.Suppliers.GetSupplierOrder(
        relatedOrderID
      )
      this.buyerOrderData = fullOrderData.BuyerOrder.Order
      this.supplierOrderData = fullOrderData.SupplierOrder.Order
      return this.supplierOrderData
    } else {
      this.buyerOrderData = await this.ocOrderService
        .Get('Incoming', relatedOrderID)
        .toPromise()
      return this.buyerOrderData
    }
  }

  async getRelatedLineItems(rma: RMA): Promise<HSLineItem[]> {
    const matchingLineItems: HSLineItem[] = []
    for (const li of rma.LineItems) {
      const ocLineItem = await this.ocLineItemService
        .Get('Incoming', this._relatedOrder.ID, li.ID)
        .toPromise()
      matchingLineItems.push(ocLineItem)
    }
    return matchingLineItems
  }

  updateRMA(event: RMA): void {
    this._rma = event
  }
}
