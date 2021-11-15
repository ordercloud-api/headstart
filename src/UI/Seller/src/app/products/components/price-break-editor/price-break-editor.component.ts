import { Component, Input, Output, EventEmitter } from '@angular/core'
import {
  PriceBreak,
  PriceSchedule,
  OcBuyerService,
} from '@ordercloud/angular-sdk'
import { ToastrService } from 'ngx-toastr'
import {
  faCog,
  faExclamationCircle,
  faTrash,
} from '@fortawesome/free-solid-svg-icons'
import { BuyerTempService } from '@app-seller/shared/services/middleware-api/buyer-temp.service'
import { SupportedRates } from '@app-seller/models/currency-geography.types'

@Component({
  selector: 'price-break-editor',
  templateUrl: './price-break-editor.component.html',
  styleUrls: ['./price-break-editor.component.scss'],
})
export class PriceBreakEditor {
  @Input()
  readonly = false
  @Input()
  currency: SupportedRates
  @Input()
  isRequired: boolean
  @Input()
  set priceSchedule(value: PriceSchedule) {
    if (value) {
      this.isAddingPriceBreak = false
      this.isValidNewBreak = false
      this.priceScheduleEditable = JSON.parse(JSON.stringify(value))
      this.newPriceBreak = this.getEmptyBreak()
    }
  }
  @Input()
  set variantCount(n: number) {
    this._variantCount = n
  }

  @Input()
  set specCount(n: number) {
    this._specCount = n
  }

  @Output()
  priceScheduleUpdated = new EventEmitter<PriceSchedule>()
  priceScheduleEditable: PriceSchedule

  faCog = faCog
  faTrash = faTrash
  faExclamationCircle = faExclamationCircle
  _specCount: number
  _variantCount: number

  isAddingPriceBreak = false
  newPriceBreak
  isValidNewBreak = false
  editPriceBreaks = false

  constructor(
    private toasterService: ToastrService,
    private ocBuyerService: OcBuyerService,
    private buyerTempService: BuyerTempService
  ) {}

  getEmptyBreak(): any {
    const ps = this.priceScheduleEditable
    if (ps.PriceBreaks.length === 0)
      this.priceScheduleEditable.PriceBreaks = [{ Quantity: 1, Price: null }]
    const nextQuantity =
      ps.PriceBreaks[this.priceScheduleEditable.PriceBreaks.length - 1]
        .Quantity + 1
    return {
      Price: 0,
      Quantity: nextQuantity,
    }
  }

  emitUpdatedSchedule(): void {
    this.priceScheduleUpdated.emit(this.priceScheduleEditable)
  }

  updateNewBreak(event: any, field: string): void {
    this.newPriceBreak[field] =
      field === 'Quantity'
        ? parseInt(event.target.value, 10)
        : event.target.value
    const areErrors = this.handlePriceBreakErrors(
      this.priceScheduleEditable.PriceBreaks
    )
    this.isValidNewBreak =
      !areErrors && !!this.newPriceBreak.Price && !!this.newPriceBreak.Quantity
  }

  getPriceBreakRange(index: number): string {
    const priceBreaks = this.priceScheduleEditable.PriceBreaks
    if (!priceBreaks.length) return ''
    const indexOfNextPriceBreak = index + 1
    let rangeString
    if (
      priceBreaks[index].Quantity ===
      priceBreaks[indexOfNextPriceBreak]?.Quantity - 1
    ) {
      rangeString = priceBreaks[index].Quantity
    } else if (indexOfNextPriceBreak < priceBreaks.length) {
      rangeString = `${priceBreaks[index].Quantity} - ${
        priceBreaks[indexOfNextPriceBreak].Quantity - 1
      }`
    } else {
      rangeString = `${priceBreaks[index].Quantity}+`
    }
    return rangeString
  }

  deletePriceBreak(priceBreak: PriceBreak): void {
    const priceBreaks = this.priceScheduleEditable.PriceBreaks
    const i = priceBreaks.indexOf(priceBreak)
    priceBreaks.splice(i, 1)
    this.priceScheduleEditable.PriceBreaks = priceBreaks
    this.emitUpdatedSchedule()
  }

  addPriceBreak(): void {
    const priceBreaks = this.priceScheduleEditable.PriceBreaks
    if (this.handlePriceBreakErrors(priceBreaks)) return
    const updatedPriceBreaks = [...priceBreaks, this.newPriceBreak]
    updatedPriceBreaks.sort((a, b) => (a.Quantity > b.Quantity ? 1 : -1))
    this.priceScheduleEditable.PriceBreaks = updatedPriceBreaks
    this.isAddingPriceBreak = false
    this.newPriceBreak = this.getEmptyBreak()
    this.emitUpdatedSchedule()
  }

  updateExistingBreakPrice(event: any, index: number): void {
    this.priceScheduleEditable.PriceBreaks[index].Price = event.target.value
    this.emitUpdatedSchedule()
  }

  handlePriceBreakErrors(priceBreaks: PriceBreak[]): boolean {
    let hasError = false
    if (
      priceBreaks.some((pb) => pb.Price === Number(this.newPriceBreak.Price))
    ) {
      this.toasterService.error('A Price Break with that price already exists')
      hasError = true
    }
    if (
      priceBreaks.some(
        (pb) => pb.Quantity === Number(this.newPriceBreak.Quantity)
      )
    ) {
      this.toasterService.error(
        'A Price Break with that quantity already exists'
      )
      hasError = true
    }
    if (this.newPriceBreak.Quantity < 2) {
      this.toasterService.error('Please enter a quantity of two or more')
      hasError = true
    }
    return hasError
  }

  handleUpdateUseCumulativeQuantity(event: any): void {
    this.priceScheduleEditable.UseCumulativeQuantity = event.target.value
    this.emitUpdatedSchedule()
  }

  handleUpdatePriceBreaks(event: any, field: string): void {
    this.newPriceBreak[field] = event.target.value
  }

  toggleEditPriceBreaks(): void {
    this.editPriceBreaks = !this.editPriceBreaks
  }
}
