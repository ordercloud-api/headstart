import { Pipe, PipeTransform } from '@angular/core'
import { getCurrencySymbol } from '@angular/common'

@Pipe({
  name: 'currencySymbol',
})
export class CurrencySymbolPipe implements PipeTransform {
  transform(currency: string): string {
    return getCurrencySymbol(currency || 'USD', 'narrow')
  }
}
