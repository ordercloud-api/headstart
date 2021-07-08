import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges,
  Inject,
} from '@angular/core'
import { RMAStatus } from '@app-seller/models/order.types'
import {
  HSLineItem,
  HSOrder,
  RMA,
  RMALineItem,
} from '@ordercloud/headstart-sdk'
import { LineItem, LineItemSpec, Order } from '@ordercloud/angular-sdk'
import { AppConfig } from '@app-seller/shared'
import { FormControl, FormGroup, Validators } from '@angular/forms'
import { getPrimaryLineItemImage } from '@app-seller/shared/services/assets/asset.helper'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { RMAService } from '@app-seller/rmas/rmas.service'
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap'
import { faExclamationCircle } from '@fortawesome/free-solid-svg-icons'

@Component({
  selector: 'rmas-summary-component',
  templateUrl: './rmas-summary.component.html',
  styleUrls: ['./rmas-summary.component.scss'],
})
export class RMASummaryComponent implements OnChanges {
  @Input()
  set selectedRMA(rma: RMA) {
    this.refreshRMAData(rma)
  }
  @Input() relatedOrder: HSOrder
  @Input() relatedLineItems: HSLineItem[]
  @Input() buyerOrderData: Order
  @Input() supplierOrderData: Order
  @Input() isSellerUser: boolean
  @Output()
  updateRMA = new EventEmitter<RMA>()

  _rma: RMA
  processingLineItems = false
  refundingLineItems = false
  lineItemsBeingProcessed: string[] = []
  lineItemsBeingDenied: string[] = []
  lineItemsReadyForRefund: HSLineItem[] = []
  rmaLineItemForm: FormGroup
  dataIsSaving: boolean
  areChanges = false
  faExclamationCircle = faExclamationCircle
  modalReference: NgbModalRef

  constructor(
    private rmaService: RMAService,
    private modalService: NgbModal,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (
      changes.selectedRMA?.currentValue !== changes.selectedRMA?.previousValue
    ) {
      this.refreshRMAData(changes.selectedRMA?.currentValue)
    }
  }

  setLineItemsBeingProcessed(): void {
    this.lineItemsBeingProcessed = []
    this._rma?.LineItems?.forEach((item) => {
      if (
        item?.Status !== RMAStatus.Requested &&
        item?.Status !== RMAStatus.Denied
      ) {
        this.lineItemsBeingProcessed.push(item?.ID)
      }
    })
  }

  setLineItemsBeingDenied(): void {
    this.lineItemsBeingDenied = []
    this._rma?.LineItems?.forEach((item) => {
      if (item?.Status === RMAStatus.Denied) {
        this.lineItemsBeingDenied.push(item?.ID)
      }
    })
  }

  getRMALineItem(lineItem: HSLineItem): RMALineItem {
    const rmaLineItem = this._rma?.LineItems?.find(
      (li) => li.ID === lineItem.ID
    )
    return rmaLineItem
  }

  getVariableTextSpecs = (li: LineItem): LineItemSpec[] =>
    li?.Specs?.filter((s) => s.OptionID === null)

  toggleProcessRMALineItems(): void {
    this.setRMALineItemForm()
    this.processingLineItems = !this.processingLineItems
    this.setLineItemsBeingProcessed()
    this.setLineItemsBeingDenied()
  }

  setRMALineItemForm(): void {
    this.rmaLineItemForm = new FormGroup({})
    this._rma.LineItems?.forEach((item) => {
      this.rmaLineItemForm.addControl(
        item.ID,
        new FormGroup({
          QuantityProcessed: new FormControl(this.getDefaultQuantity(item)),
          Status: new FormControl(this.getDefaultStatus(item)),
          Comment: new FormControl(item?.Comment, Validators.maxLength(300)),
          OverridePercentToRefund: new FormControl(
            item?.PercentToRefund !== null
          ),
          PercentToRefund: new FormControl(item?.PercentToRefund),
        })
      )
    })
  }

  getDefaultStatus(item: RMALineItem): string {
    const validStatuses = [
      RMAStatus.Approved.toString(),
      RMAStatus.Processing.toString(),
      RMAStatus.Denied.toString(),
      RMAStatus.PartialQtyApproved.toString(),
    ]
    let status = validStatuses.includes(item?.Status) ? item.Status : null
    status =
      item.Status == RMAStatus.PartialQtyApproved
        ? RMAStatus.Approved
        : item.Status

    return status
  }

  getDefaultQuantity(li: RMALineItem): number {
    return li?.Status === RMAStatus.Requested ? 0 : li?.QuantityProcessed
  }

  getRMAProcessingAction(): string {
    return this.processingLineItems ? 'Cancel Process' : 'Process'
  }

  async processRefund(): Promise<void> {
    try {
      this.dataIsSaving = true
      const processedRMA = await this.rmaService.processRefund(
        this._rma?.RMANumber
      )
      this.dataIsSaving = false
      this.modalReference.close()
      this.refreshRMAData(processedRMA)
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }

  updateRMAFromProcessingForm(event: RMA): void {
    this.refreshRMAData(event)
  }

  refreshRMAData(rma: RMA): void {
    this._rma = rma
    this.setRMALineItemForm()
    this.processingLineItems = false
    this.setLineItemsBeingProcessed()
    this.setLineItemsBeingDenied()
    this.setLineItemsReadyForRefund()
    this.updateRMA.emit(rma)
  }

  setLineItemsReadyForRefund(): void {
    this.lineItemsReadyForRefund = []
    this._rma?.LineItems?.forEach((li) => {
      if (
        li?.Status === RMAStatus.Approved ||
        (li?.Status === RMAStatus.PartialQtyApproved && li?.IsResolved)
      ) {
        const relatedLineItem = this.relatedLineItems?.find(
          (relatedLineItem) => relatedLineItem.ID === li.ID
        )
        this.lineItemsReadyForRefund.push(relatedLineItem)
      }
    })
    this.lineItemsReadyForRefund.sort((a, b) =>
      a.ID > b.ID ? 1 : b.ID > a.ID ? -1 : 0
    )
  }

  getImageUrl(lineItemID: string): string {
    return getPrimaryLineItemImage(lineItemID, this.relatedLineItems)
  }

  openRefundModal(content: any): void {
    this.modalReference = this.modalService.open(content, {
      ariaLabelledBy: 'confirm-modal',
    })
  }

  getRefundButtonVerbiage(): string {
    if (
      this._rma?.LineItems?.some((li) => li?.RefundableViaCreditCard === false)
    ) {
      return 'Complete RMA'
    } else {
      return 'Process Refund'
    }
  }
}
