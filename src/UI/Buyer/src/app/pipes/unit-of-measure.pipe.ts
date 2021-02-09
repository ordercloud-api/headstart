import { Pipe, PipeTransform } from '@angular/core'
import { HSProduct } from '@ordercloud/headstart-sdk'

@Pipe({
  name: 'UofM',
})
export class UnitOfMeasurePipe implements PipeTransform {
  transform(product: HSProduct): string {
    const uofm = product?.xp?.UnitOfMeasure
    if (uofm?.Qty == null || uofm?.Unit == null) {
      return ''
    }
    return `${uofm.Qty} / ${uofm.Unit}`
  }
}
