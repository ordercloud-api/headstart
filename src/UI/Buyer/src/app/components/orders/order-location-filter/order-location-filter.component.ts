import { Component, OnDestroy, OnInit, ChangeDetectorRef } from '@angular/core'
import { takeWhile } from 'rxjs/operators'
import { HSAddressBuyer } from '@ordercloud/headstart-sdk'
import { FormControl, FormGroup } from '@angular/forms'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { OrderFilters } from 'src/app/models/order.types'

@Component({
  templateUrl: './order-location-filter.component.html',
  styleUrls: ['./order-location-filter.component.scss'],
})
export class OCMOrderLocationFilter implements OnInit, OnDestroy {
  alive = true
  form: FormGroup
  locations: HSAddressBuyer[] = []

  constructor(
    private context: ShopperContextService,
    private changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.form = new FormGroup({
      selectedLocationID: new FormControl(''),
    })
    this.context.orderHistory.filters.activeFiltersSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe((orderFilters: OrderFilters) => {
        this.form.setValue({ selectedLocationID: orderFilters.location || '' })
      })
    this.getLocationsUserCanView()
  }

  async getLocationsUserCanView(): Promise<void> {
    this.locations = await this.context.orderHistory.getLocationsUserCanView()
    if (
      !this.context.orderHistory.filters.activeFiltersSubject.value.location
    ) {
      this.context.orderHistory.filters.filterByLocation(this.locations[0].ID)
    }
  }

  selectLocation(): void {
    const locationID = this.form.get('selectedLocationID').value
    this.context.orderHistory.filters.filterByLocation(locationID)
  }

  ngOnDestroy(): void {
    this.alive = false
  }
}
