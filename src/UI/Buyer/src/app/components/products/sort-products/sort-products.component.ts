import { Component, OnInit, OnDestroy } from '@angular/core'
import { FormGroup, FormControl } from '@angular/forms'
import { takeWhile } from 'rxjs/operators'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './sort-products.component.html',
  styleUrls: ['./sort-products.component.scss'],
})
export class OCMProductSort implements OnInit, OnDestroy {
  alive = true
  form: FormGroup
  options = [
    { value: 'ID', label: 'ID: A to Z' },
    { value: '!ID', label: 'ID: Z to A' },
    { value: 'Name', label: 'Name: A to Z' },
    { value: '!Name', label: 'Name: Z to A' },
  ]

  constructor(private context: ShopperContextService) {}

  ngOnInit(): void {
    this.form = new FormGroup({ sortBy: new FormControl(null) })
    this.context.productFilters.activeFiltersSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe((filters) => {
        const sortBy = filters.sortBy?.length ? filters.sortBy[0] : undefined
        this.setForm(sortBy)
      })
  }

  sortStrategyChanged(): void {
    const sortValue = this.form.get('sortBy').value
    this.context.productFilters.sortBy([sortValue])
  }

  ngOnDestroy(): void {
    this.alive = false
  }

  private setForm(sortBy: string): void {
    sortBy = sortBy || null
    this.form.setValue({ sortBy })
  }
}
