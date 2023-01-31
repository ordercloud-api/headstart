import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
} from '@angular/core'
import { get as _get } from 'lodash'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import {
  Address,
  SupplierAddresses,
  AdminAddresses,
  SpecOption,
  Product,
  Tokens,
} from 'ordercloud-javascript-sdk'
import {
  UntypedFormGroup,
  UntypedFormControl,
  Validators,
  AbstractControl,
} from '@angular/forms'
import { Router } from '@angular/router'
import { SafeUrl } from '@angular/platform-browser'
import {
  faCircle,
  faHeart,
  faAsterisk,
  faCheckCircle,
  faTimesCircle,
  faExclamationCircle,
} from '@fortawesome/free-solid-svg-icons'
import { ProductService } from '@app-seller/products/product.service'
import {
  SuperHSProduct,
  ListPage,
  HeadStartSDK,
  ImageAsset,
  DocumentAsset,
  HSProduct,
  TaxCategorization,
  TaxCategorizationResponse,
} from '@ordercloud/headstart-sdk'
import { Location } from '@angular/common'
import { TabIndexMapper, setProductEditTab } from './tab-mapper'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import {
  ValidateMinMax,
  ValidateNoSpecialCharactersAndSpaces,
} from '../../../validators/validators'
import { takeWhile } from 'rxjs/operators'
import { SizerTiersDescriptionMap } from './size-tier.constants'
import { ToastrService } from 'ngx-toastr'
import { Subscription } from 'rxjs'
import { SupportedRates } from '@app-seller/models/currency-geography.types'
import { FileHandle } from '@app-seller/models/file-upload.types'
import { UserContext } from '@app-seller/models/user.types'
import { AssetService } from '@app-seller/shared/services/assets/asset.service'
import { getProductMediumImageUrl } from '@app-seller/shared/services/assets/asset.helper'
import { TranslateService } from '@ngx-translate/core'
import { DeleteFileEvent } from '../product-image-upload/product-image-upload.component'
import { ProductType } from '@ordercloud/headstart-sdk/dist/models/ProductType'
import { NgbNavChangeEvent } from '@ng-bootstrap/ng-bootstrap'

@Component({
  selector: 'app-product-edit',
  templateUrl: './product-edit.component.html',
  styleUrls: ['./product-edit.component.scss'],
})
export class ProductEditComponent implements OnInit, OnDestroy {
  @Input()
  set product(_product: Product) {
    if (_product.ID) {
      void this.handleSelectedProductChange(_product)
    } else {
      this.createProductForm(this.productService.emptyResource)
      this._superHSProductEditable = this.productService.emptyResource
      this._superHSProductStatic = this.productService.emptyResource
    }
  }
  @Input() productForm: UntypedFormGroup
  @Input() filterConfig
  @Output() updateResource = new EventEmitter<any>()
  @Output() updateList = new EventEmitter<Product>()
  @Input() isCreatingNew: boolean
  @Input() dataIsSaving = false
  @Input() userContext: UserContext
  @Input() readonly
  addresses: ListPage<Address>
  isSellerUser = false
  images: ImageAsset[] = []
  faCircle = faCircle
  faHeart = faHeart
  faAsterisk = faAsterisk
  faTimesCircle = faTimesCircle
  faCheckCircle = faCheckCircle
  faExclamationCircle = faExclamationCircle
  _superHSProductStatic: SuperHSProduct
  _superHSProductEditable: SuperHSProduct
  supplierCurrency: SupportedRates
  sellerCurrency: SupportedRates
  _exchangeRates: SupportedRates[]
  areChanges = false
  taxCodes: TaxCategorizationResponse
  productType: ProductType
  shippingAddress: any
  variantsValid = true
  specsValid = true
  editSpecs = false
  fileType: string
  stagedImages: FileHandle[] = []
  stagedDocuments: FileHandle[] = []
  documents: DocumentAsset[] = []
  documentName: string
  availableProductTypes: ProductType[] = []
  availableSizeTiers = SizerTiersDescriptionMap
  active: number
  alive = true
  sizeTierSubscription: Subscription
  inventoryValidatorSubscription: Subscription

