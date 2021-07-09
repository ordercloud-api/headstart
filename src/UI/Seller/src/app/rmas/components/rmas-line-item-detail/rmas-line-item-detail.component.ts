import { Component, Inject, Input } from '@angular/core'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { getPrimaryLineItemImage } from '@app-seller/shared/services/assets/asset.helper'
import { AppConfig, RegexService } from '@app-seller/shared'
import { LineItem, LineItemSpec } from '@ordercloud/angular-sdk'
import { HSLineItem, RMA, RMALineItem } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'rmas-line-item-detail-component',
  templateUrl: './rmas-line-item-detail.component.html',
  styleUrls: ['./rmas-line-item-detail.component.scss'],
})
export class RMALineItemDetailComponent {
  @Input() processingLineItems: boolean
  @Input() relatedLineItems: HSLineItem[]
  @Input() rma: RMA
  constructor(
    private regexService: RegexService,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}
  getImageUrl(lineItemID: string): string {
    return getPrimaryLineItemImage(lineItemID, this.relatedLineItems)
  }
  getVariableTextSpecs = (li: LineItem): LineItemSpec[] =>
    li?.Specs?.filter((s) => s.OptionID === null)
  getRMALineItem(lineItem: HSLineItem): RMALineItem {
    const rmaLineItem = this.rma?.LineItems?.find((li) => li.ID === lineItem.ID)
    return rmaLineItem
  }
  getRefundStatus(rmaLineItem: RMALineItem): string {
    return rmaLineItem?.IsRefunded ? 'Y' : 'N'
  }
  getValueSplitByCapitalLetter(value: string): string {
    return this.regexService.getStatusSplitByCapitalLetter(value)
  }
}
