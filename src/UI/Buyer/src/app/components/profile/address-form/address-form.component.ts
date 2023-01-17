import {
  Component,
  OnInit,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges,
  OnDestroy,
} from '@angular/core'
import { UntypedFormGroup, Validators, UntypedFormControl } from '@angular/forms'

// 3rd party
import { BuyerAddress, Address } from 'ordercloud-javascript-sdk'
import {
  ValidateName,
  ValidateUSZip,
  ValidatePhone,
  ValidateCAZip,
} from '../../../validators/validators'
import { GeographyConfig } from '../../../config/geography.class'
import { takeWhile } from 'rxjs/operators'
import { getZip } from 'src/app/services/zip-validator.helper'

@Component({
  templateUrl: './address-form.component.html',
  styleUrls: ['./address-form.component.scss'],
})
export class OCMAddressForm implements OnInit, OnChanges, OnDestroy {
  @Input() btnText: string
  @Input() suggestedAddresses: BuyerAddress[]
  @Input() showOptionToSave = false
  @Input() homeCountry: string
  @Input() addressError: string
  @Output() formDismissed = new EventEmitter()
  @Output()
  formSubmitted = new EventEmitter<{
    address: Address
    shouldSaveAddress: boolean
  }>()
  @Output() formChanged = new EventEmitter<BuyerAddress>()
  @Input() set existingAddress(address: BuyerAddress) {
    this.ExistingAddress = address || {}
    this.setForms()
    this.listenToFormChanges()
    this.addressForm.markAsPristine()
  }
  stateOptions: string[] = []
  countryOptions: { label: string; abbreviation: string }[]
  addressForm: UntypedFormGroup
  shouldSaveAddressForm: UntypedFormGroup
  selectedAddress: BuyerAddress
  alive = true
  private ExistingAddress: BuyerAddress = {}

  constructor() {
    this.countryOptions = GeographyConfig.getCountries()
  }

  ngOnInit(): void {
    this.setForms()
    this.listenToFormChanges()
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.homeCountry) {
      this.addressForm.controls.Country.setValue(this.homeCountry)
      this.onCountryChange()
    }
  }

  setForms(): void {
    this.addressForm = new UntypedFormGroup({
      FirstName: new UntypedFormControl(this.ExistingAddress.FirstName || '', [
        Validators.required,
        ValidateName,
      ]),
      LastName: new UntypedFormControl(this.ExistingAddress.LastName || '', [
        Validators.required,
        ValidateName,
      ]),
      Street1: new UntypedFormControl(
        this.ExistingAddress.Street1 || '',
        Validators.required
      ),
      Street2: new UntypedFormControl(this.ExistingAddress.Street2 || ''),
      City: new UntypedFormControl(this.ExistingAddress.City || ''),
      State: new UntypedFormControl(
        this.ExistingAddress.State || null,
        Validators.required
      ),
      Zip: new UntypedFormControl(this.ExistingAddress.Zip || '', [
        Validators.pattern(getZip(this.homeCountry)),
        Validators.required,
      ]),
      Phone: new UntypedFormControl(this.ExistingAddress.Phone || '', ValidatePhone),
      Country: new UntypedFormControl(this.homeCountry || '', Validators.required),
      ID: new UntypedFormControl(this.ExistingAddress.ID || ''),
    })
    this.shouldSaveAddressForm = new UntypedFormGroup({
      shouldSaveAddress: new UntypedFormControl(false),
    })
  }

  listenToFormChanges(): void {
    this.addressForm.valueChanges
      .pipe(takeWhile(() => this.alive))
      .subscribe((address) => {
        if (!address.ID) {
          // default option for "Shipping Address" select dropdown expects a null value if its missing
          address.ID = null
        }
        this.formChanged.emit(address)
      })
  }

  onCountryChange(event?: any): void {
    const country = this.homeCountry
    this.stateOptions = GeographyConfig.getStates(country).map(
      (s) => s.abbreviation
    )
    if (event) {
      this.addressForm.patchValue({ State: null, Zip: '' })
    }
  }

  getCountryName(countryCode: string): string {
    const country = this.countryOptions.find(
      (c) => c.abbreviation === countryCode
    )
    return country ? country.label : ''
  }

  useSuggestedAddress(address: BuyerAddress): void {
    if (!address.ID) {
      // default option for "Shipping Address" select dropdown expects a null value if its missing
      ;(address as any).ID = null
    }
    this.formChanged.emit(address)
  }

  onSubmit(): void {
    if (this.addressForm.status === 'INVALID') return
    this.formSubmitted.emit({
      address: this.selectedAddress
        ? this.selectedAddress
        : this.addressForm.value,
      shouldSaveAddress: this.shouldSaveAddressForm.controls.shouldSaveAddress
        .value,
    })
  }

  dismissForm(): void {
    this.formDismissed.emit()
  }

  ngOnDestroy(): void {
    this.alive = false
  }
}
