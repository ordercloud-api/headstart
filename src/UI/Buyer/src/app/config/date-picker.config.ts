import { Injectable } from '@angular/core'
import {
  NgbDateStruct,
  NgbDateAdapter,
  NgbDateParserFormatter,
} from '@ng-bootstrap/ng-bootstrap'

/**
 * this adapter allows us to use the native date object when
 * interacting with ngb datepicker instead of their ngbDateStruct
 * docs on adapter: https://ng-bootstrap.github.io/#/components/datepicker/examples#adapter
 */
@Injectable({
  providedIn: 'root',
})
export class NgbDateNativeAdapter extends NgbDateAdapter<Date> {
  fromModel(date: Date): NgbDateStruct {
    return date && date.getFullYear
      ? {
          year: date.getFullYear(),
          month: date.getMonth() + 1,
          day: date.getDate(),
        }
      : null
  }

  toModel(date: NgbDateStruct): Date {
    return date ? new Date(date.year, date.month - 1, date.day) : null
  }
}

/**
 * this formatter overrides the default yyyy-mm-dd format
 * provided by ng-bootstrap to a custom one of mm-dd-yyyy
 */
@Injectable({
  providedIn: 'root',
})
export class NgbDateCustomParserFormatter extends NgbDateParserFormatter {
  parse(value: string): NgbDateStruct {
    if (!value) return null
    const dateParts = value.trim().split('-')
    if (dateParts.length === 1 && this.isNumber(dateParts[0])) {
      return { year: this.toInteger(dateParts[0]), month: null, day: null }
    } else if (
      dateParts.length === 2 &&
      this.isNumber(dateParts[0]) &&
      this.isNumber(dateParts[1])
    ) {
      return {
        year: this.toInteger(dateParts[1]),
        month: this.toInteger(dateParts[0]),
        day: null,
      }
    } else if (
      dateParts.length === 3 &&
      this.isNumber(dateParts[0]) &&
      this.isNumber(dateParts[1]) &&
      this.isNumber(dateParts[2])
    ) {
      return {
        year: this.toInteger(dateParts[2]),
        month: this.toInteger(dateParts[0]),
        day: this.toInteger(dateParts[1]),
      }
    }
  }

  format(date: NgbDateStruct): string {
    let stringDate = ''
    if (date) {
      stringDate += `${this.padNumber(date.month)}-`
      stringDate += `${this.padNumber(date.day)}-`
      stringDate += date.year
    }
    return stringDate
  }

  padNumber(value: number): string {
    if (this.isNumber(value)) {
      return `0${value}`.slice(-2)
    } else {
      return ''
    }
  }

  isNumber(value: any): boolean {
    return !isNaN(this.toInteger(value))
  }

  toInteger(value: any): number {
    return parseInt(`${value}`, 10)
  }
}
