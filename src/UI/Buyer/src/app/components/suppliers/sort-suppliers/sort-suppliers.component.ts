import {
  Component,
  OnInit,
  Output,
  EventEmitter,
  OnDestroy,
} from '@angular/core'
import { FormGroup, FormControl } from '@angular/forms'
import { takeWhile } from 'rxjs/operators'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './sort-suppliers.component.html',
  styleUrls: ['./sort-suppliers.component.scss'],
})
export class OCMSupplierSort implements OnInit, OnDestroy {
  alive = true
  form: FormGroup
  options = [
    { value: 'Name', label: 'A to Z' },
    { value: '!Name', label: 'Z to A' },
  ]
  @Output() closePopoverEvent = new EventEmitter()

  constructor(private context: ShopperContextService) {}

  ngOnInit(): void {
    this.form = new FormGroup({ sortBy: new FormControl(null) })
    this.context.supplierFilters.activeFiltersSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe((filters) => {
        const sortBy = filters.sortBy?.length ? filters.sortBy[0] : undefined
        this.setForm(sortBy)
      })
  }

  sortStrategyChanged(): void {
    const sortValue = this.form.get('sortBy').value
    this.context.supplierFilters.sortBy([sortValue])
    this.closePopoverEvent.emit()
  }

  cancelFilters(): void {
    this.closePopoverEvent.emit()
  }

  ngOnDestroy(): void {
    this.alive = false
  }

  private setForm(sortBy: string): void {
    sortBy = sortBy || null
    this.form.setValue({ sortBy })
  }
}