  constructor(
    private router: Router,
    private location: Location,
    private currentUserService: CurrentUserService,
    private productService: ProductService,
    private appAuthService: AppAuthService,
    private toastrService: ToastrService,
    private assetService: AssetService,
    private translate: TranslateService
  ) {}

  async ngOnInit(): Promise<void> {
    // TODO: Eventually move to a resolve so that they are there before the component instantiates.
    this.isCreatingNew = this.productService.checkIfCreatingNew()
    this.getAddresses()
    const getTaxCategoriesPromise = this.initTaxCategorization()
    this.isSellerUser = this.userContext.UserType === 'SELLER'
    await this.getAvailableProductTypes()
    await getTaxCategoriesPromise
    this.setProductEditTab()
  }

  setResourceType(): void {
    const routeUrl = this.router.routerState.snapshot.url
    const splitUrl = routeUrl.split('/')
    if (this.productService.checkIfCreatingNew()) {
      const productTypeFromUrl = splitUrl[splitUrl.length - 1]
        .split('-')
        .map((s) => s[0].toUpperCase() + s.slice(1))
        .join('')
      this.productType = productTypeFromUrl as ProductType
    }
    this.productForm.controls.ProductType.setValue(this.productType)
    this.handleUpdateProduct(
      { target: { value: this.productForm.controls.ProductType.value } },
      'Product.xp.ProductType'
    )
  }

  setProductEditTab(): void {
    const productDetailSection = this.isCreatingNew
      ? 'undefined'
      : this.router.url.split('/')[3]
    this.active = setProductEditTab(productDetailSection)
  }

  tabChanged(event: NgbNavChangeEvent, productID: string): void {
    const nextIndex = Number(event.nextId)
    if (productID === null || this.isCreatingNew) return
    const newLocation =
      nextIndex === 0
        ? `products/${productID}`
        : `products/${productID}/${TabIndexMapper[nextIndex] as string}`
    this.location.replaceState(newLocation)
  }

  async getAddresses(): Promise<void> {
    if (this.userContext.Me.Supplier) {
      this.addresses = await SupplierAddresses.List(
        this.userContext.Me.Supplier.ID
      )
    } else if (this.userContext.Me.Seller) {
      this.addresses = await AdminAddresses.List()
    }
  }

  async getTaxCodes(searchTerm = ''): Promise<TaxCategorizationResponse> {
    const taxCodes = await HeadStartSDK.TaxCategories.ListTaxCategories(
      searchTerm
    )
    const taxCodeInput = this.productForm.controls.TaxCode
    if (taxCodes.ProductsShouldHaveTaxCodes) {
      taxCodeInput.setValidators([Validators.required])
    } else {
      taxCodeInput.setValidators(null)
      taxCodeInput.updateValueAndValidity()
    }
    return taxCodes
  }

  async refreshProductData(superProduct: SuperHSProduct): Promise<void> {
    // If a seller, and not editing the product, grab the currency from the product xp.
    this.supplierCurrency = this._exchangeRates?.find(
      (r) => r.Currency === superProduct?.Product?.xp?.Currency
    )
    // copying to break reference bugs
    this._superHSProductStatic = JSON.parse(JSON.stringify(superProduct))
    this._superHSProductEditable = JSON.parse(JSON.stringify(superProduct))
    if (!this._superHSProductEditable?.Product?.xp?.UnitOfMeasure) {
      this._superHSProductEditable.Product.xp.UnitOfMeasure = {
        Unit: null,
        Qty: null,
      }
    }

    this.taxCodes = await this.getTaxCodes()

    this.documents = this._superHSProductEditable.Product?.xp.Documents
    this.images = this._superHSProductEditable.Product?.xp?.Images
    this.productType = this._superHSProductEditable.Product?.xp?.ProductType
    this.createProductForm(this._superHSProductEditable)
    if (
      this.userContext?.UserType === 'SELLER' &&
      superProduct.Product.DefaultSupplierID
    ) {
      this.addresses = await SupplierAddresses.List(
        this._superHSProductEditable?.Product?.DefaultSupplierID
      )
      if (superProduct.Product?.ShipFromAddressID)
        this.shippingAddress = await SupplierAddresses.Get(
          this._superHSProductEditable.Product.OwnerID,
          this._superHSProductEditable.Product.ShipFromAddressID
        )
    } else if (
      this.userContext?.UserType === 'SELLER' &&
      !superProduct.Product.DefaultSupplierID
    ) {
      this.addresses = await AdminAddresses.List()
      if (superProduct.Product?.ShipFromAddressID)
        this.shippingAddress = await AdminAddresses.Get(
          this._superHSProductEditable.Product.ShipFromAddressID
        )
    }
    this.isCreatingNew = this.productService.checkIfCreatingNew()
    this.checkForChanges()
  }

