import { Component, OnInit, OnDestroy } from '@angular/core'
import { faCalendar, faTimes } from '@fortawesome/free-solid-svg-icons'
import { FormGroup, FormControl } from '@angular/forms'
import { debounceTime, takeWhile } from 'rxjs/operators'
import { DateValidator } from '../../../validators/validators'
import { DatePipe } from '@angular/common'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { OrderFilters } from 'src/app/models/order.types'

@Component({
  templateUrl: './order-date-filter.component.html',
  styleUrls: ['./order-date-filter.component.scss'],
})
export class OCMOrderDateFilter implements OnInit, OnDestroy {
  alive = true
  faCalendar = faCalendar
  faTimes = faTimes
  form: FormGroup

  constructor(
    private datePipe: DatePipe,
    private context: ShopperContextService
  ) {}

  ngOnInit(): void {
    this.form = new FormGroup({
      fromDate: new FormControl(null as Date, DateValidator),
      toDate: new FormControl(null as Date, DateValidator),
    })
    this.onFormChanges()
    this.context.orderHistory.filters.activeFiltersSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe(this.handlefiltersChange)
  }

  handlefiltersChange = (filters: OrderFilters): void => {
    const fromDate = this.inverseFormatDate(filters.fromDate)
    const toDate = this.inverseFormatDate(filters.toDate)
    this.form.setValue({ fromDate, toDate })
  }

  clearToDate(): void {
    this.form.get('toDate').setValue(null)
    this.doFilter()
  }

  clearFromDate(): void {
    this.form.get('fromDate').setValue(null)
    this.doFilter()
  }

  ngOnDestroy(): void {
    this.alive = false
  }

  private onFormChanges(): void {
    this.form.valueChanges
      .pipe(
        debounceTime(500),
        takeWhile(() => this.alive)
      )
      .subscribe(() => {
        this.doFilter()
      })
  }

  private doFilter(): void {
    if (this.form.get('fromDate').invalid || this.form.get('toDate').invalid)
      return

    const fromDate: Date = this.form.get('fromDate').value
    const toDate: Date = this.form.get('toDate').value
    this.context.orderHistory.filters.filterByDateSubmitted(
      this.formatDate(fromDate),
      this.formatDate(toDate)
    )
  }

  private formatDate(date: Date): string {
    return date
      ? this.datePipe.transform(date, 'shortDate').replace(/\//g, '-')
      : null
  }

  private inverseFormatDate(date: string): Date {
    return date ? new Date(date) : null
  }
}
