import { Component, EventEmitter, Inject, Input, Output } from '@angular/core'
import { FormGroup, Validators } from '@angular/forms'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { getPrimaryLineItemImage } from '@app-seller/shared/services/assets/asset.helper'
import { RMAService } from '@app-seller/rmas/rmas.service'
import { AppConfig, RegexService, RMAStatus } from '@app-seller/shared'
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap'
import { HSLineItem, RMA, RMALineItem } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'rmas-form-processing-component',
  templateUrl: './rmas-form-processing.component.html',
  styleUrls: ['./rmas-form-processing.component.scss'],
})
export class RMAFormProcessingComponent {
  @Input() rma: RMA
  @Input() rmaLineItemForm: FormGroup
  @Input() relatedLineItems: HSLineItem[]
  @Input() lineItemsBeingProcessed: string[]
  @Input() lineItemsBeingDenied: string[]
  @Output() updateRMA = new EventEmitter<RMA>()
  lineItemsForModal: HSLineItem[] = []
  dataIsSaving: boolean
  modalReference: NgbModalRef

  constructor(
    private rmaService: RMAService,
    private regexService: RegexService,
    private modalService: NgbModal,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  getImageUrl(lineItemID: string): string {
    return getPrimaryLineItemImage(lineItemID, this.relatedLineItems)
  }

  getRMALineItem(lineItem: HSLineItem): RMALineItem {
    const rmaLineItem = this.rma?.LineItems?.find((li) => li.ID === lineItem.ID)
    return rmaLineItem
  }

  getValueSplitByCapitalLetter(value: string): string {
    return this.regexService.getStatusSplitByCapitalLetter(value)
  }

  isApprovingLineItem(lineItem: HSLineItem): boolean {
    return this.lineItemsBeingProcessed?.some((value) => value === lineItem?.ID)
  }

  isDenyingLineItem(lineItem: HSLineItem): boolean {
    return this.lineItemsBeingDenied?.some((value) => value === lineItem?.ID)
  }

  lineItemNotComplete(lineItem: HSLineItem): boolean {
    const rmaLineItem = this.getRMALineItem(lineItem)
    return (
      rmaLineItem.Status != RMAStatus.Denied &&
      rmaLineItem.Status != RMAStatus.Complete &&
      rmaLineItem.Status != RMAStatus.PartialQtyComplete
    )
  }

  processLineItem(lineItem: HSLineItem): void {
    this.lineItemsBeingProcessed.push(lineItem?.ID)
    const lineItemFormControl = this.rmaLineItemForm?.get(lineItem.ID)[
      'controls'
    ]
    lineItemFormControl?.QuantityProcessed?.setValidators(Validators.min(1))
    lineItemFormControl?.QuantityProcessed?.updateValueAndValidity()
    lineItemFormControl?.Status?.setValue(null)
    lineItemFormControl?.Status?.setValidators(Validators.required)
    lineItemFormControl?.Status?.updateValueAndValidity()
  }

  denyLineItem(lineItem: HSLineItem): void {
    this.lineItemsBeingDenied.push(lineItem?.ID)
    this.lineItemsForModal.push(lineItem)
    this.lineItemsForModal.sort((a, b) =>
      a.ID > b.ID ? 1 : b.ID > a.ID ? -1 : 0
    )
    const lineItemFormControl = this.rmaLineItemForm?.get(lineItem.ID)[
      'controls'
    ]
    lineItemFormControl?.QuantityProcessed?.setValue(
      this.getRMALineItem(lineItem)?.QuantityRequested
    )
    lineItemFormControl?.Status?.setValue(RMAStatus.Denied)
  }

  editProcessingType(lineItem: HSLineItem): void {
    const lineItemFormControl = this.rmaLineItemForm?.get(lineItem.ID)[
      'controls'
    ]
    lineItemFormControl?.QuantityProcessed?.setValue(0)
    lineItemFormControl?.QuantityProcessed?.setValidators(null)
    lineItemFormControl?.QuantityProcessed?.updateValueAndValidity()
    lineItemFormControl?.Status?.setValue(null)
    lineItemFormControl?.Status?.setValidators(null)
    lineItemFormControl?.Status?.updateValueAndValidity()
    lineItemFormControl?.Comment?.reset()
    const processingLineItemIndex = this.lineItemsBeingProcessed?.indexOf(
      lineItem?.ID
    )
    const denyingLineItemIndex = this.lineItemsBeingDenied?.indexOf(
      lineItem?.ID
    )
    const lineItemForModalIndex = this.lineItemsForModal?.indexOf(lineItem)
    if (processingLineItemIndex > -1) {
      this.lineItemsBeingProcessed?.splice(processingLineItemIndex, 1)
    }
    if (denyingLineItemIndex > -1) {
      this.lineItemsBeingDenied?.splice(denyingLineItemIndex, 1)
    }
    if (lineItemForModalIndex > -1) {
      this.lineItemsForModal?.splice(lineItemForModalIndex, 1)
    }
  }

  getQuantityDropdown(lineItem: HSLineItem): number[] {
    const relatedLineItem = this.getRMALineItem(lineItem)
    const quantityList: number[] = []
    for (let i = 1; i <= relatedLineItem?.QuantityRequested; i++) {
      quantityList.push(i)
    }
    return quantityList
  }

  setPercentToOverride(event: boolean, lineItem: HSLineItem): void {
    const lineItemFormControl = this.rmaLineItemForm?.get(lineItem.ID)[
      'controls'
    ]
    lineItemFormControl?.PercentToRefund?.setValue(event ? 100 : null)
    lineItemFormControl?.PercentToRefund?.updateValueAndValidity()
  }

  statusNotAvailableToProcess(lineItem: HSLineItem): boolean {
    const status = this.rmaLineItemForm?.get(lineItem?.ID)['controls']?.Status
      ?.value
    return status === null || status === RMAStatus.Requested
  }

  async handleProcessRMA(): Promise<void> {
    try {
      this.dataIsSaving = true
      const rma = this.buildRMA()
      const processedRMA = await this.rmaService.updateResource(
        rma.RMANumber,
        rma
      )
      this.updateRMA.emit(processedRMA)
      if (this.lineItemsForModal.length) {
        this.modalReference.close()
      }
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }

  buildRMA(): RMA {
    const lineItems = this.buildRMALineItems()
    const rma = { ...this.rma, LineItems: lineItems }
    return rma
  }

  buildRMALineItems(): RMALineItem[] {
    const rmaLineItems: RMALineItem[] = []
    this.rma?.LineItems.forEach((li: RMALineItem) => {
      const relatedFormControl = this.rmaLineItemForm?.get(li?.ID)['controls']
      rmaLineItems.push({
        ...li,
        QuantityProcessed: relatedFormControl?.QuantityProcessed?.value,
        Status:
          relatedFormControl?.Status?.value === null
            ? RMAStatus.Requested
            : relatedFormControl?.Status?.value,
        PercentToRefund: relatedFormControl?.PercentToRefund?.value,
        Comment: relatedFormControl?.Comment?.value,
      })
    })
    return rmaLineItems
  }

  openConfirm(content: any): void {
    this.modalReference = this.modalService.open(content, {
      ariaLabelledBy: 'confirm-modal',
    })
  }

  async checkForDeniedRMALineItems(content: any): Promise<void> {
    if (this.lineItemsForModal.length) {
      this.openConfirm(content)
    } else {
      await this.handleProcessRMA()
    }
  }
}
