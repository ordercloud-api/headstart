import { Component, OnInit, OnDestroy } from '@angular/core'
import { FormGroup, FormControl } from '@angular/forms'
import { takeWhile } from 'rxjs/operators'
import { OrderFilters, HeadstartOrderStatus } from 'src/app/models/order.types'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './seller-product-filter.component.html',
  styleUrls: ['./seller-product-filter.component.scss'],
})
export class OCMOrderSellerOwnedProductFilter implements OnInit, OnDestroy {
  alive = true
  form: FormGroup
  statuses = []

  constructor(private context: ShopperContextService) {}

  ngOnInit(): void {
    this.statuses = Object.values(HeadstartOrderStatus)
    this.form = new FormGroup({
      status: new FormControl(HeadstartOrderStatus.AllSubmitted),
    })
    this.context.orderHistory.filters.activeFiltersSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe((filters: OrderFilters) => {
        this.form.setValue({ status: filters.status || '' })
      })
  }

  selectIsSellerOwned(): void {
    const status = this.form.get('isSellerOwned').value
    this.context.orderHistory.filters.filterByStatus(status)
  }

  ngOnDestroy(): void {
    this.alive = false
  }
}
