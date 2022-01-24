import { Component, Input, OnInit } from '@angular/core'
import { faTimes, faListUl, faTh } from '@fortawesome/free-solid-svg-icons'
import {
  Spec,
  PriceBreak,
  SpecOption,
  Suppliers,
} from 'ordercloud-javascript-sdk'
import { PriceSchedule } from 'ordercloud-javascript-sdk'
import {
  HSVariant,
  HSMeProduct,
  HSSupplier,
  DocumentAsset,
} from '@ordercloud/headstart-sdk'
import { Observable } from 'rxjs'
import { SpecFormService } from '../spec-form/spec-form.service'
import { SuperHSProduct } from '@ordercloud/headstart-sdk'
import { FormGroup } from '@angular/forms'
import { ProductDetailService } from './product-detail.service'
import { ToastrService } from 'ngx-toastr'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { QtyChangeEvent, SpecFormEvent } from 'src/app/models/product.types'
import { CurrentUser } from 'src/app/models/profile.types'
import { ContactSupplierBody } from 'src/app/models/buyer.types'
import { ModalState } from 'src/app/models/shared.types'
import { SitecoreSendTrackingService } from 'src/app/services/sitecore-send/sitecore-send-tracking.service'
import { OrderType } from 'src/app/models/order.types'
import { TranslateService } from '@ngx-translate/core'
import { AppConfig } from 'src/app/models/environment.types'

@Component({
  templateUrl: './product-details.component.html',
  styleUrls: ['./product-details.component.scss'],
})
export class OCMProductDetails implements OnInit {
  // font awesome icons
  faTh = faTh
  faListUl = faListUl
  faTimes = faTimes

  _superProduct: SuperHSProduct
  specs: Spec[]
  _product: HSMeProduct
  priceSchedule: PriceSchedule
  priceBreaks: PriceBreak[]
  unitPrice: number
  attachments: DocumentAsset[] = []
  isOrderable = false
  quantity: number
  price: number
  percentSavings: number
  relatedProducts$: Observable<HSMeProduct[]>
  favoriteProducts: string[] = []
  qtyValid = true
  supplierNote: string
  userCurrency: string
  quoteFormModal = ModalState.Closed
  contactSupplierFormModal = ModalState.Closed
  currentUser: CurrentUser
  showRequestSubmittedMessage = false
  showContactSupplierFormSubmittedMessage = false
  submittedQuoteOrder: any
  showGrid = false
  isAddingToCart = false
  contactRequest: ContactSupplierBody
  specForm: FormGroup
  isInactiveVariant: boolean
  _disabledVariants: HSVariant[]
  variant: HSVariant
  variantInventory: number
  _productSupplier: HSSupplier
  isQuoteAnonUser = false
  quoteContactEmail = ''
  constructor(
    private specFormService: SpecFormService,
    private context: ShopperContextService,
    private productDetailService: ProductDetailService,
    private toastrService: ToastrService,
    private send: SitecoreSendTrackingService,
    private translate: TranslateService,
    private appConfig: AppConfig
  ) {}

  @Input() set product(superProduct: SuperHSProduct) {
    this._superProduct = superProduct
    this._product = superProduct.Product
    this.attachments = superProduct?.Product?.xp?.Documents
    this.priceBreaks = superProduct.PriceSchedule?.PriceBreaks
    this.unitPrice =
      this.priceBreaks && this.priceBreaks.length
        ? this.priceBreaks[0].Price
        : null
    this.isOrderable = !!superProduct.PriceSchedule
    this.supplierNote = this._product.xp && this._product.xp.Note
    this.specs = superProduct.Specs
    if (this._product.DefaultSupplierID !== null) {
      this.setSupplier(this._product.DefaultSupplierID)
    } else {
      this.setSellerQuoteContactEmail()
    }
    this.setPageTitle()
    this.populateInactiveVariants(superProduct)
    this.showGrid = superProduct?.PriceSchedule?.UseCumulativeQuantity
    this.send.viewProduct(superProduct.Product)
    this.isQuoteAnonUser =
      this.isQuoteProduct() && this.context.currentUser.isAnonymous()
  }

