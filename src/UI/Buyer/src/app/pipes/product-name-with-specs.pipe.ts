import { Pipe, PipeTransform } from '@angular/core'
import { HSLineItem } from '@ordercloud/headstart-sdk'

@Pipe({
  name: 'productNameWithSpecs',
})
export class ProductNameWithSpecsPipe implements PipeTransform {
  transform(lineItem: HSLineItem): any {
    // TODO - this check is needed because of the bare lineItem object that gets added right away.
    // If the activeProduct state was saved in some cache, this wouldn't be needed.
    if (!lineItem.Product) return ''
    const productName = lineItem.Product.Name
    if (lineItem.Specs) {
      const specs = lineItem.Specs
      if (specs.length === 0) return productName
      const list = specs.map((spec) => spec.Value).join(', ')
      return `${productName} (${list})`
    }
  }
}
