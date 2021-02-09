import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core'
import { FormGroup, FormBuilder, Validators } from '@angular/forms'

import { BuyerAddress, Address } from '@ordercloud/angular-sdk'
import { AppFormErrorService } from '@app-seller/shared/services/form-error/form-error.service'
import { AppGeographyService } from '@app-seller/shared/services/geography/geography.service'
import { RegexService } from '@app-seller/shared/services/regex/regex.service'

@Component({
  selector: 'shared-address-form',
  templateUrl: './address-form.component.html',
  styleUrls: ['./address-form.component.scss'],
})
export class AddressFormComponent implements OnInit {
  private _existingAddress: BuyerAddress = {}
  @Input()
  btnText: string
  @Output()
  formSubmitted = new EventEmitter<{ address: Address; prevID: string }>()
  stateOptions: string[] = []
  countryOptions: { label: string; abbreviation: string }[]
  addressForm: FormGroup

  constructor(
    private geographyService: AppGeographyService,
    private formBuilder: FormBuilder,
    private formErrorService: AppFormErrorService,
    private regexService: RegexService
  ) {
    this.countryOptions = this.geographyService.getCountries()
  }

  ngOnInit() {
    this.setForm()
  }

  @Input()
  set existingAddress(address: BuyerAddress) {
    this._existingAddress = address || {}
    if (!this.addressForm) {
      this.setForm()
      return
    }

    this.addressForm.setValue({
      ID: this._existingAddress.ID || '',
      AddressName: this._existingAddress.AddressName || '',
      FirstName: this._existingAddress.FirstName || '',
      LastName: this._existingAddress.LastName || '',
      Street1: this._existingAddress.Street1 || '',
      Street2: this._existingAddress.Street2 || '',
      City: this._existingAddress.City || '',
      State: this._existingAddress.State || null,
      Zip: this._existingAddress.Zip || '',
      Country: this._existingAddress.Country || 'US',
      Phone: this._existingAddress.Phone || '',
    })
    this.onCountryChange()
  }

  setForm() {
    this.addressForm = this.formBuilder.group({
      ID: [
        this._existingAddress.ID || '',
        Validators.pattern(this.regexService.ID),
      ],
      AddressName: this._existingAddress.AddressName || '',
      FirstName: [
        this._existingAddress.FirstName || '',
        [Validators.required, Validators.pattern(this.regexService.HumanName)],
      ],
      LastName: [
        this._existingAddress.LastName || '',
        [Validators.required, Validators.pattern(this.regexService.HumanName)],
      ],
      Street1: [this._existingAddress.Street1 || '', Validators.required],
      Street2: [this._existingAddress.Street2 || ''],
      City: [
        this._existingAddress.City || '',
        [Validators.required, Validators.pattern(this.regexService.HumanName)],
      ],
      State: [this._existingAddress.State || null, Validators.required],
      Zip: [
        this._existingAddress.Zip || '',
        [
          Validators.required,
          Validators.pattern(
            this.regexService.getZip(this._existingAddress.Country)
          ),
        ],
      ],
      Country: [this._existingAddress.Country || 'US', Validators.required],
      Phone: [
        this._existingAddress.Phone || '',
        Validators.pattern(this.regexService.Phone),
      ],
    })
    this.onCountryChange()
  }

  onCountryChange(event?) {
    const country = this.addressForm.value.Country
    this.stateOptions = this.geographyService
      .getStates(country)
      .map((s) => s.abbreviation)
    this.addressForm
      .get('Zip')
      .setValidators([
        Validators.required,
        Validators.pattern(this.regexService.getZip(country)),
      ])
    if (event) {
      this.addressForm.patchValue({ State: null, Zip: '' })
    }
  }

  protected onSubmit() {
    if (this.addressForm.status === 'INVALID') {
      return this.formErrorService.displayFormErrors(this.addressForm)
    }
    this.formSubmitted.emit({
      address: this.addressForm.value,
      prevID: this._existingAddress.ID,
    })
  }

  // control display of error messages
  protected hasRequiredError = (controlName: string) =>
    this.formErrorService.hasRequiredError(controlName, this.addressForm)
  protected hasPatternError = (controlName: string) =>
    this.formErrorService.hasPatternError(controlName, this.addressForm)
}
