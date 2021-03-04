import {
  Component,
  Input,
  ChangeDetectorRef,
  Output,
  EventEmitter,
} from '@angular/core'
import { FormControl, FormGroup, Validators } from '@angular/forms'
import { Router } from '@angular/router'
import { Category } from '@ordercloud/angular-sdk'
import { ValidateNoSpecialCharactersAndSpaces } from "@app-seller/validators/validators";

@Component({
  selector: 'app-buyer-category-edit',
  templateUrl: './buyer-category-edit.component.html',
  styleUrls: ['./buyer-category-edit.component.scss'],
})
export class BuyerCategoryEditComponent {
  _category: any
  _categoryFields: any[] = [
    { field: 'ParentID', type: 'string' },
    { field: 'ID', type: 'string', validators: [ValidateNoSpecialCharactersAndSpaces]},
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
  updateCategory = new EventEmitter()

  handleUpdateCategory(event: any, fieldType?: string) {
    this.updateCategory.emit(this.resourceForm)
  }

  handleCheckChange(event: any, field: string) {
    this.resourceForm.patchValue({
      [field]: event.target.checked
    })
    this.updateCategory.emit(this.resourceForm)
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

  buildForm(resource: Category): FormGroup {
    var formGroup = new FormGroup({})
    this._categoryFields?.forEach((item) => {
      var control = new FormControl((resource[item.field] || ''), item.validators)
      formGroup.addControl(item.field, control)
    })

    return formGroup
  }
}
