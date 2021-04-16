import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  OnInit,
  SimpleChanges,
} from '@angular/core'
import { get as _get } from 'lodash'
import { FormGroup, FormControl } from '@angular/forms'
import { SupplierService } from '../supplier.service'
import {
  ListPage,
  HSSupplier,
  HeadStartSDK,
} from '@ordercloud/headstart-sdk'
import { DomSanitizer, SafeUrl } from '@angular/platform-browser'
import {
  faTimes,
  faSpinner,
  faExclamationCircle,
  faTimesCircle,
} from '@fortawesome/free-solid-svg-icons'
import { User, OcSupplierUserService, Buyer } from '@ordercloud/angular-sdk'
import { Buyers, Supplier, Suppliers } from 'ordercloud-javascript-sdk'
import { Router } from '@angular/router'
import { FileHandle } from '@app-seller/models/file-upload.types'
import {
  SupportedCurrencies,
  SupportedRates,
} from '@app-seller/models/currency-geography.types'
import { getAssetIDFromUrl } from '@app-seller/shared/services/assets/asset.helper'
@Component({
  selector: 'app-supplier-edit',
  templateUrl: './supplier-edit.component.html',
  styleUrls: ['./supplier-edit.component.scss'],
})
export class SupplierEditComponent implements OnInit, OnChanges {
  @Input()
  resourceForm: FormGroup
  @Input()
  filterConfig
  @Output()
  updateResource = new EventEmitter<any>()
  @Output()
  logoStaged = new EventEmitter<File>()
  @Output()
  updateList = new EventEmitter<Supplier>()
  @Input() set supplierEditable(value: HSSupplier) {
    this._supplierEditable = value
  }
  supplierUsers: ListPage<User>
  buyers: ListPage<Buyer>
  _supplierEditable: HSSupplier
  availableCurrencies: SupportedRates[] = []
  isCreatingNew: boolean
  countriesServicingOptions = []
  countriesServicingForm: FormGroup
  hasLogo = false
  logoUrl = ''
  stagedLogoUrl: SafeUrl = null
  logoLoading = false
  faTimes = faTimes
  faSpinner = faSpinner
  faExclamationCircle = faExclamationCircle
  faTimesCircle = faTimesCircle

  constructor(
    public supplierService: SupplierService,
    private sanitizer: DomSanitizer,
    private ocSupplierUserService: OcSupplierUserService,
    private router: Router,
  ) {
    this.isCreatingNew = this.supplierService.checkIfCreatingNew()
  }

  async ngOnInit(): Promise<void> {
    this.availableCurrencies = (
      await HeadStartSDK.ExchangeRates.GetRateList()
    ).Items
    this.availableCurrencies = this.availableCurrencies.filter((c) =>
      Object.values(SupportedCurrencies).includes(
        SupportedCurrencies[c.Currency]
      )
    )
  }

  async ngOnChanges(changes: SimpleChanges): Promise<void> {
    if (
      changes?.supplierEditable?.currentValue?.ID !==
      changes?.supplierEditable?.previousValue?.ID
    ) {
      await this.handleSelectedSupplierChange()
    }
  }

  async handleSelectedSupplierChange(): Promise<void> {
    !this.isCreatingNew &&
      (this.hasLogo = (this._supplierEditable?.xp as any)?.Image?.ThumbnailUrl && (this._supplierEditable?.xp as any)?.Image?.ThumbnailUrl !== '')
    !this.isCreatingNew &&
      (this.supplierUsers = await this.ocSupplierUserService
        .List(this._supplierEditable.ID)
        .toPromise())
    !this.router.url.startsWith('/my-supplier') &&
      (this.buyers = await Buyers.List())
    this.setUpSupplierCountrySelectIfNeeded()
  }

  setUpSupplierCountrySelectIfNeeded(): void {
    const indexOfCountriesServicingConfig = this.filterConfig.Filters?.findIndex(
      (s) => s.Path === 'xp.CountriesServicing'
    )
    if (indexOfCountriesServicingConfig > -1) {
      this.countriesServicingOptions = this.filterConfig.Filters[
        indexOfCountriesServicingConfig
      ].Items
      const formGroupCountriesServicing = {}
      this.countriesServicingOptions.forEach((option) => {
        formGroupCountriesServicing[option.Value] = new FormControl(
          (this._supplierEditable as any).xp?.CountriesServicing?.includes(
            option.Value
          ) || false
        )
      })
      this.countriesServicingForm = new FormGroup(formGroupCountriesServicing)
    }
  }

  updateCountriesServicing(event: any, country: string): void {
    const checked = event.target.checked
    let newCountriesSupported =
      this._supplierEditable.xp.CountriesServicing || []
    const isCountryInExistingValue = newCountriesSupported.includes(country)
    if (checked && !isCountryInExistingValue) {
      newCountriesSupported = [...newCountriesSupported, country]
    } else if (!checked && isCountryInExistingValue) {
      newCountriesSupported = newCountriesSupported.filter((n) => n !== country)
    }
    this.updateResource.emit({
      value: newCountriesSupported,
      field: 'xp.CountriesServicing',
    })
  }

