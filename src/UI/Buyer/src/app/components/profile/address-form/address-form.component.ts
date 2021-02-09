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
import { FormGroup, Validators, FormControl } from '@angular/forms'

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
  addressForm: FormGroup
  shouldSaveAddressForm: FormGroup
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
    this.addressForm = new FormGroup({
      FirstName: new FormControl(this.ExistingAddress.FirstName || '', [
        Validators.required,
        ValidateName,
      ]),
      LastName: new FormControl(this.ExistingAddress.LastName || '', [
        Validators.required,
        ValidateName,
      ]),
      Street1: new FormControl(
        this.ExistingAddress.Street1 || '',
        Validators.required
      ),
      Street2: new FormControl(this.ExistingAddress.Street2 || ''),
      City: new FormControl(this.ExistingAddress.City || ''),
      State: new FormControl(
        this.ExistingAddress.State || null,
        Validators.required
      ),
      Zip: new FormControl(this.ExistingAddress.Zip || '', [
        Validators.pattern(getZip(this.homeCountry)),
        Validators.required,
      ]),
      Phone: new FormControl(this.ExistingAddress.Phone || '', ValidatePhone),
      Country: new FormControl(this.homeCountry || '', Validators.required),
      ID: new FormControl(this.ExistingAddress.ID || ''),
    })
    this.shouldSaveAddressForm = new FormGroup({
      shouldSaveAddress: new FormControl(false),
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