  createProductForm(superHSProduct: SuperHSProduct): void {
    if (superHSProduct.Product) {
      this.productForm = new UntypedFormGroup(
        {
          Active: new UntypedFormControl(superHSProduct.Product.Active),
          Name: new UntypedFormControl(superHSProduct.Product.Name, [
            Validators.required,
            Validators.maxLength(100),
          ]),
          ID: new UntypedFormControl(
            superHSProduct.Product.ID,
            ValidateNoSpecialCharactersAndSpaces
          ),
          Description: new UntypedFormControl(
            superHSProduct.Product.Description,
            Validators.maxLength(2000)
          ),
          QuantityMultiplier: new UntypedFormControl(
            superHSProduct.Product.QuantityMultiplier
          ),
          ShipFromAddressID: new UntypedFormControl(
            superHSProduct.Product.ShipFromAddressID,
            Validators.required
          ),
          ShipHeight: new UntypedFormControl(superHSProduct.Product.ShipHeight),
          ShipWidth: new UntypedFormControl(superHSProduct.Product.ShipWidth),
          ShipLength: new UntypedFormControl(superHSProduct.Product.ShipLength),
          ShipWeight: new UntypedFormControl(superHSProduct.Product.ShipWeight, [
            Validators.required,
            Validators.min(0),
          ]),
          Price: new UntypedFormControl(
            _get(superHSProduct.PriceSchedule, 'PriceBreaks[0].Price', null),
            Validators.required
          ),
          MinQuantity: new UntypedFormControl(
            superHSProduct.PriceSchedule?.MinQuantity,
            Validators.min(1)
          ),
          MaxQuantity: new UntypedFormControl(
            superHSProduct.PriceSchedule?.MaxQuantity,
            Validators.min(1)
          ),
          UseCumulativeQuantity: new UntypedFormControl(
            superHSProduct.PriceSchedule?.UseCumulativeQuantity
          ),
          Note: new UntypedFormControl(
            _get(superHSProduct.Product, 'xp.Note'),
            Validators.maxLength(140)
          ),
          ProductType: new UntypedFormControl(
            _get(superHSProduct.Product, 'xp.ProductType'),
            Validators.required
          ),
          QuantityAvailable: new UntypedFormControl(
            superHSProduct.Product?.Inventory?.QuantityAvailable,
            null
          ),
          InventoryEnabled: new UntypedFormControl(
            _get(superHSProduct.Product, 'Inventory.Enabled')
          ),
          VariantLevelTracking: new UntypedFormControl(
            _get(superHSProduct.Product, 'Inventory.VariantLevelTracking'),
            null
          ),
          Returnable: new UntypedFormControl(
            _get(superHSProduct.Product, 'Returnable'),
            []
          ),
          OrderCanExceed: new UntypedFormControl(
            _get(superHSProduct.Product, 'Inventory.OrderCanExceed')
          ),
          TaxCode: new UntypedFormControl(
            _get(superHSProduct.Product, 'xp.Tax.Code', null)
          ),
          UnitOfMeasureUnit: new UntypedFormControl(
            _get(superHSProduct.Product, 'xp.UnitOfMeasure.Unit'),
            Validators.required
          ),
          SizeTier: new UntypedFormControl(
            _get(superHSProduct.Product, 'xp.SizeTier'),
            Validators.required
          ),
          UnitOfMeasureQty: new UntypedFormControl(
            _get(superHSProduct.Product, 'xp.UnitOfMeasure.Qty'),
            Validators.required
          ),
          FreeShipping: new UntypedFormControl(
            _get(superHSProduct.Product, 'xp.FreeShipping')
          ),
          FreeShippingMessage: new UntypedFormControl(
            _get(superHSProduct.Product, 'xp.FreeShippingMessage') ||
              'Free Shipping'
          ),
        },
        { validators: ValidateMinMax }
      )
      this.setInventoryValidator()
      this.setVariantLevelTrackingDisabledSubscription()
      this.setSizeTierValidators()
      this.setNonRequiredFields()
      this.setResourceType()
    }
  }