  ngOnInit(): void {
    this.calculatePrice()
    this.currentUser = this.context.currentUser.get()
    this.userCurrency = this.currentUser.Currency
    this.context.currentUser.onChange(
      (user) => (this.favoriteProducts = user.FavoriteProductIDs)
    )
  }

  async setSupplier(supplierID: string): Promise<void> {
    this._productSupplier = await Suppliers.Get<HSSupplier>(supplierID)
    if (this._productSupplier?.xp?.SupportContact?.Email) {
      this.quoteContactEmail = this._productSupplier?.xp?.SupportContact?.Email
    } else if (this._productSupplier?.xp?.NotificationRcpts?.length) {
      this.quoteContactEmail = this._productSupplier.xp.NotificationRcpts[0]
    } else {
      console.error('No email available for this supplier')
    }
  }

  setSellerQuoteContactEmail(): void {
    this.quoteContactEmail = this.appConfig.sellerQuoteContactEmail
  }

  setPageTitle(): void {
    this.context.router.setPageTitle(this._superProduct.Product.Name)
  }

  onSpecFormChange(event: SpecFormEvent): void {
    this.specForm = event.form
    if (
      this._superProduct?.Product?.Inventory?.Enabled &&
      this._superProduct?.Product?.Inventory?.VariantLevelTracking
    ) {
      this.variantInventory = this.getVariantInventory()
    }
    this.calculatePrice()
  }

  getVariantInventory(): number {
    let specCombo = ''
    let specOptions: SpecOption[] = []
    this._superProduct?.Specs?.filter((s) => s.DefinesVariant).forEach((s) =>
      s.Options.forEach((o) => (specOptions = specOptions.concat(o)))
    )
    for (let i = 0; i < this.specForm.value.ctrls.length; i++) {
      const matchingOption = specOptions.find(
        (o) => o.Value === this.specForm.value.ctrls[i]
      )
      if (matchingOption) {
        i === 0
          ? (specCombo += matchingOption?.ID)
          : (specCombo += `-${matchingOption?.ID}`)
      }
    }
    this.variant = this._superProduct?.Variants?.find(
      (v) => v.xp?.SpecCombo === specCombo
    )
    return this._superProduct?.Variants?.find(
      (v) => v.xp?.SpecCombo === specCombo
    )?.Inventory?.QuantityAvailable
  }

  onSelectionInactive(event: boolean): void {
    this.isInactiveVariant = event
  }

  populateInactiveVariants(superProduct: SuperHSProduct): void {
    this._disabledVariants = []
    superProduct.Variants?.forEach((variant) => {
      if (!variant.Active) {
        this._disabledVariants.push(variant)
      }
    })
  }

  toggleGrid(showGrid: boolean): void {
    this.showGrid = showGrid
  }

  qtyChange(event: QtyChangeEvent): void {
    this.qtyValid = event.valid
    if (event.valid) {
      this.quantity = event.qty
      this.calculatePrice()
    }
  }

  calculatePrice(): void {
    this.price = this.productDetailService.getProductPrice(
      this.priceBreaks,
      this.specs,
      this.quantity,
      this.specForm
    )
    if (this.priceBreaks?.length) {
      const basePrice = this.quantity * this.priceBreaks[0].Price
      this.percentSavings = this.productDetailService.getPercentSavings(
        this.price,
        basePrice
      )
    }
  }

