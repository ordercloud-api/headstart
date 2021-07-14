import { Component, Inject, Input } from '@angular/core'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { getPrimaryLineItemImage } from '@app-seller/shared/services/assets/asset.helper'
import { AppConfig } from '@app-seller/shared'
import { LineItem, LineItemSpec } from '@ordercloud/angular-sdk'
import { HSLineItem, RMA, RMALineItem } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'rmas-modal-content-component',
  templateUrl: './rmas-modal-content.component.html',
  styleUrls: ['./rmas-modal-content.component.scss'],
})
export class RMAModalContent {
  @Input() lineItemsForModal: HSLineItem[]
  @Input() relatedLineItems: HSLineItem[]
  @Input() rma: RMA
  @Input() isDenial: boolean

  constructor(@Inject(applicationConfiguration) private appConfig: AppConfig) {}

  getImageUrl(lineItemID: string): string {
    return getPrimaryLineItemImage(lineItemID, this.relatedLineItems)
  }

  getVariableTextSpecs = (li: LineItem): LineItemSpec[] =>
    li?.Specs?.filter((s) => s.OptionID === null)

  getRMALineItem(lineItem: HSLineItem): RMALineItem {
    const rmaLineItem = this.rma?.LineItems?.find((li) => li.ID === lineItem.ID)
    return rmaLineItem
  }

  getQuantityToDisplay(li: HSLineItem): number {
    const rmaLineItem = this.getRMALineItem(li)
    return this.isDenial
      ? rmaLineItem.QuantityRequested
      : rmaLineItem.QuantityProcessed
  }
}
