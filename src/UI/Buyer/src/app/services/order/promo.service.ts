import { Injectable } from '@angular/core'
import { Orders, OrderPromotion } from 'ordercloud-javascript-sdk'
import { Subject } from 'rxjs'
import { OrderStateService } from './order-state.service'
import { HSOrder, ListPage } from '@ordercloud/headstart-sdk'
import { TempSdk } from '../temp-sdk/temp-sdk.service'

@Injectable({
  providedIn: 'root',
})
export class PromoService {
  public onAdd = new Subject<OrderPromotion>() // need to make available as observable
  public onChange: (
    callback: (promos: ListPage<OrderPromotion>) => void
  ) => void
  private initializingOrder = false

  constructor(private state: OrderStateService, private tempsdk: TempSdk) {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
    this.onChange = this.state.onPromosChange.bind(this.state)
  }

  get(): ListPage<OrderPromotion> {
    return this.promos
  }

  async refresh(): Promise<void> {
    this.promos = await Orders.ListPromotions('Outgoing', this.order.ID)
  }

  public async removePromo(promoCode: string): Promise<void> {
    this.promos.Items = this.promos.Items.filter((p) => p.Code !== promoCode)
    try {
      await Orders.RemovePromotion('Outgoing', this.order.ID, promoCode)
    } finally {
      this.state.reset()
    }
  }

  public async applyPromo(promoCode: string): Promise<OrderPromotion> {
    try {
      const newPromo = await Orders.AddPromotion(
        'Outgoing',
        this.order?.ID,
        promoCode
      )
      this.onAdd.next(newPromo)
      return newPromo
    } finally {
      await this.state.reset()
    }
  }

  public async applyAutomaticPromos(): Promise<void> {
    try {
      await this.tempsdk.applyAutomaticPromotionsToOrder(this.order.ID)
    } finally {
      await this.state.reset()
    }
  }

  public async removeAllPromos(): Promise<void> {
    const reqs = this.promos?.Items?.map((promo) =>
      Orders.RemovePromotion('Outgoing', this.order.ID, promo.Code)
    )
    try {
      await Promise.all(reqs)
    } finally {
      void this.state.reset()
    }
  }

  private get order(): HSOrder {
    return this.state.order
  }

  private set order(value: HSOrder) {
    this.state.order = value
  }

  private get promos(): ListPage<OrderPromotion> {
    return this.state.orderPromos
  }

  private set promos(value: ListPage<OrderPromotion>) {
    this.state.orderPromos = value
  }
}
