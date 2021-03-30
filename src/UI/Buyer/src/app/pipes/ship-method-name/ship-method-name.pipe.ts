import { Pipe, PipeTransform } from '@angular/core'

@Pipe({
  name: 'shipMethodNameMapper',
})
export class ShipMethodNameMapperPipe implements PipeTransform {
  transform(methodName: string): string {
    if (!methodName) return ''

    const methodNameMapper: { [key: string]: string } = {
      ['Priority']: 'Prority',
      ['First']: 'Priority First',
      ['ParcelSelect']: 'Economical Ground',
      ['Express']: 'Priority Express',
      ['FIRST_OVERNIGHT']: 'First Overnight',
      ['PRIORITY_OVERNIGHT']: 'Priority Overnight',
      ['STANDARD_OVERNIGHT']: 'Standard Overnight',
      ['FEDEX_2_DAY']: '2 Day',
      ['FEDEX_GROUND']: 'Ground',
      ['FREE_SHIPPING']: '',
    }
    let mappedMethodName: string = methodName.includes('No shipping rates')
      ? methodNameMapper['FREE_SHIPPING']
      : methodNameMapper[methodName]

    if (!methodName.includes('No shipping rates')) {
      mappedMethodName = `${mappedMethodName} -`
    }

    if (mappedMethodName !== null || mappedMethodName !== undefined) {
      return mappedMethodName
    } else {
      return methodName
    }
  }
}
