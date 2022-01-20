import { Component, Input, EventEmitter, Output, OnInit } from '@angular/core'
import { faCaretDown, faCaretUp } from '@fortawesome/free-solid-svg-icons'
import { ListPage, Order } from 'ordercloud-javascript-sdk'
import { isQuoteOrder } from '../../../services/orderType.helper'
import { HSOrder } from '@ordercloud/headstart-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { OrderListColumn, OrderViewContext } from 'src/app/models/order.types'

@Component({
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.scss'],
})
export class OCMOrderList implements OnInit {
  @Input() orders: ListPage<HSOrder>
  @Input() columns: OrderListColumn[]
  faCaretDown = faCaretDown
  faCaretUp = faCaretUp
  _userCurrency: string
  isQuoteOrder = isQuoteOrder
  // todo - should this be in some kinda service?
  @Input() sortBy: string
  @Output() updatedSort = new EventEmitter<string>()
  @Output() changedPage = new EventEmitter<number>()

  constructor(private context: ShopperContextService) {}

  ngOnInit(): void {
    this._userCurrency = this.context.currentUser.get().Currency
  }

  updateSort(selectedSortBy: string): void {
    let sortBy
    switch (this.sortBy) {
      case selectedSortBy:
        sortBy = `!${selectedSortBy}`
        break
      case `!${selectedSortBy}`:
        sortBy = selectedSortBy
        break
      default:
        sortBy = selectedSortBy
    }
    this.updatedSort.emit(sortBy)
  }

  changePage(page: number): void {
    this.changedPage.emit(page)
  }

  getOrderStatus(order: Order): string {
    // AwaitingApproval is the one status order xp doesn't account for. If order.status is AwaitingApproval, take that.
    if (order?.Status === 'AwaitingApproval') {
      return 'AwaitingApproval'
    } else if(order?.xp?.OrderType === 'Quote'){
      return order?.xp?.QuoteStatus
    } 
    else {
      return order?.xp?.SubmittedOrderStatus
    }
  }

  isFavorite(orderID: string): boolean {
    return this.context.currentUser.get().FavoriteOrderIDs.includes(orderID)
  }

  setIsFavorite(isFav: boolean, orderID: string): void {
    this.context.currentUser.setIsFavoriteOrder(isFav, orderID)
  }

  toOrderDetails(orderID: string): void {
    const isOrderToApprove =
    this.context.orderHistory.filters.getOrderViewContext() === OrderViewContext.Approve
    if (isOrderToApprove) {
      this.context.router.toOrderToAppoveDetails(orderID)
    } else {
      this.context.router.toMyOrderDetails(orderID)
    }
  }
}
