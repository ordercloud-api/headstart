import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core'
import { Product } from '@ordercloud/angular-sdk'
import { FormGroup, FormBuilder, Validators } from '@angular/forms'
import { AppFormErrorService } from '@app-seller/shared/services/form-error/form-error.service'
import { RegexService } from '@app-seller/shared/services/regex/regex.service'

@Component({
  selector: 'product-form',
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.scss'],
})
export class ProductFormComponent implements OnInit {
  private _existingProduct = {} as Product
  @Input()
  btnText: string
  @Output()
  formSubmitted = new EventEmitter()
  productForm: FormGroup

  constructor(
    private formBuilder: FormBuilder,
    private formErrorService: AppFormErrorService,
    private regexService: RegexService
  ) {}

  ngOnInit() {
    this.setForm()
  }

  @Input()
  set existingProduct(product: Product) {
    this._existingProduct = product || ({} as Product)
    if (!this.productForm) {
      this.setForm()
      return
    }
    this.productForm.setValue({
      ID: this._existingProduct.ID || '',
      Name: this._existingProduct.Name || '',
      Description: this._existingProduct.Description || '',
      Active: !!this._existingProduct.Active,
      Featured: this._existingProduct.xp && this._existingProduct.xp.Featured,
    })
  }

  setForm() {
    this.productForm = this.formBuilder.group({
      ID: [
        this._existingProduct.ID || '',
        Validators.pattern(this.regexService.ID),
      ],
      Name: [
        this._existingProduct.Name || '',
        [Validators.required, Validators.pattern(this.regexService.ID)],
      ],
      Description: [this._existingProduct.Description || ''],
      Active: [!!this._existingProduct.Active],
      Featured: [this._existingProduct.xp && this._existingProduct.xp.Featured],
    })
  }

  protected onSubmit() {
    if (this.productForm.status === 'INVALID') {
      return this.formErrorService.displayFormErrors(this.productForm)
    }

    const product = {
      ...this.productForm.value,
      xp: { Featured: this.productForm.value.Featured },
    }

    this.formSubmitted.emit(product)
  }

  // control display of error messages
  protected hasRequiredError = (controlName: string) =>
    this.formErrorService.hasRequiredError(controlName, this.productForm)
  protected hasPatternError = (controlName: string) =>
    this.formErrorService.hasPatternError(controlName, this.productForm)
}
