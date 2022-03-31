import { Component, Input, Output, EventEmitter } from '@angular/core'
import { PriceBreak, PriceSchedule } from 'ordercloud-javascript-sdk'
import { ToastrService } from 'ngx-toastr'
import {
  faCalendar,
  faCog,
  faExclamationCircle,
  faQuestionCircle,
  faTrash,
} from '@fortawesome/free-solid-svg-icons'
import { SupportedRates } from '@app-seller/models/currency-geography.types'
import { FormBuilder } from '@angular/forms'
import { TypedFormGroup } from 'ngx-forms-typed'
import { NgbTimeStruct } from '@ng-bootstrap/ng-bootstrap'

interface SalePriceFormValue {
  saleStartDate: Date
  saleStartTime: NgbTimeStruct
  saleEndDate: Date
  saleEndTime: NgbTimeStruct
}
type SalePriceForm = TypedFormGroup<SalePriceFormValue>
@Component({
  selector: 'price-break-editor',
  templateUrl: './price-break-editor.component.html',
  styleUrls: ['./price-break-editor.component.scss'],
})
export class PriceBreakEditor {
  salePriceForm: SalePriceForm
  @Input() readonly = false
  @Input() currency: SupportedRates
  @Input() isRequired: boolean
  @Input()
  set priceSchedule(value: PriceSchedule) {
    if (value) {
      this.isAddingPriceBreak = false
      this.isValidNewBreak = false
      this.priceScheduleEditable = JSON.parse(
        JSON.stringify(value)
      ) as PriceSchedule
      this.newPriceBreak = this.getEmptyBreak()
      this.salePriceForm = this.formBuilder.group({
        saleStartDate: [value.SaleStart ? new Date(value.SaleStart) : null],
        saleStartTime: [this.toTimeStruct(value.SaleStart)],
        saleEndDate: [value.SaleEnd ? new Date(value.SaleEnd) : null],
        saleEndTime: [this.toTimeStruct(value.SaleEnd)],
      }) as SalePriceForm
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
  faCalendar = faCalendar
  faQuestionCircle = faQuestionCircle
  _specCount: number
  _variantCount: number

  isAddingPriceBreak = false
  newPriceBreak: PriceBreak
  isValidNewBreak = false
  editPriceBreaks = false

  constructor(
    private toasterService: ToastrService,
    private formBuilder: FormBuilder
  ) {}

  getEmptyBreak(): PriceBreak {
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

  updateNewBreak(
    event: MouseEvent & { target: HTMLInputElement },
    field: string
  ): void {
    let value: string | number = event.target.value
    if (field === 'Quantity') {
      value = parseInt(value, 10)
    }
    this.newPriceBreak[field] = value
    const areErrors = this.handlePriceBreakErrors(
      this.priceScheduleEditable.PriceBreaks
    )
    this.isValidNewBreak =
      !areErrors && !!this.newPriceBreak.Price && !!this.newPriceBreak.Quantity
  }

  saleStartChanged(): void {
    if (this.readonly) {
      return
    }
    const date = this.salePriceForm.controls.saleStartDate.value
    const time = this.salePriceForm.controls.saleStartTime.value
    if (typeof date === 'string') {
      // malformed date, don't update
      return
    }

    if (date) {
      if (time) {
        date.setHours(time.hour)
        date.setMinutes(time.minute)
        date.setSeconds(time.second)
      }
      this.priceScheduleEditable.SaleStart = date.toISOString()
      this.emitUpdatedSchedule()
    } else {
      this.priceScheduleEditable.SaleStart = null
      this.emitUpdatedSchedule()
    }
  }

  saleEndChanged(): void {
    if (this.readonly) {
      return
    }
    const date = this.salePriceForm.controls.saleEndDate.value
    const time = this.salePriceForm.controls.saleEndTime.value
    if (typeof date === 'string') {
      // malformed date, don't update
      return
    }

    if (date) {
      if (time) {
        date.setHours(time.hour)
        date.setMinutes(time.minute)
        date.setSeconds(time.second)
      }
      this.priceScheduleEditable.SaleEnd = date.toISOString()
      this.emitUpdatedSchedule()
    } else {
      this.priceScheduleEditable.SaleEnd = null
      this.emitUpdatedSchedule()
    }
  }

  updateExistingBreak(
    event: MouseEvent & { target: HTMLInputElement },
    index: number,
    field: string
  ): void {
    const value = event.target.value
    this.priceScheduleEditable.PriceBreaks[index][field] = parseFloat(value)
    this.emitUpdatedSchedule()
  }

  getPriceBreakRange(index: number): string {
    const priceBreaks = this.priceScheduleEditable.PriceBreaks
    if (!priceBreaks.length) return ''
    const indexOfNextPriceBreak = index + 1
    let rangeString: string
    if (
      priceBreaks[index].Quantity ===
      priceBreaks[indexOfNextPriceBreak]?.Quantity - 1
    ) {
      rangeString = priceBreaks[index].Quantity?.toString?.()
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

  toTimeStruct(isoDateString: string): NgbTimeStruct {
    if (!isoDateString) {
      return {
        hour: 8,
        minute: 0,
        second: 0,
      }
    }
    const date = new Date(isoDateString)
    return {
      hour: date.getHours(),
      minute: date.getMinutes(),
      second: date.getSeconds(),
    }
  }

  toUtcDate(date?: Date): Date {
    if (!date) {
      return date
    }
    if (typeof date === 'string') {
      // malformed date
      return date
    }
    return new Date(
      Date.UTC(
        date.getUTCFullYear(),
        date.getUTCMonth() + 1,
        date.getUTCDate(),
        date.getUTCHours(),
        date.getUTCMinutes(),
        date.getUTCSeconds()
      )
    )
  }

  handleUpdateUseCumulativeQuantity(
    event: MouseEvent & { target: HTMLInputElement }
  ): void {
    this.priceScheduleEditable.UseCumulativeQuantity = Boolean(
      event.target.value
    )
    this.emitUpdatedSchedule()
  }

  handleUpdatePriceBreaks(
    event: MouseEvent & { target: HTMLInputElement },
    field: string
  ): void {
    this.newPriceBreak[field] = event.target.value
  }

  toggleEditPriceBreaks(): void {
    this.editPriceBreaks = !this.editPriceBreaks
  }
}
