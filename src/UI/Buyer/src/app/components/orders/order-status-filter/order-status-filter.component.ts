import { Component, OnInit, OnDestroy } from '@angular/core'
import { UntypedFormGroup, UntypedFormControl } from '@angular/forms'
import { takeWhile } from 'rxjs/operators'
import { OrderFilters, HeadstartOrderStatus } from 'src/app/models/order.types'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './order-status-filter.component.html',
  styleUrls: ['./order-status-filter.component.scss'],
})
export class OCMOrderStatusFilter implements OnInit, OnDestroy {
  alive = true
  form: UntypedFormGroup
  statuses = []

  constructor(private context: ShopperContextService) {}

  ngOnInit(): void {
    this.statuses = Object.values(HeadstartOrderStatus)
    this.form = new UntypedFormGroup({
      status: new UntypedFormControl(HeadstartOrderStatus.AllSubmitted),
    })
    this.context.orderHistory.filters.activeFiltersSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe((filters: OrderFilters) => {
        this.form.setValue({ status: filters.status || '' })
      })
  }

  selectStatus(): void {
    const status = this.form.get('status').value
    this.context.orderHistory.filters.filterByStatus(status)
  }

  ngOnDestroy(): void {
    this.alive = false
  }
}
