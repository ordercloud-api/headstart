import {
  Component,
  EventEmitter,
  Input,
  Output,
  OnChanges,
} from '@angular/core'
import { FormGroup, Validators, FormControl } from '@angular/forms'
import { Address, ListPage } from '@ordercloud/angular-sdk'
import { MiddlewareAPIService } from '@app-seller/shared/services/middleware-api/middleware-api.service'
import { ActivatedRoute } from '@angular/router'
import { GeographyConfig } from '@app-seller/shared/models/supported-countries.constant'
import {
  ValidateCAZip,
  ValidateUSZip,
  ValidatePhone,
} from '@app-seller/validators/validators'
import { takeWhile } from 'rxjs/operators'
import { SupplierAddressService } from '../supplier-address.service'
import {
  OrderCloudIntegrationsConversionRate,
  HeadStartSDK,
} from '@ordercloud/headstart-sdk'
import { SupportedCountries } from '@app-seller/models/currency-geography.types'
@Component({
  selector: 'app-supplier-location-edit',
  templateUrl: './supplier-location-edit.component.html',
  styleUrls: ['./supplier-location-edit.component.scss'],
})
export class SupplierLocationEditComponent implements OnChanges {
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
  set location(supplierLocation: Address) {
    if (supplierLocation.ID) {
      this.handleSelectedLocationChange(supplierLocation)
    } else {
      this.handleSelectedLocationChange(
        this.supplierLocationService.emptyResource
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
    private middleware: MiddlewareAPIService,
    private activatedRoute: ActivatedRoute,
    private supplierLocationService: SupplierAddressService
  ) {
    this.countryOptions = GeographyConfig.getCountries()
  }

  ngOnChanges() {
    this.setLocationID()
  }

  async handleSelectedLocationChange(location: Address): Promise<void> {
    // Bug with the SDK where GetRateList has wrong return type
    try {
      HeadStartSDK.ExchangeRates.GetRateList().then((res) => {
        this.currencyOptions = res.Items
        location.Country && this.setFlag(location.Country)
      })
    } finally {
      this.createSupplierLocationForm(location)
    }
  }

  createSupplierLocationForm(supplierLocation: Address) {
    this.resourceForm = new FormGroup({
      AddressName: new FormControl(
        supplierLocation.AddressName,
        Validators.required
      ),
      CompanyName: new FormControl(
        supplierLocation.CompanyName,
        Validators.required
      ),
      Street1: new FormControl(supplierLocation.Street1, Validators.required),
      Street2: new FormControl(supplierLocation.Street2),
      City: new FormControl(supplierLocation.City, Validators.required),
      State: new FormControl(supplierLocation.State, Validators.required),
      Zip: new FormControl({ value: supplierLocation.Zip, disabled: true }, [
        Validators.required,
        ValidateUSZip || ValidateCAZip,
      ]),
      Country: new FormControl(supplierLocation.Country, Validators.required),
      Phone: new FormControl(supplierLocation.Phone, ValidatePhone),
    })
    this.setZipValidator()
  }

  setFlag(locationCountry: string) {
    const currency = this.getCurrencyFromCode(locationCountry)
    this.flag = this.getFlagForCountry(currency)
  }

  setZipValidator() {
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
        if (code === 'CA') {
          zipControl.setValidators(ValidateCAZip)
        } else {
          zipControl.setValidators(ValidateUSZip)
        }
      })
  }

  private setLocationID(): void {
    const url = window.location.href
    const splitUrl = url.split('/')
    const endUrl = splitUrl[splitUrl.length - 1]
    const urlParams = this.activatedRoute.snapshot.params
    if (urlParams.locationID) {
      this.determineIfDeletable(urlParams.locationID)
    }
  }

  private async determineIfDeletable(locationID: string): Promise<void> {
    const hasNoProducts = await this.middleware.isLocationDeletable(locationID)
    this.canDelete.emit(hasNoProducts)
  }

  updateResourceFromEvent(event: any, field: string): void {
    this.updateResource.emit({ value: event.target.value, field })
  }

  handleAddressSelect(address) {
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
