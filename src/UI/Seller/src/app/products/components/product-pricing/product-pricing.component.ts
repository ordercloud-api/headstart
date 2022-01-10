import { Component, Input, Output, EventEmitter } from '@angular/core'
import { PriceSchedule, OcBuyerService } from '@ordercloud/angular-sdk'
import { ToastrService } from 'ngx-toastr'
import { FormControl } from '@angular/forms'
import { BuyerTempService } from '@app-seller/shared/services/middleware-api/buyer-temp.service'
import { CatalogsTempService } from '@app-seller/shared/services/middleware-api/catalogs-temp.service'
import {
  SuperHSProduct,
  HSBuyer,
  HeadStartSDK,
  HSPriceSchedule,
} from '@ordercloud/headstart-sdk'
import { faExclamationCircle } from '@fortawesome/free-solid-svg-icons'
import { SupportedRates } from '@app-seller/models/currency-geography.types'
import { ResourceUpdate } from '@app-seller/models/shared.types'
import { HSBuyerPriceMarkup } from '@app-seller/models/buyer.types'

@Component({
  selector: 'product-pricing-component',
  templateUrl: './product-pricing.component.html',
  styleUrls: ['./product-pricing.component.scss'],
})
export class ProductPricingComponent {
  @Input()
  readonly = false
  @Input()
  productForm: FormControl
  @Input()
  supplierCurrency: SupportedRates
  @Input()
  sellerCurrency: SupportedRates
  @Input()
  isRequired: boolean
  @Input()
  set superHSProductEditable(value: SuperHSProduct) {
    this.setData(value)
    if (value && this.readonly) {
      this.setUpBuyers()
      this.setUpExchangeRate()
      this.buyerMarkedUpSupplierPrices = this.getBuyerDisplayOfSupplierPriceSchedule()
    }
  }

  @Output()
  updateProduct = new EventEmitter<{ field: string; value: any }>()
  faExclamationCircle = faExclamationCircle
  supplierPriceSchedule: PriceSchedule
  buyerMarkedUpSupplierPrices: PriceSchedule

  // defaulting to they are the same currency
  supplierToSellerCurrencyRate = 1
  superProduct

  buyers: HSBuyer[] = []
  selectedBuyerIndex = 0
  selectedSuperHSBuyer: HSBuyerPriceMarkup

  isUsingPriceOverride = false
  areChangesToBuyerVisibility = false

  emptyPriceSchedule
  isSavedOverride = false
  overridePriceScheduleEditable
  overridePriceScheduleStatic

  constructor(
    private toasterService: ToastrService,
    private ocBuyerService: OcBuyerService,
    private catalogsTempService: CatalogsTempService,
    private buyerTempService: BuyerTempService
  ) {}

  setData(value: SuperHSProduct): void {
    this.superProduct = value
    if (
      value.Product?.xp?.ProductType === 'Quote' &&
      value.PriceSchedule.PriceBreaks === null
    ) {
      this.superProduct.PriceSchedule.PriceBreaks = [
        { Price: null, Quantity: null },
      ]
    }
    this.buildEmptyPriceSchedule(value)
    this.isSavedOverride = false
    this.overridePriceScheduleEditable = this.emptyPriceSchedule
    this.overridePriceScheduleStatic = this.emptyPriceSchedule

    if (value) {
      this.supplierPriceSchedule = JSON.parse(
        JSON.stringify(value?.PriceSchedule)
      )
    }
  }

  buildEmptyPriceSchedule(value: SuperHSProduct): void {
    this.emptyPriceSchedule = {
      UseCumulativeQuantity: value?.PriceSchedule?.UseCumulativeQuantity,
      ApplyTax: value?.PriceSchedule?.ApplyTax,
      ApplyShipping: value?.PriceSchedule?.ApplyShipping,
      RestrictedQuantity: value?.PriceSchedule?.RestrictedQuantity,
      MinQuantity: value?.PriceSchedule?.MinQuantity,
      MaxQuantity: value?.PriceSchedule?.MaxQuantity,
      PriceBreaks: [
        {
          Price: 0,
          Quantity: 1,
        },
      ],
    } as PriceSchedule
  }

  async setUpExchangeRate(): Promise<void> {
    if (this.supplierCurrency !== this.sellerCurrency) {
      const usdExchangeRates = await HeadStartSDK.ExchangeRates.Get(
        this.sellerCurrency.Currency as any
      )
      if(this.supplierCurrency){
        const supplierToSellerExchangeRate = usdExchangeRates.Items.find(
          (r) => r.Currency === this.supplierCurrency.Currency
        )
        this.supplierToSellerCurrencyRate = supplierToSellerExchangeRate.Rate
      }
    }
  }