  async addToCart(): Promise<void> {
    this.isAddingToCart = true
    try {
      const currentOrder = this.context.order.get()
      if (this._product.xp.ProductType === 'Quote') {
        if (
          currentOrder?.xp?.OrderType == 'Standard' &&
          currentOrder?.LineItemCount > 0
        ) {
          const existingCartQuoteError = this.translate.instant(
            'PRODUCTS.DETAILS.QUOTE_EXISTING_CART_ERROR'
          )
          this.toastrService.error(existingCartQuoteError)
          this.isAddingToCart = false
          return
        } else {
          currentOrder.xp.OrderType = OrderType.Quote
          currentOrder.xp.QuoteBuyerContactEmail = this.currentUser?.Email
          currentOrder.xp.QuoteSellerContactEmail = this.quoteContactEmail
          currentOrder.xp.QuoteSupplierID = this._productSupplier?.ID
          currentOrder.xp.QuoteOrderInfo = {
            FirstName: this.currentUser?.FirstName,
            LastName: this.currentUser?.LastName,
            Phone: this.currentUser?.Phone,
            Email: this.currentUser?.Email,
            BuyerLocation: '',
          }
          await this.context.order.patch(currentOrder)
        }
      } else {
        if (
          currentOrder?.xp?.OrderType == 'Quote' &&
          currentOrder?.LineItemCount > 0
        ) {
          const existingCartStandardError = this.translate.instant(
            'PRODUCTS.DETAILS.QUOTE_STANDARD_EXISTING_CART_ERROR'
          ) as string
          this.toastrService.error(existingCartStandardError)
          this.isAddingToCart = false
          return
        }
      }
      await this.context.order.cart.add({
        ProductID: this._product.ID,
        Quantity: this.quantity,
        Specs: this.specFormService.getLineItemSpecs(this.specs, this.specForm),
        xp: {
          ImageUrl: this.specFormService.getLineItemImageUrl(
            this._superProduct.Product?.xp?.Images,
            this._superProduct.Specs,
            this.specForm
          ),
        },
      })
    } catch (err) {
      this.toastrService.error('Something went wrong')
      console.log(err)
    } finally {
      this.isAddingToCart = false
    }
  }

  getPriceBreakRange(index: number): string {
    if (!this.priceBreaks.length) return ''
    const indexOfNextPriceBreak = index + 1
    if (indexOfNextPriceBreak < this.priceBreaks.length) {
      return `${this.priceBreaks[index].Quantity} - ${
        this.priceBreaks[indexOfNextPriceBreak].Quantity - 1
      }`
    } else {
      return `${this.priceBreaks[index].Quantity}+`
    }
  }

  isFavorite(): boolean {
    return this.favoriteProducts.includes(this._product.ID)
  }

  setIsFavorite(isFav: boolean): void {
    this.context.currentUser.setIsFavoriteProduct(isFav, this._product.ID)
  }

  setActiveSupplier(supplierId: string): void {
    this.context.router.toProductList({
      activeFacets: { supplier: supplierId.toLowerCase() },
    })
  }

  openQuoteForm(): void {
    this.quoteFormModal = ModalState.Open
  }

  openContactSupplierForm(): void {
    this.contactSupplierFormModal = ModalState.Open
  }

  isQuoteProduct(): boolean {
    return this._product.xp.ProductType === 'Quote'
  }

  dismissQuoteForm(): void {
    this.quoteFormModal = ModalState.Closed
  }

  dismissContactSupplierForm(): void {
    this.contactSupplierFormModal = ModalState.Closed
  }

  async submitContactSupplierForm(formData: any): Promise<void> {
    this.contactRequest = { Product: this._product, BuyerRequest: formData }
    try {
      await this.context.currentUser.submitContactSupplierForm(
        this.contactRequest
      )
      this.contactSupplierFormModal = ModalState.Closed
      this.showContactSupplierFormSubmittedMessage = true
    } catch (ex) {
      this.contactSupplierFormModal = ModalState.Closed
      throw ex
    }
  }

  toOrderDetail(): void {
    this.context.router.toMyOrderDetails(this.submittedQuoteOrder.ID)
  }

  hasNoOpenTextSpecs(): boolean {
    return !this.specs.some((spec) => spec?.AllowOpenText)
  }
}
