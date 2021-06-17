import { Pipe, PipeTransform } from '@angular/core'

@Pipe({
  name: 'paymentMethodDisplay',
})
export class PaymentMethodDisplayPipe implements PipeTransform {
  transform(method: string): string {
    if (!method) {
      return null
    }
    const PaymentMethodMap = {
      ['SpendingAccount']: 'Spending Account',
      ['CreditCard']: 'Credit Card',
    }
    return PaymentMethodMap[method]
  }
}