  getBuyerPercentMarkupPrice(supplierPrice: number): number {
    const markupMultiplier =
      (this.selectedSuperHSBuyer?.Markup?.Percent || 0) / 100 + 1
    return supplierPrice * markupMultiplier
  }

  updateSupplierPriceSchedule(event: PriceSchedule): void {
    this.updateProduct.emit({ field: 'PriceSchedule', value: event })
  }

  updateOverridePriceSchedule(event: PriceSchedule): void {
    this.overridePriceScheduleEditable = event
    this.diffBuyerVisibility()
  }

  diffBuyerVisibility(): void {
    if (!this.isSavedOverride && this.isUsingPriceOverride) {
      this.areChangesToBuyerVisibility = true
    } else if (this.isSavedOverride && !this.isUsingPriceOverride) {
      this.areChangesToBuyerVisibility = true
    } else {
      this.areChangesToBuyerVisibility =
        JSON.stringify(this.overridePriceScheduleEditable) !==
        JSON.stringify(this.overridePriceScheduleStatic)
    }
  }

  handleDiscardBuyerPricingChanges(): void {
    this.isUsingPriceOverride = this.isSavedOverride
    this.resetOverridePriceSchedules(this.overridePriceScheduleStatic)
  }

  async handleSaveBuyerPricing(): Promise<void> {
    const productID = this.superProduct.Product.ID
    const buyerID = this.selectedSuperHSBuyer.Buyer.ID
    this.overridePriceScheduleEditable.Name =
      this.superProduct.Product.Name + ' Seller Override'
    if (!this.isSavedOverride && this.isUsingPriceOverride) {
      const newPriceSchedule = await this.catalogsTempService.CreatePricingOverride(
        productID,
        buyerID,
        this.overridePriceScheduleEditable
      )
      this.isSavedOverride = true
      this.resetOverridePriceSchedules(newPriceSchedule)
    } else if (this.isSavedOverride && !this.isUsingPriceOverride) {
      await this.catalogsTempService.DeletePricingOverride(productID, buyerID)
      this.isSavedOverride = false
      this.resetOverridePriceSchedules(this.emptyPriceSchedule)
    } else {
      const newPriceSchedule = await this.catalogsTempService.UpdatePricingOverride(
        productID,
        buyerID,
        this.overridePriceScheduleEditable
      )
      this.isSavedOverride = true
      this.resetOverridePriceSchedules(newPriceSchedule)
    }
  }

  getBuyerDisplayOfSupplierPriceSchedule(): PriceSchedule {
    const priceScheduleCopy = JSON.parse(
      JSON.stringify(this.supplierPriceSchedule)
    )
    priceScheduleCopy.PriceBreaks = priceScheduleCopy.PriceBreaks.map((b) => {
      b.Price = this.getBuyerPercentMarkupPrice(b.Price)
      return b
    })
    return priceScheduleCopy
  }

  setIsUsingPriceOverride(isUsingPriceOverride: boolean) {
    this.isUsingPriceOverride = isUsingPriceOverride
    this.diffBuyerVisibility()
  }

  async getPriceScheduleOverrides(): Promise<void> {
    try {
      const override = await this.catalogsTempService.GetPricingOverride(
        this.superProduct.Product.ID,
        this.selectedSuperHSBuyer.Buyer.ID
      )
      this.resetOverridePriceSchedules(override)
      this.isSavedOverride = true
      this.isUsingPriceOverride = true
    } catch (ex) {
      if (ex?.error?.Errors[0].ErrorCode === 'NotFound') {
        this.isSavedOverride = false
        this.isUsingPriceOverride = false
        this.resetOverridePriceSchedules(this.emptyPriceSchedule)
      } else {
        //  if it's anything other than a not found error throw it
        // not found just means no override exists
        throw ex
      }
    }
  }

  resetOverridePriceSchedules(priceSchedule: HSPriceSchedule): void {
    this.overridePriceScheduleEditable = JSON.parse(
      JSON.stringify(priceSchedule)
    )
    this.overridePriceScheduleStatic = JSON.parse(JSON.stringify(priceSchedule))
    this.diffBuyerVisibility()
  }

  async setUpBuyers(): Promise<void> {
    const buyersResponse = await this.ocBuyerService
      .List({ pageSize: 100 })
      .toPromise()
    this.buyers = buyersResponse.Items
    await this.selectBuyer(this.buyers[0])
  }

  async selectBuyer(buyer: HSBuyer): Promise<void> {
    const superBuyer = await this.buyerTempService.get(buyer.ID)
    this.selectedSuperHSBuyer = superBuyer
    this.buyerMarkedUpSupplierPrices = this.getBuyerDisplayOfSupplierPriceSchedule()
    this.getPriceScheduleOverrides()
  }
}
