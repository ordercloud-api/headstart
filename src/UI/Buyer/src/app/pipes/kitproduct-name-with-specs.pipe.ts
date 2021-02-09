import { Pipe, PipeTransform } from '@angular/core'
import { HSProduct } from '@ordercloud/headstart-sdk'
import { Spec } from 'ordercloud-javascript-sdk'
import { flatten } from 'lodash'

@Pipe({
  name: 'kitproductNameWithSpecs',
})
export class KitProductNameWithSpecsPipe implements PipeTransform {
  transform(product: HSProduct, specs: Spec[]): string {
    if (!product) return ''
    const productName = product.Name
    if (!specs || specs.length === 0) return productName
    const options = flatten(specs.map((spec) => spec.Options || []))
    const list = options.map((o) => o.Value).join(', ')
    return `${productName} (${list})`
  }
}
