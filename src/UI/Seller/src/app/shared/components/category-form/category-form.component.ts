import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core'
import { Category } from '@ordercloud/angular-sdk'
import { FormGroup, FormBuilder, Validators } from '@angular/forms'
import { AppFormErrorService } from '@app-seller/shared/services/form-error/form-error.service'
import { RegexService } from '@app-seller/shared/services/regex/regex.service'

@Component({
  selector: 'category-form',
  templateUrl: './category-form.component.html',
  styleUrls: ['./category-form.component.scss'],
})
export class CategoryFormComponent implements OnInit {
  private _existingCategory: Category = {}
  @Input()
  btnText: string
  @Output()
  formSubmitted = new EventEmitter()
  categoryForm: FormGroup

  constructor(
    private formBuilder: FormBuilder,
    private formErrorService: AppFormErrorService,
    private regexService: RegexService
  ) {}

  ngOnInit() {
    this.setForm()
  }

  @Input()
  set existingCategory(category: Category) {
    this._existingCategory = category || {}
    if (!this.categoryForm) {
      this.setForm()
      return
    }
    this.categoryForm.setValue({
      ID: this._existingCategory.ID || '',
      Name: this._existingCategory.Name || '',
      Description: this._existingCategory.Description || '',
      Active: !!this._existingCategory.Active,
    })
  }

  setForm() {
    this.categoryForm = this.formBuilder.group({
      ID: [
        this._existingCategory.ID || '',
        Validators.pattern(this.regexService.ID),
      ],
      Name: [
        this._existingCategory.Name || '',
        [Validators.required, Validators.pattern(this.regexService.ObjectName)],
      ],
      Description: [this._existingCategory.Description || ''],
      Active: [!!this._existingCategory.Active],
    })
  }

  protected onSubmit() {
    if (this.categoryForm.status === 'INVALID') {
      return this.formErrorService.displayFormErrors(this.categoryForm)
    }

    this.formSubmitted.emit(this.categoryForm.value)
  }

  // control display of error messages
  protected hasRequiredError = (controlName: string) =>
    this.formErrorService.hasRequiredError(controlName, this.categoryForm)
  protected hasPatternError = (controlName: string) =>
    this.formErrorService.hasPatternError(controlName, this.categoryForm)
}
