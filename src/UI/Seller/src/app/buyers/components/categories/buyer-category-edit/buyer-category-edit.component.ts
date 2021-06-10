import {
  Component,
  Input,
  ChangeDetectorRef,
  Output,
  EventEmitter,
} from '@angular/core'
import { FormControl, FormGroup, Validators } from '@angular/forms'
import { Router } from '@angular/router'
import { ResourceUpdate } from '@app-seller/shared'
import { ValidateNoSpecialCharactersAndSpaces } from '@app-seller/validators/validators'
import { Category } from 'ordercloud-javascript-sdk'

@Component({
  selector: 'app-buyer-category-edit',
  templateUrl: './buyer-category-edit.component.html',
  styleUrls: ['./buyer-category-edit.component.scss'],
})
export class BuyerCategoryEditComponent {
  _category: any
  _categoryFields: any[] = [
    { field: 'ParentID', type: 'string' },
    {
      field: 'ID',
      type: 'string',
      validators: [ValidateNoSpecialCharactersAndSpaces],
    },
    { field: 'Name', type: 'string', validators: [Validators.required] },
    { field: 'Description', type: 'string' },
    { field: 'Active', type: 'boolean' },
  ]
  _params: any
  resourceForm: FormGroup

  constructor(
    private changeDetectorRef: ChangeDetectorRef,
    private router: Router
  ) {}

  @Input()
  set resource(value: any) {
    this._category = value
    this.resourceForm = this.buildForm(value)
    this.changeDetectorRef.detectChanges()
  }
  @Output()
  updateCategory = new EventEmitter<ResourceUpdate>()

  handleUpdateCategory(event: any, fieldType: string) {
    const categoryUpdate = {
      field: event.target.id,
      value:
        fieldType === 'boolean' ? event.target.checked : event.target.value,
      form: this.resourceForm,
    }
    this.updateCategory.emit(categoryUpdate)
  }

  getOptionalText(field: string) {
    if (field.toUpperCase() === 'ID' || field.toUpperCase() === 'PARENTID') {
      return ' (Optional)'
    }
    return ''
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

  getParentID() {
    const routeUrl = this.router.routerState.snapshot.url
    const splitUrl = routeUrl.split('/')
    const endUrl = splitUrl[splitUrl.length - 1]
    if (endUrl.includes('new?')) {
      return endUrl.split('=')[1]
    } else {
      ;('')
    }
  }

  buildForm(resource: Category): FormGroup {
    const formGroup = new FormGroup({})
    this._categoryFields?.forEach((item) => {
      const control = new FormControl(
        resource[item.field] || '',
        item.validators
      )
      formGroup.addControl(item.field, control)
    })

    return formGroup
  }
}
