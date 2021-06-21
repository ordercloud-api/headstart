import { Injectable } from '@angular/core'
import { Payment, Payments, Me } from 'ordercloud-javascript-sdk'
import { ListPage } from '@ordercloud/headstart-sdk'

@Injectable({
  providedIn: 'root',
})
export class PaymentHelperService {
  constructor() { }

  async ListPaymentsOnOrder(
    orderID: string,
    includeDetails = true
  ): Promise<ListPage<Payment>> {
    const payments = await Payments.List('Outgoing', orderID)
    const withDetails = payments.Items.map((payment) =>
      this.setPaymentDetails(payment, includeDetails)
    )
    const Items = await Promise.all(withDetails)
    return { Items, Meta: payments.Meta }
  }

  private async setPaymentDetails(
    payment: Payment,
    includeDetails: boolean
  ): Promise<Payment> {
    if (includeDetails) {
      const details = await this.getPaymentDetails(payment);
      (payment as any).Details = details
      return payment
    } else {
      (payment as any).Details = {}
      return payment
    }
  }

  private async getPaymentDetails(payment: Payment): Promise<any> {
    switch (payment.Type) {
      case 'CreditCard':
        return this.safeCall(Me.GetCreditCard(payment.CreditCardID))
      case 'SpendingAccount':
        return this.safeCall(Me.GetSpendingAccount(payment.SpendingAccountID))
    }
  }

  private async safeCall(funct: Promise<any>): Promise<any> {
    try {
      return await funct
    } catch {
      return Promise.resolve({})
    }
  }
}
