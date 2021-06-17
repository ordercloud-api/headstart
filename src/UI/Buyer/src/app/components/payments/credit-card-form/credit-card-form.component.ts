import {
  Component,
  Output,
  EventEmitter,
  Input,
  OnChanges,
  OnInit
} from '@angular/core'
import { FormGroup, Validators, FormControl } from '@angular/forms'
import { CreditCardFormatPipe } from 'src/app/pipes/credit-card-format.pipe'
import { ValidateCreditCard } from 'src/app/validators/validators'
import { OrderCloudIntegrationsCreditCardToken } from '@ordercloud/headstart-sdk'
import { GeographyConfig } from 'src/app/config/geography.class'
import {
  faCcAmex,
  faCcMastercard,
  faCcVisa,
} from '@fortawesome/free-brands-svg-icons'
import { getZip } from 'src/app/services/zip-validator.helper'
import { TypedFormGroup } from 'ngx-forms-typed'
import {
  ComponentChanges,
  CreditCard,
  CreditCardFormOutput,
} from 'src/app/models/credit-card.types'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './credit-card-form.component.html',
  styleUrls: ['./credit-card-form.component.scss'],
})
export class OCMCreditCardForm implements OnChanges, OnInit {
  @Output() formSubmitted = new EventEmitter<CreditCardFormOutput>()
  @Output() formDismissed = new EventEmitter()
  @Input() card: OrderCloudIntegrationsCreditCardToken
  @Input() submitText: string
  @Input() showCVV: boolean
  @Input() showCardDetails: boolean
  @Input() isAnon: boolean
  _showCardDetails: boolean
  _showCVV: boolean
  cardError?: string
  cardForm = new FormGroup({}) as TypedFormGroup<CreditCard>
  monthOptions = [
    '01',
    '02',
    '03',
    '04',
    '05',
    '06',
    '07',
    '08',
    '09',
    '10',
    '11',
    '12',
  ]
  yearOptions = this.buildYearOptions()
  stateOptions: string[] = []
  countryOptions: { label: string; abbreviation: string }[]
  faCcVisa = faCcVisa
  faCcMastercard = faCcMastercard
  faCcAmex = faCcAmex
  private readonly defaultCountry = 'US'
  shouldShowShippingOption

  constructor(
    private creditCardFormatPipe: CreditCardFormatPipe,
    private context: ShopperContextService
  ) {
    this.countryOptions = GeographyConfig.getCountries()
    this.stateOptions = this.getStateOptions(this.defaultCountry)
  }

  ngOnInit() {
    this.shouldShowShippingOption = !this.context.router
      .getActiveUrl()
      .includes('/profile')
  }

  ngOnChanges(changes: ComponentChanges<OCMCreditCardForm>): void {
    // template can't reference input properties directly because they may change outside of angular's knowledge
    // instead reference controlled variables that are only updated when angular knows about them (in ngOnChanges)
    if (changes.showCardDetails) {
      this._showCardDetails = changes.showCardDetails.currentValue
    }
    if (changes.showCVV) {
      this._showCVV = changes.showCVV.currentValue
    }

    if (changes.showCVV) {
      const { currentValue, previousValue } = changes.showCVV
      this.handleShowCVVChanges(currentValue, previousValue)
    }
    if (changes.showCardDetails) {
      const { currentValue, previousValue } = changes.showCardDetails
      this.handleShowDetailChanges(currentValue, previousValue)
    }
  }

  handleShowCVVChanges(currentValue: boolean, previousValue: boolean): void {
    if (currentValue && !previousValue) {
      this.buildCVVForm()
    }
    if (!currentValue && previousValue) {
      this.removeCVVForm()
    }
  }

  handleShowDetailChanges(currentValue: boolean, previousValue: boolean): void {
    if (currentValue && !previousValue) {
      this.buildCardDetailsForm(this.card)
    }
    if (!currentValue && previousValue) {
      this.removeCardDetailsForm()
    }
  }

  ccEntered({
    message,
    validationError,
  }: {
    message: string
    validationError: string
  }): void {
    this.cardForm.controls.token.setValue(message)
    this.cardError = validationError
  }

  onSubmit(): void {
    this.formSubmitted.emit({
      card: {
        AccountNumber: this.cardForm.value.token,
        CardholderName: this.cardForm.value.name,
        ExpirationDate: `${this.cardForm.value.month}${this.cardForm.value.year}`,
        CCBillingAddress: {
          Street1: this.cardForm.value.street,
          City: this.cardForm.value.city,
          State: this.cardForm.value.state,
          Zip: this.cardForm.value.zip,
          Country: this.cardForm.value.country,
        },
      },
      cvv: this.cardForm.value.cvv,
    })
  }

