import { Component, OnInit } from '@angular/core'
import {
  HSOrder,
  Meta,
  ListPage,
  HSLineItem,
  HSMeProduct,
} from '@ordercloud/headstart-sdk'
import { uniq } from 'lodash'
import { BuyerProduct, Me, OrderPromotion } from 'ordercloud-javascript-sdk'
import { HSLineItemWithBuyerProduct, LineItemWithProduct } from '../models/line-item.types'
import { CurrentOrderService } from '../services/order/order.service'
import { PromoService } from '../services/order/promo.service'


@Component({
  template: `
    <ocm-cart
      [order]="order"
      [lineItems]="lineItems"
      [invalidLineItems]="invalidLineItems"
      [orderPromos]="orderPromos"
    ></ocm-cart>
  `,
})
export class CartWrapperComponent implements OnInit {
  order: HSOrder
  lineItems: ListPage<LineItemWithProduct>
  invalidLineItems: HSLineItem[]
  orderPromos: ListPage<OrderPromotion>
  productCache: HSMeProduct[] = [] // TODO - move to cart service?

  constructor(
    private currentOrder: CurrentOrderService,
    private currentPromos: PromoService
  ) {}

  ngOnInit(): void {
    this.currentOrder.onChange(this.setOrder)
    this.currentPromos.onChange(this.setOrderPromos)
    this.currentOrder.cart.onChange(this.setLineItems)
  }

  setOrder = (order: HSOrder): void => {
    this.order = order
  }

  setLineItems = async (
    items: ListPage<LineItemWithProduct>
  ): Promise<void> => {
    this.invalidLineItems = []
    // TODO - this requests all the products on navigation to the cart.
    // Fewer requests could be acomplished by moving this logic to the cart service so it runs only once.
    const availableLineItems = await this.checkForProductAvailability(items)
    this.updateProductCache(availableLineItems.map((li) => li.buyerProduct))
    this.lineItems = this.mapToLineItemsWithProduct(
      availableLineItems.map(a => a.lineItem),
      items.Meta
    )
  }

  async checkForProductAvailability(
    items: ListPage<HSLineItem>
  ): Promise<HSLineItemWithBuyerProduct[]> {
    const activeLineItems: HSLineItemWithBuyerProduct[] = []
    const invalidLineItems: HSLineItem[] = []
    const uniqIDs = uniq(items.Items.map(i => i.ProductID))
    const requests = uniqIDs.map(async id => 
      Me.GetProduct(id)
      .catch(e => undefined))

    const responses = await Promise.all(requests);
    items.Items.forEach(li => {
      const matchingProd: BuyerProduct = responses.find(p => p && p.ID === li.ProductID);
      if(matchingProd) {
        activeLineItems.push({
          lineItem: li,
          buyerProduct: matchingProd
        })
      } else {
        invalidLineItems.push(li)
      }
    })
    this.invalidLineItems = invalidLineItems
    return activeLineItems
  }

  setOrderPromos = (promos: ListPage<OrderPromotion>): void => {
    this.orderPromos = promos
  }

  updateProductCache(products: BuyerProduct[]): void {
    const cachedIDs = this.productCache.map((p) => p.ID)
    const toAdd = products.filter((p) => !cachedIDs.includes(p.ID))
    this.productCache = [
      ...this.productCache,
      ...toAdd,
    ]
  }

  mapToLineItemsWithProduct(
    items: HSLineItem[],
    meta: Meta
  ): ListPage<LineItemWithProduct> {
    const Items = items.map((li: LineItemWithProduct) => {
      const product = this.getCachedProduct(li.ProductID)
      li.Product = product
      return li
    })
    return { Items, Meta: meta }
  }

  async requestProducts(ids: string[]): Promise<HSMeProduct[]> {
    return await Promise.all(ids.map((id) => Me.GetProduct(id)))
  }

  getCachedProduct(id: string): HSMeProduct {
    return this.productCache.find((product) => product.ID === id)
  }
}
