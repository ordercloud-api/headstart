import { Injectable } from '@angular/core'
import { BehaviorSubject } from 'rxjs'
import { ListPage, HeadStartSDK } from '@ordercloud/headstart-sdk'
import { CurrentUserService } from '../current-user/current-user.service'
import { HeadstartExchangeRates } from 'src/app/models/currency.types'


@Injectable({
  providedIn: 'root',
})
export class ExchangeRatesService {
  private ratesSubject: BehaviorSubject<
    ListPage<HeadstartExchangeRates>
  > = new BehaviorSubject<ListPage<HeadstartExchangeRates>>(null)

  constructor(private currentUser: CurrentUserService) {}

  Get(): ListPage<HeadstartExchangeRates> {
    return this.exchangeRates
  }

  async reset(): Promise<void> {
    const me = this.currentUser.get()
    this.exchangeRates = (await HeadStartSDK.ExchangeRates.Get(me.Currency))
  }

  private get exchangeRates(): ListPage<HeadstartExchangeRates> {
    return this.ratesSubject.value
  }

  private set exchangeRates(value: ListPage<HeadstartExchangeRates>) {
    this.ratesSubject.next(value)
  }
}