  updateResourceFromEvent(event: any, field: string): void {
    if (field.startsWith('xp.ProductTypes')) {
      const form = this.resourceForm.getRawValue()
      const valueToArray = []
      Object.keys(form.ProductTypes).forEach((item) => {
        if (form.ProductTypes[item]) valueToArray.push(item)
      })
      this.updateResource.emit({
        field: 'xp.ProductTypes',
        value: valueToArray,
      })
    } else {
      let value
      if (field === 'xp.FreeShippingThreshold') {
        value = parseInt(event.target.value, 10)
          ? parseInt(event.target.value, 10)
          : event.target.value === ''
          ? 0
          : event.target.checked
          ? 0
          : null
      } else {
        value = ['Active', 'xp.SyncFreightPop'].includes(field)
          ? event.target.checked
          : event.target.value
      }
      this.updateResource.emit({ value, field })
    }
  }

  async dropFileUpload(event: FileHandle[]): Promise<void> {
    if (this.isCreatingNew) {
      this.logoStaged.emit(event[0].File)
      this.hasLogo = true
      this.stagedLogoUrl = this.sanitizer.bypassSecurityTrustUrl(
        window.URL.createObjectURL(event[0].File)
      )
    } else {
      this.logoLoading = true
      try {
        await this.uploadAsset(event[0].File)
      } catch (err) {
        this.hasLogo = false
        this.logoLoading = false
        throw err
      } finally {
        this.hasLogo = true
        this.logoLoading = false
      }
    }
  }

  // TODO: Some work to be done around 'isCreatingNew
  async manualFileUpload(event): Promise<void> {
    if (this.isCreatingNew) {
      this.logoStaged.emit(event?.target?.files[0])
      this.hasLogo = true
      this.stagedLogoUrl = this.sanitizer.bypassSecurityTrustUrl(
        window.URL.createObjectURL(event?.target?.files[0])
      )
    } else {
      this.logoLoading = true
      const file: File = event?.target?.files[0]
      if((this._supplierEditable?.xp as any)?.Image?.Url) {
        await HeadStartSDK.Assets.Delete(
          getAssetIDFromUrl((this._supplierEditable?.xp as any)?.Image?.Url)
        )
      }
      // Then upload logo asset
      try {
        await this.uploadAsset(file)
      } catch (err) {
        this.hasLogo = false
        this.logoLoading = false
        throw err
      } finally {
        this.hasLogo = true
        this.logoLoading = false
      }
    }
  }

  async uploadAsset(file: File): Promise<void> {
    const imgUrls = await HeadStartSDK.Assets.CreateImage({
      File: file
    })
    const patchObj = {
      xp: {
        Image: imgUrls
      }
    }
    await this.PatchAndUpdateList(patchObj)
  }

  async removeLogo(): Promise<void> {
    this.logoLoading = true
    try {
      if((this._supplierEditable.xp as any)?.Image?.Url) {
        await HeadStartSDK.Assets.Delete(getAssetIDFromUrl((this._supplierEditable.xp as any)?.Image?.Url))
        const patchObj = {
          xp: {
            Image: null
          }
        }
        await this.PatchAndUpdateList(patchObj)
      }
    } catch (err) {
      throw err
    } finally {
      this.hasLogo = false
      this.logoLoading = false
    }
  }

  async PatchAndUpdateList(patchObj: Partial<Supplier>) {
    const updatedSupplier = await Suppliers.Patch(this._supplierEditable?.ID, patchObj)
    this.updateList.emit(updatedSupplier);
    (this._supplierEditable.xp as any).Image = updatedSupplier.xp.Image; 
  }

  assignSupplierUser(email: string): void {
    this._supplierEditable?.xp?.NotificationRcpts
      ? null
      : (this._supplierEditable.xp.NotificationRcpts = [])
    const index = this._supplierEditable?.xp?.NotificationRcpts?.indexOf(email)
    if (index !== -1) {
      this.removeAddtlRcpt(index)
      return
    }
    const existingRcpts = this._supplierEditable?.xp?.NotificationRcpts || []
    const constructedEvent = { target: { value: [...existingRcpts, email] } }
    this.updateResourceFromEvent(constructedEvent, 'xp.NotificationRcpts')
  }

  assignBuyer(buyerID: string): void {
    this._supplierEditable?.xp?.BuyersServicing
      ? null
      : (this._supplierEditable.xp.BuyersServicing = [])
    const index = this._supplierEditable?.xp?.BuyersServicing?.indexOf(buyerID)
    if (index !== -1) {
      this.removeBuyerServicing(index)
      return
    }
    const existingBuyersToService =
      this._supplierEditable?.xp?.BuyersServicing || []
    const constructedEvent = {
      target: { value: [...existingBuyersToService, buyerID] },
    }
    this.updateResourceFromEvent(constructedEvent, 'xp.BuyersServicing')
  }

  removeAddtlRcpt(index: number): void {
    const copiedResource = JSON.parse(JSON.stringify(this._supplierEditable))
    const editedArr = copiedResource.xp?.NotificationRcpts.filter(
      (e) => e !== copiedResource.xp?.NotificationRcpts[index]
    )
    this.updateResourceFromEvent(
      { target: { value: editedArr } },
      'xp.NotificationRcpts'
    )
  }

  removeBuyerServicing(index: number): void {
    const copiedResource = JSON.parse(JSON.stringify(this._supplierEditable))
    const editedArr = copiedResource.xp?.BuyersServicing.filter(
      (e) => e !== copiedResource.xp?.BuyersServicing[index]
    )
    this.updateResourceFromEvent(
      { target: { value: editedArr } },
      'xp.BuyersServicing'
    )
  }

  isAssigned(val: string, field: string): boolean {
    if (field === 'NotificationRcpts') {
      return this._supplierEditable?.xp?.NotificationRcpts?.includes(val)
    } else {
      return this._supplierEditable?.xp?.BuyersServicing?.includes(val)
    }
  }
}
