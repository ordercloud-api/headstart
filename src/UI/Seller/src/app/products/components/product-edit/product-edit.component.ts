import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  Inject,
  ChangeDetectorRef,
  OnDestroy,
} from '@angular/core'
import { get as _get } from 'lodash'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import {
  Address,
  SupplierAddresses,
  AdminAddresses,
  MeUser,
  SpecOption,
} from 'ordercloud-javascript-sdk'
import {
  FormGroup,
  FormControl,
  Validators,
  AbstractControl,
} from '@angular/forms'
import { Router } from '@angular/router'
import { Product } from '@ordercloud/angular-sdk'
import { DomSanitizer, SafeUrl } from '@angular/platform-browser'
import { applicationConfiguration } from '@app-seller/config/app.config'
import {
  faTrash,
  faTimes,
  faCircle,
  faHeart,
  faAsterisk,
  faCheckCircle,
  faTimesCircle,
  faExclamationCircle,
} from '@fortawesome/free-solid-svg-icons'
import { NgbModal } from '@ng-bootstrap/ng-bootstrap'
import { ProductService } from '@app-seller/products/product.service'
import {
  SuperHSProduct,
  ListPage,
  HeadStartSDK,
  ProductXp,
  AssetType,
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
import { AppConfig } from '@app-seller/models/environment.types'
import { AssetService } from '@app-seller/shared/services/assets/asset.service'
import { getProductMediumImageUrl } from '@app-seller/shared/services/assets/asset.helper'
import { TranslateService } from '@ngx-translate/core'

@Component({
  selector: 'app-product-edit',
  templateUrl: './product-edit.component.html',
  styleUrls: ['./product-edit.component.scss'],
})
export class ProductEditComponent implements OnInit, OnDestroy {
  @Input()
  productForm: FormGroup
  @Input()
  set orderCloudProduct(product: Product) {
    if (product.ID) {
      void this.handleSelectedProductChange(product)
    } else {
      this.createProductForm(this.productService.emptyResource)
      this._superHSProductEditable = this.productService.emptyResource
      this._superHSProductStatic = this.productService.emptyResource
    }
  }
  readonly: boolean
  sellerView: boolean
  @Input()
  filterConfig
  @Output()
  updateResource = new EventEmitter<any>()
  @Output()
  updateList = new EventEmitter<Product>()
  @Input()
  addresses: ListPage<Address>
  @Input()
  isCreatingNew: boolean
  @Input()
  dataIsSaving = false
  userContext: UserContext = {} as any
  hasVariations = false
  images: ImageAsset[] = []
  files: FileHandle[] = []
  faTimes = faTimes
  faTrash = faTrash
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
  productType: ProductXp['ProductType']
  shippingAddress: any
  productVariations: any
  variantsValid = true
  specsValid = true
  editSpecs = false
  fileType: string
  imageFiles: FileHandle[] = []
  staticContentFiles: FileHandle[] = []
  staticContent: DocumentAsset[] = []
  documentName: string
  selectedTabIndex = 0
  editPriceBreaks = false
  newPriceBreakPrice = 0
  newPriceBreakQty = 2
  newProductPriceBreaks = []
  availableProductTypes = []
  availableSizeTiers = SizerTiersDescriptionMap
  active: number
  alive = true
  sizeTierSubscription: Subscription
  inventoryValidatorSubscription: Subscription

  constructor(
    private changeDetectorRef: ChangeDetectorRef,
    private router: Router,
    private location: Location,
    private currentUserService: CurrentUserService,
    private productService: ProductService,
    private sanitizer: DomSanitizer,
    private modalService: NgbModal,
    private appAuthService: AppAuthService,
    @Inject(applicationConfiguration) private appConfig: AppConfig,
    private toastrService: ToastrService,
    private assetService: AssetService,
    private translate: TranslateService
  ) {}

  async ngOnInit(): Promise<void> {
    // TODO: Eventually move to a resolve so that they are there before the component instantiates.
    this.isCreatingNew = this.productService.checkIfCreatingNew()
    this.getAddresses()
    const getTaxCategoriesPromise = this.initTaxCategorization()
    this.userContext = await this.currentUserService.getUserContext()
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
      this.productType = productTypeFromUrl as ProductXp['ProductType']
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

  tabChanged(event: any, productID: string): void {
    const nextIndex = Number(event.nextId)
    if (productID === null || this.isCreatingNew) return
    const newLocation =
      nextIndex === 0
        ? `products/${productID}`
        : `products/${productID}/${TabIndexMapper[nextIndex]}`
    this.location.replaceState(newLocation)
  }

  async getAddresses(product?): Promise<void> {
    const context: UserContext = await this.currentUserService.getUserContext()
    if (context.Me.Supplier) {
      this.addresses = await SupplierAddresses.List(context.Me.Supplier.ID)
    } else if (context.Me.Seller) {
      this.addresses = await AdminAddresses.List()
    }
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

    this.taxCodes = await HeadStartSDK.TaxCategories.ListTaxCategories()

    this.staticContent = this._superHSProductEditable.Product?.xp.Documents
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
      this.readonly = this.isProductReadonly(superHSProduct)
      this.sellerView = this.userContext?.UserType === 'SELLER'
      this.productForm = new FormGroup(
        {
          Active: new FormControl(superHSProduct.Product.Active),
          Name: new FormControl(superHSProduct.Product.Name, [
            Validators.required,
            Validators.maxLength(100),
          ]),
          ID: new FormControl(
            superHSProduct.Product.ID,
            ValidateNoSpecialCharactersAndSpaces
          ),
          Description: new FormControl(
            superHSProduct.Product.Description,
            Validators.maxLength(2000)
          ),
          QuantityMultiplier: new FormControl(
            superHSProduct.Product.QuantityMultiplier
          ),
          ShipFromAddressID: new FormControl(
            superHSProduct.Product.ShipFromAddressID,
            Validators.required
          ),
          ShipHeight: new FormControl(superHSProduct.Product.ShipHeight),
          ShipWidth: new FormControl(superHSProduct.Product.ShipWidth),
          ShipLength: new FormControl(superHSProduct.Product.ShipLength),
          ShipWeight: new FormControl(superHSProduct.Product.ShipWeight, [
            Validators.required,
            Validators.min(0),
          ]),
          Price: new FormControl(
            _get(superHSProduct.PriceSchedule, 'PriceBreaks[0].Price', null),
            Validators.required
          ),
          MinQuantity: new FormControl(
            superHSProduct.PriceSchedule?.MinQuantity,
            Validators.min(1)
          ),
          MaxQuantity: new FormControl(
            superHSProduct.PriceSchedule?.MaxQuantity,
            Validators.min(1)
          ),
          UseCumulativeQuantity: new FormControl(
            superHSProduct.PriceSchedule?.UseCumulativeQuantity
          ),
          Note: new FormControl(
            _get(superHSProduct.Product, 'xp.Note'),
            Validators.maxLength(140)
          ),
          ProductType: new FormControl(
            _get(superHSProduct.Product, 'xp.ProductType'),
            Validators.required
          ),
          IsResale: new FormControl(
            _get(superHSProduct.Product, 'xp.IsResale')
          ),
          QuantityAvailable: new FormControl(
            superHSProduct.Product?.Inventory?.QuantityAvailable,
            null
          ),
          InventoryEnabled: new FormControl(
            _get(superHSProduct.Product, 'Inventory.Enabled')
          ),
          VariantLevelTracking: new FormControl(
            _get(superHSProduct.Product, 'Inventory.VariantLevelTracking'),
            null
          ),
          OrderCanExceed: new FormControl(
            _get(superHSProduct.Product, 'Inventory.OrderCanExceed')
          ),
          TaxCode: new FormControl(
            _get(superHSProduct.Product, 'xp.Tax.Code', null),
            Validators.required
          ),
          UnitOfMeasureUnit: new FormControl(
            _get(superHSProduct.Product, 'xp.UnitOfMeasure.Unit'),
            Validators.required
          ),
          SizeTier: new FormControl(
            _get(superHSProduct.Product, 'xp.SizeTier'),
            Validators.required
          ),
          UnitOfMeasureQty: new FormControl(
            _get(superHSProduct.Product, 'xp.UnitOfMeasure.Qty'),
            Validators.required
          ),
          ArtworkRequired: new FormControl(
            _get(superHSProduct.Product, 'xp.ArtworkRequired')
          ),
          FreeShipping: new FormControl(
            _get(superHSProduct.Product, 'xp.FreeShipping')
          ),
          FreeShippingMessage: new FormControl(
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

  isProductReadonly(superHSProduct: SuperHSProduct): boolean {
    if (this.productService.checkIfCreatingNew()) {
      return false
    }

    const currentUser: MeUser = this.userContext.Me
    const product: HSProduct = superHSProduct?.Product
    const isSellerUser: boolean = this.userContext.UserType === 'SELLER'

    if (!product?.DefaultSupplierID && isSellerUser) {
      return false
    }

    if (currentUser?.Supplier?.ID === product?.DefaultSupplierID) {
      return false
    }

    if (isSellerUser && product?.OwnerID === currentUser?.Seller?.ID) {
      return false
    }
    return true
  }

  setInventoryValidator(): void {
    const quantityControl = this.productForm.get('QuantityAvailable')
    const variantLevelTrackingControl = this.productForm.get(
      'VariantLevelTracking'
    )
    this.inventoryValidatorSubscription = this.productForm
      .get('InventoryEnabled')
      .valueChanges.subscribe((inventory) => {
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
    this.sizeTierSubscription = sizeTier.valueChanges.subscribe((sizeTier) => {
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
    const optionalFieldsArray = [
      'TaxCode',
      'ShipWeight',
      'ShipFromAddressID',
      'Price',
    ]
    const optionalControls = optionalFieldsArray.map((item) =>
      this.productForm.get(item)
    )
    this.productForm
      .get('ProductType')
      .valueChanges.pipe(takeWhile(() => this.alive))
      .subscribe((productType) => {
        if (productType === 'Quote') {
          optionalControls.forEach((control) => {
            control.setValidators(null)
            control.updateValueAndValidity()
          })
        } else {
          optionalControls.forEach((control) => {
            control.setValidators(Validators.required)
            control.updateValueAndValidity()
          })
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
    const accessToken = await this.appAuthService.fetchToken().toPromise()
    await HeadStartSDK.Products.Delete(
      this._superHSProductStatic.Product.ID,
      accessToken
    )
    void this.router.navigateByUrl('/products')
  }

  handleDiscardChanges(): void {
    this.imageFiles = []
    this.staticContentFiles = []
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
      this.imageFiles.length > 0 ||
      this.staticContentFiles.length > 0
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
    const productFields: string[] = [
      'Product.Active',
      'Product.Inventory.Enabled',
      'Product.Inventory.OrderCanExceed',
      'Product.Inventory.VariantLevelTracking',
      'Product.xp.ArtworkRequired',
      'Product.xp.FreeShipping',
    ]
    const productUpdate = {
      field,
      value: productFields.includes(field)
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
      this.imageFiles?.length > 0 ||
      this.staticContentFiles?.length > 0
  }

  /** ****************************************
   *  **** PRODUCT IMAGE UPLOAD FUNCTIONS ****
   * ******************************************/

  manualFileUpload(
    event: Event & { target: HTMLInputElement },
    fileType: string
  ): void {
    if (fileType === 'image') {
      const files: FileHandle[] = Array.from(event.target.files).map(
        (file: File) => {
          const URL = this.sanitizer.bypassSecurityTrustUrl(
            window.URL.createObjectURL(file)
          )
          return { File: file, URL }
        }
      )
      this.stageImages(files)
    } else if (fileType === 'staticContent') {
      const files: FileHandle[] = Array.from(event.target.files).map(
        (file: File) => {
          const URL = this.sanitizer.bypassSecurityTrustUrl(
            window.URL.createObjectURL(file)
          )
          return { File: file, URL, Filename: this.documentName }
        }
      )
      this.documentName = null
      this.stageDocuments(files)
    }
  }

  stageImages(files: FileHandle[]): void {
    this.imageFiles = this.imageFiles.concat(files)
    this.checkForChanges()
  }

  async removeFile(file: DocumentAsset, assetType: AssetType): Promise<void> {
    this._superHSProductStatic.Product =
      await this.assetService.deleteAssetUpdateProduct(
        this._superHSProductEditable.Product,
        file.Url,
        assetType
      )
    this.updateList.emit(this._superHSProductStatic.Product as Product)
    void this.refreshProductData(this._superHSProductStatic)
  }

  unstageFile(index: number, fileType: string): void {
    if (fileType === 'image') {
      this.imageFiles.splice(index, 1)
    } else {
      this.staticContentFiles.splice(index, 1)
    }
    this.checkForChanges()
  }

  /** ****************************************
   *  *** PRODUCT DOCUMENT UPLOAD FUNCTIONS ***
   * ******************************************/

  /* This url points to the document in blob storage in order for it to be downloadable. */

  getDocumentName(event: KeyboardEvent): void {
    this.documentName = (event.target as HTMLInputElement).value
  }

  stageDocuments(files: FileHandle[]): void {
    files.forEach((file) => {
      const fileName = file.File.name.split('.')
      const ext = fileName[1]
      const fileNameWithExt = file.Filename + '.' + ext
      file.Filename = fileNameWithExt
    })
    this.staticContentFiles = this.staticContentFiles.concat(files)
    this.checkForChanges()
  }

  open(content): void {
    this.modalService.open(content, { ariaLabelledBy: 'confirm-modal' })
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
    this.taxCodes = await HeadStartSDK.TaxCategories.ListTaxCategories()
  }

  handleTaxCodeSelection(event: TaxCategorization): void {
    debugger
    const codeUpdate = { target: { value: event.Code } }
    const descriptionUpdate = { target: { value: event.Description } }
    this.productForm.controls.TaxCode.setValue(event.Code)
    this.handleUpdateProduct(codeUpdate, 'Product.xp.Tax.Code')
    this.handleUpdateProduct(descriptionUpdate, 'Product.xp.Tax.Description')
  }

  async searchTaxCodes(searchTerm: string): Promise<void> {
    this.taxCodes = await HeadStartSDK.TaxCategories.ListTaxCategories(
      searchTerm ?? ''
    )
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
    superHSProduct.Product.xp.PromotionEligible = true
    superHSProduct.Product.xp.Status = 'Draft'
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
    if (this.imageFiles.length > 0) {
      const imgAssets = await this.assetService.uploadImageFiles(
        this.imageFiles
      )
      superHSProduct.Product.xp.Images = imgAssets
    }
    if (this.staticContentFiles.length > 0) {
      const documentAssets = await this.assetService.uploadDocumentFiles(
        this.staticContentFiles
      )
      superHSProduct.Product.xp.Documents = documentAssets
    }
    try {
      return await HeadStartSDK.Products.Post(superHSProduct)
    } finally {
      this.imageFiles = []
      this.staticContentFiles = []
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
    superHSProduct.Product.xp.Status = 'Draft'
    if (this.imageFiles.length > 0) {
      const imgAssets = await this.assetService.uploadImageFiles(
        this.imageFiles
      )
      superHSProduct.Product.xp.Images = [
        ...(superHSProduct.Product.xp?.Images || []),
        ...imgAssets,
      ]
    }
    if (this.staticContentFiles.length > 0) {
      const documentAssets = await this.assetService.uploadDocumentFiles(
        this.staticContentFiles
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
      this.imageFiles = []
      this.staticContentFiles = []
    }
  }

  async handleSelectedProductChange(product: HSProduct): Promise<void> {
    this._exchangeRates = (await HeadStartSDK.ExchangeRates.GetRateList()).Items
    const currencyOnProduct = product.xp.Currency
    this.supplierCurrency = this._exchangeRates?.find(
      (r) => r.Currency === currencyOnProduct
    )
    this.sellerCurrency = this._exchangeRates?.find((r) => r.Currency === 'USD')
    const accessToken = await this.appAuthService.fetchToken().toPromise()
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

  shouldIsResaleBeChecked(): boolean {
    return this._superHSProductEditable?.Product?.xp?.IsResale
  }

  getProductPreviewImage(): string | SafeUrl {
    return (
      this.imageFiles[0]?.URL ||
      getProductMediumImageUrl(this._superHSProductEditable?.Product)
    )
  }

  ngOnDestroy(): void {
    this.alive = false
    this.sizeTierSubscription.unsubscribe()
    this.inventoryValidatorSubscription.unsubscribe()
  }
}
