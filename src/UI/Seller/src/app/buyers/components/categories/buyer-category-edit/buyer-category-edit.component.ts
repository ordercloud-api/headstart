import {
  Component,
  Input,
  ChangeDetectorRef,
  Output,
  EventEmitter,
} from '@angular/core'
import { Router } from '@angular/router'

@Component({
  selector: 'app-buyer-category-edit',
  templateUrl: './buyer-category-edit.component.html',
  styleUrls: ['./buyer-category-edit.component.scss'],
})
export class BuyerCategoryEditComponent {
  _category: any
  _categoryFields: any[] = [
    { field: 'ParentID', type: 'string' },
    { field: 'ID', type: 'string' },
    { field: 'Name', type: 'string' },
    { field: 'Description', type: 'string' },
    { field: 'Active', type: 'boolean' },
  ]
  _params: any

  constructor(
    private changeDetectorRef: ChangeDetectorRef,
    private router: Router
  ) {}

  @Input()
  set resource(value: any) {
    this._category = value
    this.changeDetectorRef.detectChanges()
  }
  @Output()
  updateCategory = new EventEmitter()

  handleUpdateCategory(event: any, fieldType: string) {
    const categoryUpdate = {
      field: event.target.id,
      value:
        fieldType === 'boolean' ? event.target.checked : event.target.value,
    }
    this.updateCategory.emit(categoryUpdate)
  }

  checkForParent() {
    const routeUrl = this.router.routerState.snapshot.url
    const splitUrl = routeUrl.split('/')
    const endUrl = splitUrl[splitUrl.length - 1]
    let params = endUrl.includes('new?') ? endUrl.split('=') : ''
    params = params[1] ? params[1] : ''
    params = params[2] ? params[2] : ''
    this._params = params
    return endUrl.includes('new?')
  }
}