  setInventoryValidator(): void {
    const quantityControl = this.productForm.get('QuantityAvailable')
    const variantLevelTrackingControl = this.productForm.get(
      'VariantLevelTracking'
    )
    this.inventoryValidatorSubscription = this.productForm
      .get('InventoryEnabled')
      .valueChanges.pipe(takeWhile(() => this.alive))
      .subscribe((inventory) => {
        if (inventory && variantLevelTrackingControl.value === false) {
          quantityControl.setValidators([
            Validators.required,
            Validators.min(1),
          ])
        } else {
          quantityControl.setValidators(null)
        }
        quantityControl.updateValueAndValidity()
      })
  }

  setVariantLevelTrackingDisabledSubscription(): void {
    const variantLevelTrackingControl = this.productForm.get(
      'VariantLevelTracking'
    )
    // Set initial state to disabled
    if (this.isCreatingNew) {
      variantLevelTrackingControl.disable()
    }
    this.productForm
      .get('ID')
      .valueChanges.pipe(takeWhile(() => this.alive))
      .subscribe((id) => {
        if (id) {
          variantLevelTrackingControl.enable()
        } else {
          variantLevelTrackingControl.disable()
        }
      })
  }

  setSizeTierValidators(): void {
    const sizeTier = this.productForm.get('SizeTier')
    const shipLength = this.productForm.get('ShipLength')
    const shipHeight = this.productForm.get('ShipHeight')
    const shipWidth = this.productForm.get('ShipWidth')
    this.sizeTierSubscription = sizeTier.valueChanges
      .pipe(takeWhile(() => this.alive))
      .subscribe((sizeTier) => {
        if (sizeTier === 'G') {
          shipLength.setValidators([Validators.required])
          shipHeight.setValidators([Validators.required])
          shipWidth.setValidators([Validators.required])
        } else {
          shipLength.setValidators(null)
          shipLength.setValue(null)
          shipHeight.setValidators(null)
          shipHeight.setValue(null)
          shipWidth.setValidators(null)
          shipWidth.setValue(null)
        }
        shipLength.updateValueAndValidity()
        shipHeight.updateValueAndValidity()
        shipWidth.updateValueAndValidity()
      })
  }

  setNonRequiredFields(): void {
    const optionalFieldsArray = ['ShipWeight', 'ShipFromAddressID', 'Price']
    const optionalControls = optionalFieldsArray.map((item) =>
      this.productForm.get(item)
    )
    const taxCodeInput = this.productForm.get('TaxCode')
    this.productForm
      .get('ProductType')
      .valueChanges.pipe(takeWhile(() => this.alive))
      .subscribe((productType: ProductType) => {
        if (productType === 'Quote') {
          optionalControls.forEach((control) => {
            control.setValidators(null)
            control.updateValueAndValidity()
          })
          taxCodeInput.setValue(null)
          taxCodeInput.updateValueAndValidity()
        } else {
          optionalControls.forEach((control) => {
            control.setValidators(Validators.required)
            control.updateValueAndValidity()
          })

          // Tax code is an exception because it may additionally be optional on standard products
          // if and only if the tax integration is not set up to return tax codes
          if (this.taxCodes?.ProductsShouldHaveTaxCodes) {
            taxCodeInput.setValidators(Validators.required)
            taxCodeInput.updateValueAndValidity()
          } else {
            taxCodeInput.setValidators(null)
            taxCodeInput.updateValueAndValidity()
          }
        }
      })
  }

  isRequired(control: string): boolean {
    const theControl = this.productForm.get(control)
    if (theControl.validator === null) return false
    const validator = this.productForm
      .get(control)
      .validator({} as AbstractControl)
    return validator && validator.required
  }

  productDetailsTabIsValid(): boolean {
    return (
      this.isShippingValid() &&
      this.unitOfMeasureValid() &&
      this.productForm.controls.Name.valid &&
      this.productForm.controls.TaxCode.valid
    )
  }

