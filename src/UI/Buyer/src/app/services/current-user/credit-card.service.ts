import { Injectable } from '@angular/core'
import { Me, ListPage } from 'ordercloud-javascript-sdk'
import { OrderCloudIntegrationsCreditCardToken } from '@ordercloud/headstart-sdk'
import { HSBuyerCreditCard } from 'src/app/models/credit-card.types'

@Injectable({
  providedIn: 'root',
})
export class CreditCardService {
  constructor() {}

  async Save(
    card: OrderCloudIntegrationsCreditCardToken
  ): Promise<HSBuyerCreditCard> {
    const cardType = this.getCardType(card.AccountNumber)
    const partialAcctNumber = this.getPartialAccountNumber(card.AccountNumber)
    return await Me.CreateCreditCard({
      Token: card.AccountNumber,
      CardType: cardType,
      PartialAccountNumber: partialAcctNumber,
      CardholderName: card.CardholderName,
      ExpirationDate: this.getIsoDate(card.ExpirationDate),
      xp: {
        CCBillingAddress: card.CCBillingAddress,
      },
    })
  }

  async Delete(cardID: string): Promise<void> {
    return await Me.DeleteCreditCard(cardID)
  }

  async List(isAnon?: boolean): Promise<ListPage<HSBuyerCreditCard>> {
    if(isAnon && isAnon === true) {
      var res: ListPage<HSBuyerCreditCard> = {
        Items: []
      }
      return Promise.resolve(res)
    } else {
      return await Me.ListCreditCards({ pageSize: 100 })
    }
  }

  private getPartialAccountNumber(token: string): string {
    // https://developer.cardconnect.com/guides/cardsecure
    return token.slice(-4)
  }

  private getCardType(token: string): string {
    if (!token) return null

    // https://developer.cardconnect.com/guides/cardsecure
    const secondChar = token.charAt(1)
    if (secondChar === '3') {
      return 'Amex'
    } else if (secondChar === '4') {
      return 'Visa'
    } else if (secondChar === '5') {
      return 'Mastercard'
    } else {
      return 'Discover'
    }
  }

  private getIsoDate(mmyy: string): string {
    const month = mmyy.slice(0, 2)
    const year = mmyy.slice(-2)
    const date = new Date()
    date.setMonth(parseInt(month, 10) - 1) // subtract 1, month is zero-based in javascript
    date.setFullYear(parseInt(`20${year}`, 10))
    const dateString = date.toISOString()
    return dateString
  }
}