  dismissForm(): void {
    this.formDismissed.emit()
  }

  onCountryChange(event?: any): void {
    this.stateOptions = this.getStateOptions(this.cardForm.value.country)
    this.cardForm
      .get('zip')
      .setValidators([
        Validators.pattern(getZip(this.cardForm.value.country)),
        Validators.required,
      ])
    if (event) {
      this.cardForm.patchValue({ state: null, zip: '' })
    }
  }

  private buildCVVForm(): void {
    this.cardForm.addControl('cvv', new FormControl('', Validators.required))
  }

  private removeCVVForm(): void {
    this.cardForm.removeControl('cvv')
  }

  private buildCardDetailsForm(
    card: OrderCloudIntegrationsCreditCardToken
  ): void {
    const form = {
      name: card?.CardholderName || '',
      token: card?.AccountNumber
        ? this.creditCardFormatPipe.transform(card.AccountNumber)
        : '',
      month: card?.ExpirationDate?.substring(0, 2) || this.monthOptions[0],
      year:
        card?.ExpirationDate?.substring(2, 4) || this.yearOptions[1].slice(-2),
      street: card?.CCBillingAddress?.Street1 || '',
      city: card?.CCBillingAddress?.City || '',
      state: card?.CCBillingAddress?.State || null,
      zip: card?.CCBillingAddress?.Zip || '',
      country: card?.CCBillingAddress?.Country || this.defaultCountry,
    }

    this.cardForm.addControl(
      'token',
      new FormControl(form.token, [Validators.required, ValidateCreditCard])
    )
    this.cardForm.addControl('name', new FormControl(name, Validators.required))
    this.cardForm.addControl(
      'month',
      new FormControl(form.month, Validators.required)
    )
    this.cardForm.addControl(
      'year',
      new FormControl(form.year, Validators.required)
    )
    this.cardForm.addControl(
      'street',
      new FormControl(form.street, Validators.required)
    )
    this.cardForm.addControl(
      'city',
      new FormControl(form.city, Validators.required)
    )
    this.cardForm.addControl(
      'state',
      new FormControl(form.state, Validators.required)
    )
    this.cardForm.addControl(
      'zip',
      new FormControl(form.zip, [
        Validators.pattern(getZip(form.country)),
        Validators.required,
      ])
    )
    this.cardForm.addControl(
      'country',
      new FormControl(form.country, Validators.required)
    )
    if (this.shouldShowShippingOption) {
      this.cardForm.addControl('useShippingAddress', new FormControl(false))
    }
  }

  mapShippingAddressToBilling(event: Event) {
    const value = (event.target as HTMLInputElement).checked
    const lineItems = this.context.order.getLineItems()
    if (value && lineItems?.Items && lineItems.Items[0]) {
      const firstItem = lineItems.Items[0]
      this.cardForm.controls['street'].setValue(firstItem.ShippingAddress?.Street1)
      this.cardForm.controls['city'].setValue(firstItem.ShippingAddress?.City)
      this.cardForm.controls['state'].setValue(firstItem.ShippingAddress?.State)
      this.cardForm.controls['zip'].setValue(firstItem.ShippingAddress?.Zip)
      this.cardForm.controls['country'].setValue(firstItem.ShippingAddress?.Country)
    } else {
      this.cardForm.controls['street'].setValue('')
      this.cardForm.controls['city'].setValue('')
      this.cardForm.controls['state'].setValue('')
      this.cardForm.controls['zip'].setValue('')
      this.cardForm.controls['country'].setValue('')
    }
  }

  private removeCardDetailsForm(): void {
    const nonCVVCtrls = [
      'token',
      'name',
      'number',
      'month',
      'year',
      'street',
      'city',
      'state',
      'zip',
      'country',
    ]
    for (const ctrl of nonCVVCtrls) {
      this.cardForm.removeControl(ctrl)
    }
  }

  private getStateOptions(country: string): string[] {
    return GeographyConfig.getStates(country).map((s) => s.abbreviation)
  }

  private buildYearOptions(): string[] {
    const currentYear = new Date().getFullYear()
    return Array(20)
      .fill(0)
      .map((x, i) => `${i + currentYear}`)
  }
}