  isShippingValid(): boolean {
    if (this.productForm.controls.ProductType.value === 'Quote') return true

    const sizeTier = this.productForm.controls.SizeTier.value

    const formControls = this.productForm.controls
    const hasAlwaysRequiredFields =
      formControls.ShipFromAddressID.valid &&
      formControls.SizeTier.valid &&
      formControls.ShipWeight.valid &&
      sizeTier

    if (!hasAlwaysRequiredFields) {
      return false
    }

    if (sizeTier === 'G') {
      const hasDimensions =
        formControls.ShipWidth.valid &&
        formControls.ShipHeight.valid &&
        formControls.ShipLength.valid
      return hasDimensions
    } else {
      return true
    }
  }

  unitOfMeasureValid(): boolean {
    return (
      this.isCreatingNew &&
      this.isRequired('UnitOfMeasureQty') &&
      this.isRequired('UnitOfMeasureUnit') &&
      this.productForm.controls.UnitOfMeasureUnit.valid &&
      this.productForm.controls.UnitOfMeasureQty.valid
    )
  }

  async getAvailableProductTypes(): Promise<void> {
    const supplier = await this.currentUserService.getMySupplier()
    this.availableProductTypes = supplier?.xp?.ProductTypes || [
      'Standard',
      'Quote',
      'Bundle',
    ]
  }

  async handleSave(): Promise<void> {
    if (this.isCreatingNew) {
      await this.createNewProduct()
    } else {
      void this.updateProduct()
    }
  }

  async handleDelete(): Promise<void> {
    await HeadStartSDK.Products.Delete(this._superHSProductStatic.Product.ID)
    void this.router.navigateByUrl('/products')
  }

  handleDiscardChanges(): void {
    this.stagedImages = []
    this.stagedDocuments = []
    this._superHSProductEditable = this._superHSProductStatic
    void this.refreshProductData(this._superHSProductStatic)
  }

  async createNewProduct(): Promise<void> {
    try {
      this.dataIsSaving = true
      const superProduct = await this.createNewSuperHSProduct(
        this._superHSProductEditable
      )
      void this.refreshProductData(superProduct)
      void this.router.navigateByUrl(`/products/${superProduct.Product.ID}`)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      const message = ex?.response?.data?.Data as string
      if (message) {
        this.toastrService.error(message, 'Error', { onActivateTick: true })
      }
      throw ex
    }
  }

  showVariantToastr(): void {
    this.toastrService.warning(
      this.translate.instant('ADMIN.PRODUCT_EDIT.VARIANTS_WARNING'),
      'Warning',
      { onActivateTick: true }
    )
  }
  productWasModified(): boolean {
    return (
      JSON.stringify(this._superHSProductEditable) !==
        JSON.stringify(this._superHSProductStatic) ||
      this.stagedImages.length > 0 ||
      this.stagedDocuments.length > 0
    )
  }

  async updateProduct(): Promise<void> {
    try {
      this.dataIsSaving = true
      let superProduct = this._superHSProductStatic
      if (this.productWasModified()) {
        superProduct = await this.updateHSProduct(this._superHSProductEditable)
        this.updateList.emit(superProduct.Product as Product)
      }
      void this.refreshProductData(superProduct)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      const message = ex?.response?.data?.Data as string
      if (message) {
        this.toastrService.error(message, 'Error', { onActivateTick: true })
      }
      throw ex
    }
  }

  updateProductResource(productUpdate: any): void {
    const resourceToUpdate =
      this._superHSProductEditable || this.productService.emptyResource
    this._superHSProductEditable =
      this.productService.getUpdatedEditableResource(
        productUpdate,
        resourceToUpdate
      )
    this.checkForChanges()
  }

  handleUpdateProduct(event: any, field: string, typeOfValue?: string): void {
    const booleanProductFields: string[] = [
      'Product.Active',
      'Product.Inventory.Enabled',
      'Product.Inventory.OrderCanExceed',
      'Product.Inventory.VariantLevelTracking',
      'Product.xp.FreeShipping',
      'Product.Returnable',
    ]
    const productUpdate = {
      field,
      value: booleanProductFields.includes(field)
        ? event.target.checked
        : typeOfValue === 'number'
        ? Number(event.target.value)
        : event.target.value,
    }

    if (field === 'PriceSchedule.MaxQuantity' && productUpdate.value === 0) {
      //If max quantity is changed to zero, they are trying to remove it completely.
      productUpdate.value = null
    }

    this.updateProductResource(productUpdate)
  }

