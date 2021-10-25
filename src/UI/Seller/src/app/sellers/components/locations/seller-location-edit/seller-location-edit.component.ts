import {
  Component,
  EventEmitter,
  Input,
  Output,
  OnChanges,
} from '@angular/core'
import { FormGroup, Validators, FormControl } from '@angular/forms'
import { Address, ListPage } from '@ordercloud/angular-sdk'
import { ActivatedRoute } from '@angular/router'
import { GeographyConfig } from '@app-seller/shared/models/supported-countries.constant'
import {
  ValidateZip,
  ValidatePhone,
} from '@app-seller/validators/validators'
import { takeWhile } from 'rxjs/operators'
import { SellerAddressService } from '../seller-address.service'
import {
  OrderCloudIntegrationsConversionRate,
  HeadStartSDK,
} from '@ordercloud/headstart-sdk'
import { SupportedCountries } from '@app-seller/models/currency-geography.types'

@Component({
  selector: 'app-seller-location-edit',
  templateUrl: './seller-location-edit.component.html',
  styleUrls: ['./seller-location-edit.component.scss'],
})
export class SellerLocationEditComponent implements OnChanges {
  alive = true
  locationHasNoProducts: boolean
  countryOptions: SupportedCountries[]
  flag: string
  resourceForm: FormGroup
  zipLabel = 'Zip Code'
  zipPlaceholder = 'Enter Zip'
  stateLabel = 'State'
  statePlaceholder = 'Enter State'
  cityLabel = 'City'
  cityPlaceholder = 'Enter City'
  street1Label = 'Street 1'
  street1Placeholder = 'Enter Street 1'
  countryHasBeenSelected = false
  currencyOptions: OrderCloudIntegrationsConversionRate[]
  @Input()
  set location(sellerLocation: Address) {
    if (sellerLocation.ID) {
      void this.handleSelectedLocationChange(sellerLocation)
    } else {
      void this.handleSelectedLocationChange(
        this.sellerLocationService.emptyResource
      )
    }
  }
  @Input()
  filterConfig
  @Input()
  suggestedAddresses: ListPage<Address>
  @Output()
  updateResource = new EventEmitter<any>()
  @Output()
  selectAddress = new EventEmitter<any>()
  @Output()
  canDelete = new EventEmitter<boolean>()

  constructor(
    private activatedRoute: ActivatedRoute,
    private sellerLocationService: SellerAddressService
  ) {
    this.countryOptions = GeographyConfig.getCountries()
  }

  ngOnChanges(): void {
    this.setLocationID()
  }

  async handleSelectedLocationChange(location: Address): Promise<void> {
    await HeadStartSDK.ExchangeRates.GetRateList().then((res) => {
      this.currencyOptions = res.Items
      location.Country && this.setFlag(location.Country)
    })
    this.createSellerLocationForm(location)
  }

  createSellerLocationForm(sellerLocation: Address): void {
    this.resourceForm = new FormGroup({
      AddressName: new FormControl(
        sellerLocation.AddressName,
        Validators.required
      ),
      CompanyName: new FormControl(
        sellerLocation.CompanyName,
        Validators.required
      ),
      Street1: new FormControl(sellerLocation.Street1, Validators.required),
      Street2: new FormControl(sellerLocation.Street2),
      City: new FormControl(sellerLocation.City, Validators.required),
      State: new FormControl(sellerLocation.State, Validators.required),
      Zip: new FormControl({ value: sellerLocation.Zip, disabled: true }, [
        Validators.required,
        ValidateZip(sellerLocation.Zip),
      ]),
      Country: new FormControl(sellerLocation.Country, Validators.required),
      Phone: new FormControl(sellerLocation.Phone, ValidatePhone),
    })
    this.setZipValidator()
  }

  setFlag(locationCountry: string): void {
    const currency = this.getCurrencyFromCode(locationCountry)
    this.flag = this.getFlagForCountry(currency)
  }

  setZipValidator(): void {
    const zipControl = this.resourceForm.get('Zip')
    this.countryHasBeenSelected =
      this.resourceForm.controls['Country'].value !== ''
    this.resourceForm
      .get('Country')
      .valueChanges.pipe(takeWhile(() => this.alive))
      .subscribe((code) => {
        const currency = this.getCurrencyFromCode(code)
        this.flag = this.getFlagForCountry(currency)
        this.countryHasBeenSelected = code !== ''
        if (code !== null) zipControl.enable()
        zipControl.setValidators(ValidateZip(code));
      })
  }

  private setLocationID(): void {
    const url = window.location.href
    const splitUrl = url.split('/')
    const endUrl = splitUrl[splitUrl.length - 1]
    const urlParams = this.activatedRoute.snapshot.params
    if (urlParams.locationID) {
      void this.determineIfDeletable(urlParams.locationID)
    }
  }

  private async determineIfDeletable(locationID: string): Promise<void> {
    const hasNoProducts = ((await HeadStartSDK.Suppliers.CanDeleteLocation(
      locationID
    )) as unknown) as boolean
    this.canDelete.emit(hasNoProducts)
  }

  updateResourceFromEvent(event: any, field: string): void {
    this.updateResource.emit({ value: event.target.value, field, form: this.resourceForm })
  }

  handleSellerAddressSelect(address: Address): void {
    this.selectAddress.emit(address)
  }

  ngOnDestroy(): void {
    this.alive = false
  }

  getControlValue(control: string): string | number {
    return this.resourceForm?.controls[control]?.value
  }

  getCountryFromCode(code: string): string {
    return this.countryOptions.find((c) => c.abbreviation === code)?.label
  }

  getCurrencyFromCode = (code: string) =>
    this.countryOptions.find((c) => c.abbreviation === code)?.currency

  getFlagForCountry(currency: string): string {
    if (currency === '') return undefined
    const flagMatch = this.currencyOptions?.find(
      (c) => c.Currency.toString() === currency
    )?.Icon
    if (flagMatch) return flagMatch
  }
}