  handleUpdatePricing(event: any): void {
    this.updateProductResource(event)
    this.productForm.controls.Price.setValue(
      event?.value?.PriceBreaks[0]?.Price
    )
  }

  // Used only for Product.Description coming out of quill editor (no 'event.target'.)
  updateResourceFromFieldValue(field: string, value: any): void {
    const updateProductResourceCopy = this.productService.copyResource(
      this._superHSProductEditable || this.productService.emptyResource
    )
    updateProductResourceCopy.Product = {
      ...updateProductResourceCopy.Product,
      [field]: value,
    }
    this._superHSProductEditable = updateProductResourceCopy
    this.checkForChanges()
  }
  // TODO: Remove duplicate function, function exists in resource-crud.component.ts (minus the files check);
  checkForChanges(): void {
    this.areChanges =
      JSON.stringify(this._superHSProductEditable) !==
        JSON.stringify(this._superHSProductStatic) ||
      this.stagedImages?.length > 0 ||
      this.stagedDocuments?.length > 0
  }

  onStagedImagesChange(files: FileHandle[]): void {
    this.stagedImages = files
    this.checkForChanges()
  }

  onStagedDocumentsChange(files: FileHandle[]): void {
    this.stagedDocuments = files
    this.checkForChanges()
  }

  async onDeleteFile(event: DeleteFileEvent): Promise<void> {
    this._superHSProductStatic.Product =
      await this.assetService.deleteAssetUpdateProduct(
        this._superHSProductEditable.Product,
        event.FileUrl,
        event.AssetType
      )
    this.updateList.emit(this._superHSProductStatic.Product as Product)
    void this.refreshProductData(this._superHSProductStatic)
  }

  async initTaxCategorization(): Promise<void> {
    // TODO: This is a temporary fix to accomodate for data not having xp.TaxCode yet
    if (
      this._superHSProductEditable?.Product?.xp &&
      !this._superHSProductEditable.Product.xp.Tax
    ) {
      this._superHSProductEditable.Product.xp.Tax = {
        Code: '',
        Description: '',
        LongDescription: '',
      }
    }
    this.taxCodes = await this.getTaxCodes()
  }

  handleTaxCodeSelection(event: TaxCategorization): void {
    const codeUpdate = { target: { value: event.Code } }
    const descriptionUpdate = { target: { value: event.Description } }
    this.productForm.controls.TaxCode.setValue(event.Code)
    this.handleUpdateProduct(codeUpdate, 'Product.xp.Tax.Code')
    this.handleUpdateProduct(descriptionUpdate, 'Product.xp.Tax.Description')
  }

  async searchTaxCodes(searchTerm: string): Promise<void> {
    this.taxCodes = await this.getTaxCodes(searchTerm)
  }

  getSaveBtnText(): string {
    return this.productService.getSaveBtnText(
      this.dataIsSaving,
      this.isCreatingNew
    )
  }

  async createNewSuperHSProduct(
    superHSProduct: SuperHSProduct
  ): Promise<SuperHSProduct> {
    const supplier = await this.currentUserService.getMySupplier()
    superHSProduct.Product.xp.ProductType = this.productType
    superHSProduct.Product.xp.Currency = supplier?.xp?.Currency
    superHSProduct.PriceSchedule.ID = superHSProduct.Product.ID
    superHSProduct.PriceSchedule.Name = `Default_HS_Buyer${superHSProduct.Product.Name}`
    // Slice Price Schedule if more than 100 characters after the pre-pended 'Default_HS_Buyer'.
    if (superHSProduct.PriceSchedule.Name.length > 100) {
      superHSProduct.PriceSchedule.Name =
        superHSProduct.PriceSchedule.Name.slice(0, 100)
    }
    if (superHSProduct.Product.xp.Tax.Code === null)
      superHSProduct.Product.xp.Tax = null
    if (superHSProduct.PriceSchedule.PriceBreaks[0].Price === null)
      superHSProduct.PriceSchedule.PriceBreaks[0].Price = 0
    if (this.stagedImages.length > 0) {
      const imgAssets = await this.assetService.uploadImageFiles(
        this.stagedImages
      )
      superHSProduct.Product.xp.Images = imgAssets
    }
    if (this.stagedDocuments.length > 0) {
      const documentAssets = await this.assetService.uploadDocumentFiles(
        this.stagedDocuments
      )
      superHSProduct.Product.xp.Documents = documentAssets
    }
    try {
      return await HeadStartSDK.Products.Post(superHSProduct)
    } finally {
      this.stagedImages = []
      this.stagedDocuments = []
    }
  }

  async updateHSProduct(
    superHSProduct: SuperHSProduct
  ): Promise<SuperHSProduct> {
    // If PriceSchedule has a price break price, but no ID or name, set them
    if (
      superHSProduct.PriceSchedule?.PriceBreaks[0]?.Price &&
      superHSProduct.PriceSchedule.ID === null
    ) {
      superHSProduct.PriceSchedule.ID = superHSProduct.Product.ID
      superHSProduct.PriceSchedule.Name = `Default_HS_Buyer${superHSProduct.Product.Name}`
      // Slice Price Schedule if more than 100 characters after the pre-pended 'Default_HS_Buyer'.
      if (superHSProduct.PriceSchedule.Name.length > 100) {
        superHSProduct.PriceSchedule.Name =
          superHSProduct.PriceSchedule.Name.slice(0, 100)
      }
    }
    if (!superHSProduct.Product.xp) {
      superHSProduct.Product.xp = {}
    }
    if (superHSProduct.PriceSchedule.PriceBreaks.length === 0)
      superHSProduct.PriceSchedule = null
    if (this.stagedImages.length > 0) {
      const imgAssets = await this.assetService.uploadImageFiles(
        this.stagedImages
      )
      superHSProduct.Product.xp.Images = [
        ...(superHSProduct.Product.xp?.Images || []),
        ...imgAssets,
      ]
    }
    if (this.stagedDocuments.length > 0) {
      const documentAssets = await this.assetService.uploadDocumentFiles(
        this.stagedDocuments
      )
      superHSProduct.Product.xp.Documents = [
        ...(superHSProduct.Product.xp?.Documents || []),
        ...documentAssets,
      ]
    }
    try {
      return await HeadStartSDK.Products.Put(
        superHSProduct.Product.ID,
        superHSProduct
      )
    } finally {
      this.stagedImages = []
      this.stagedDocuments = []
    }
  }

  async handleSelectedProductChange(product: HSProduct): Promise<void> {
    this._exchangeRates = (await HeadStartSDK.ExchangeRates.GetRateList()).Items
    const currencyOnProduct = product.xp.Currency
    this.supplierCurrency = this._exchangeRates?.find(
      (r) => r.Currency === currencyOnProduct
    )
    this.sellerCurrency = this._exchangeRates?.find((r) => r.Currency === 'USD')
    const accessToken = Tokens.GetAccessToken()
    const hsProduct = await HeadStartSDK.Products.Get(product.ID, accessToken)
    void this.refreshProductData(hsProduct)
  }

  getTotalMarkup = (specOptions: SpecOption[]): number => {
    let totalMarkup = 0
    if (specOptions) {
      specOptions.forEach((opt) =>
        opt.PriceMarkup ? (totalMarkup = +totalMarkup + +opt.PriceMarkup) : 0
      )
    }
    return totalMarkup
  }

  updateEditableProductWithVariationChanges(e): void {
    const updateProductResourceCopy = this.productService.copyResource(
      this._superHSProductEditable || this.productService.emptyResource
    )
    updateProductResourceCopy.Specs = e.Specs
    updateProductResourceCopy.Variants = e.Variants
    this._superHSProductEditable = updateProductResourceCopy
    this.checkForChanges()
  }

  validateVariants(e: boolean): void {
    this.variantsValid = e
  }

  validateSpecs(e: boolean): void {
    this.specsValid = e
  }

  getProductPreviewImage(): string | SafeUrl {
    return (
      this.stagedImages[0]?.URL ||
      getProductMediumImageUrl(this._superHSProductEditable?.Product)
    )
  }

  ngOnDestroy(): void {
    this.alive = false
  }
}
