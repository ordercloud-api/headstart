import { Component, Input, OnInit } from '@angular/core'
import { FormControl, FormGroup } from '@angular/forms'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { KitService } from '@app-seller/kits/kits.service'
import {
  faCircle,
  faHeart,
  faTimes,
  faTrash,
} from '@fortawesome/free-solid-svg-icons'
import {
  HeadStartSDK,
  Asset,
  AssetUpload,
  ListPage,
  SuperHSProduct,
  HSKitProduct,
  HSProductInKit,
  ListArgs,
} from '@ordercloud/headstart-sdk'
import { Router } from '@angular/router'
import { DomSanitizer } from '@angular/platform-browser'
import { NgbModal } from '@ng-bootstrap/ng-bootstrap'
import { Location } from '@angular/common'
import { Buyer, OcBuyerService } from '@ordercloud/angular-sdk'
import { TabIndexMapper } from './tab-mapper'
import { ContentManagementClient } from '@ordercloud/cms-sdk'
import { FileHandle } from '@app-seller/models/file-upload.types'
@Component({
  selector: 'app-kits-edit',
  templateUrl: './kits-edit.component.html',
  styleUrls: ['./kits-edit.component.scss'],
})
export class KitsEditComponent implements OnInit {
  kitProductEditable: HSKitProduct
  kitProductStatic: HSKitProduct
  kitProductForm: FormGroup
  @Input()
  set selectedKitProduct(product: HSKitProduct) {
    if (product?.Product) this.handleSelectedProductChange(product)
    else {
      this.setForms(this.kitService.emptyResource)
      this.kitProductEditable = this.kitService.emptyResource
      this.kitProductStatic = this.kitService.emptyResource
    }
  }
  faTimes = faTimes
  faTrash = faTrash
  faCircle = faCircle
  faHeart = faHeart
  @Input() readonly: boolean
  @Input()
  filterConfig
  isCreatingNew: boolean
  isLoading = false
  dataIsSaving = false
  areChanges = false
  productAssignments: HSProductInKit[] = []
  productsIncluded: any[] = []
  productList: ListPage<SuperHSProduct>
  productsToAdd: string[] = []
  imageFiles: FileHandle[] = []
  images: Asset[] = []
  staticContentFiles: FileHandle[] = []
  staticContent: Asset[] = []
  documentName: string
  searchTermInput: string
  buyers: Buyer[]
  constructor(
    private router: Router,
    private appAuthService: AppAuthService,
    private location: Location,
    private ocBuyerService: OcBuyerService,
    private kitService: KitService,
    private sanitizer: DomSanitizer,
    private modalService: NgbModal
  ) {}

  async ngOnInit(): Promise<void> {
    this.isCreatingNew = this.kitService.checkIfCreatingNew()
    this.productList = await this.getProductList()
    this.getBuyers()
  }

  setForms(kitProduct: HSKitProduct): void {
    this.kitProductForm = new FormGroup({
      ID: new FormControl(kitProduct.Product.ID),
      Name: new FormControl(kitProduct.Product.Name),
      Active: new FormControl(kitProduct.Product.Active),
    })
  }

  async getBuyers(): Promise<void> {
    const buyers = await this.ocBuyerService.List().toPromise()
    this.buyers = buyers.Items
  }

  async getProductList(args?: ListArgs): Promise<ListPage<SuperHSProduct>> {
    this.isLoading = true
    const accessToken = await this.appAuthService.fetchToken().toPromise()
    const productList = args
      ? await HeadStartSDK.Products.List(args, accessToken)
      : await HeadStartSDK.Products.List({}, accessToken)
    this.isLoading = false
    return {
      Meta: productList.Meta,
      Items: productList?.Items.filter((p) => p.PriceSchedule.ID !== null),
    }
  }

  async handleSelectedProductChange(product: HSKitProduct): Promise<void> {
    const hsKitProduct = this.isCreatingNew
      ? this.kitService.emptyResource
      : await HeadStartSDK.KitProducts.Get(product.Product.ID)
    this.refreshProductData(hsKitProduct)
  }

  async refreshProductData(product: HSKitProduct): Promise<void> {
    this.kitProductEditable = JSON.parse(JSON.stringify(product))
    this.kitProductStatic = JSON.parse(JSON.stringify(product))
    this.staticContent = product.Attachments
    this.images = product.Images
    this.setForms(product)
    this.isCreatingNew = this.kitService.checkIfCreatingNew()
    if (!this.isCreatingNew) await this.getProductsInKit(product)
    this.checkForChanges()
  }

  async getProductsInKit(product: HSKitProduct): Promise<void> {
    const productAssignments = []
    const accessToken = await this.appAuthService.fetchToken().toPromise()
    // eslint-disable-next-line @typescript-eslint/no-misused-promises
    product.ProductAssignments.ProductsInKit.forEach(async (p) => {
      const ocProduct = await HeadStartSDK.Products.Get(p.ID, accessToken)
      productAssignments.push({
        ID: p.ID,
        Name: ocProduct.Product.Name,
        Variants: p.Variants,
        MinQty: p.MinQty,
        MaxQty: p.MaxQty,
        Static: p.Static,
        SpecCombo: p.SpecCombo,
        Optional: p.Optional,
      })
    })
    this.productsIncluded = productAssignments
  }

  handleUpdateProduct(
    event: any,
    field: string,
    typeOfValue?: string,
    product?: any
  ): void {
    if (product?.ID) {
      const updatedAssignments = this.kitProductEditable.ProductAssignments
        .ProductsInKit
      let index
      for (let i = 0; i < updatedAssignments.length; i++) {
        if (updatedAssignments[i].ID === product.ID) {
          index = i
        }
      }
      field = 'ProductAssignments.ProductsInKit[' + index + '].' + field
    }
    const productUpdate = {
      field,
      value:
        typeOfValue === 'boolean'
          ? event.target.checked
          : typeOfValue === 'number'
          ? Number(event.target.value)
          : event.target.value,
    }
    this.updateProductResource(productUpdate)
  }

  updateProductResource(productUpdate: any): void {
    const resourceToUpdate =
      this.kitProductEditable || this.kitService.emptyResource
    this.kitProductEditable = this.kitService.getUpdatedEditableResource(
      productUpdate,
      resourceToUpdate
    )
    this.checkForChanges()
  }

  async updateProduct(): Promise<void> {
    try {
      this.dataIsSaving = true
      let superProduct = this.kitProductStatic
      if (
        JSON.stringify(this.kitProductEditable) !==
        JSON.stringify(this.kitProductStatic)
      ) {
        superProduct = await HeadStartSDK.KitProducts.Save(
          this.kitProductEditable.Product.ID,
          this.kitProductEditable
        )
      }
      if (this.imageFiles.length > 0)
        await this.addImages(this.imageFiles, superProduct.Product.ID)
      if (this.staticContentFiles.length > 0) {
        await this.addDocuments(
          this.staticContentFiles,
          superProduct.Product.ID
        )
      }
      this.refreshProductData(superProduct)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }

  async createNewKitProduct(): Promise<void> {
    try {
      this.dataIsSaving = true
      const superProduct = await HeadStartSDK.KitProducts.Create(
        this.kitProductEditable
      )
      this.refreshProductData(superProduct)
      if (this.imageFiles.length > 0)
        await this.addImages(this.imageFiles, superProduct.Product.ID)
      if (this.staticContentFiles.length > 0)
        await this.addDocuments(
          this.staticContentFiles,
          superProduct.Product.ID
        )
      this.router.navigateByUrl(`/kitproducts/${superProduct.Product.ID}`)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }

  async changePage(page: number): Promise<void> {
    this.productList = await this.getProductList({ page })
  }

  handleSave(): void {
    if (this.isCreatingNew) this.createNewKitProduct()
    else this.updateProduct()
  }

  async handleDelete(): Promise<void> {
    await HeadStartSDK.KitProducts.Delete(this.kitProductStatic.Product.ID)
    this.router.navigateByUrl('/kitproducts')
  }

  handleDiscardChanges(): void {
    this.imageFiles = []
    this.staticContentFiles = []
    this.productsToAdd = []
    this.kitProductEditable = this.kitProductStatic
    this.refreshProductData(this.kitProductStatic)
  }

  getSaveBtnText(): string {
    return this.kitService.getSaveBtnText(this.dataIsSaving, this.isCreatingNew)
  }

  checkForChanges(): void {
    this.areChanges =
      JSON.stringify(this.kitProductEditable) !==
        JSON.stringify(this.kitProductStatic) ||
      this.imageFiles?.length > 0 ||
      this.staticContentFiles?.length > 0
  }

  /** *******************************************
   ******** PRODUCT ASSIGNMENT FUNCTIONS ********
   *********************************************/

  async handleCreateAssignment(): Promise<void> {
    const updatedAssignments =
      this.isCreatingNew && !this.productsIncluded.length
        ? []
        : this.kitProductEditable.ProductAssignments.ProductsInKit
    const accessToken = await this.appAuthService.fetchToken().toPromise()
    await this.asyncForEach(this.productsToAdd, async (productID) => {
      const ocProduct = await HeadStartSDK.Products.Get(productID, accessToken)
      const newProduct = {
        ID: productID,
        Name: ocProduct.Product.Name,
        Variants: ocProduct.Variants,
        SpecCombo: '',
        MinQty: null,
        MaxQty: null,
        Static: false,
      }
      const productInKit = {
        ID: productID,
        MinQty: null,
        MaxQty: null,
        Static: false,
        Variants: ocProduct.Variants,
        SpecCombo: ocProduct?.Variants?.length
          ? ocProduct.Variants[0].xp.SpecCombo
          : '',
      }
      if (!this.productsIncluded.includes(newProduct))
        this.productsIncluded.push(newProduct)
      if (!updatedAssignments.includes(productInKit))
        updatedAssignments.push(productInKit)
    })
    this.productsToAdd = []
    const updatedProduct = {
      field: 'ProductAssignments.ProductsInKit',
      value: updatedAssignments,
    }
    this.updateProductResource(updatedProduct)
  }

  // this helper function is to ensure that each async api call is completed before continuing
  async asyncForEach(array, cb): Promise<void> {
    for (let i = 0; i < array.length; i++) {
      await cb(array[i], i, array)
    }
  }

  handleDeleteAssignment(product: any): void {
    let updatedAssignments = this.kitProductEditable.ProductAssignments
      .ProductsInKit
    updatedAssignments = updatedAssignments.filter((p) => p.ID !== product.ID)
    this.productsIncluded = this.productsIncluded.filter(
      (includedProduct) => includedProduct.ID !== product.ID
    )
    const updatedProduct = {
      field: 'ProductAssignments.ProductsInKit',
      value: updatedAssignments,
    }
    this.updateProductResource(updatedProduct)
  }

  async searchedResources(searchText: any): Promise<void> {
    this.searchTermInput = searchText
    this.productList = await this.getProductList({ search: searchText })
  }

  setProductConfigurability(event: any, product): void {
    product.Static = event.target.checked
  }

  isProductInKit(productID: string): boolean {
    return this.productsIncluded.some((pInKit) => productID === pInKit.ID)
  }

  selectProductsToAdd(event: any, productID: string): void {
    if (
      event.target.checked &&
      !this.isProductInKit(productID) &&
      !this.productsToAdd.includes(productID)
    ) {
      this.productsToAdd.push(productID)
    } else if (!event.target.checked) {
      this.productsToAdd = this.productsToAdd.filter(
        (product) => productID !== product
      )
    }
  }

  openProductList(content): void {
    this.modalService.open(content, { ariaLabelledBy: 'product-list' })
  }

  tabChanged(event: any, productID: string): void {
    const nextIndex = Number(event.nextId)
    if (productID === null || this.isCreatingNew) return
    const newLocation =
      nextIndex === 0
        ? `kitproducts/${productID}`
        : `kitproducts/${productID}/${TabIndexMapper[nextIndex]}`
    this.location.replaceState(newLocation)
  }

  /** *******************************************
   **** PRODUCT IMAGE / DOC UPLOAD FUNCTIONS ****
   *********************************************/

  manualFileUpload(event, fileType: string): void {
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
  stageImages(files: FileHandle[]): void {
    this.imageFiles = this.imageFiles.concat(files)
    this.checkForChanges()
  }
  async addDocuments(files: FileHandle[], productID: string): Promise<void> {
    let superProduct
    for (const file of files) {
      superProduct = await this.uploadAsset(productID, file, true)
    }
    this.staticContentFiles = []
    // Only need the `|| {}` to account for creating new product where this._superHSProductStatic doesn't exist yet.
    superProduct = Object.assign(this.kitProductStatic || {}, superProduct)
    this.refreshProductData(superProduct)
  }
  async addImages(files: FileHandle[], productID: string): Promise<void> {
    let superProduct
    for (const file of files) {
      superProduct = await this.uploadAsset(productID, file)
    }
    this.imageFiles = []
    // Only need the `|| {}` to account for creating new product where this._superHSProductStatic doesn't exist yet.
    superProduct = Object.assign(this.kitProductStatic || {}, superProduct)
    this.refreshProductData(superProduct)
  }
  async uploadAsset(
    productID: string,
    file: FileHandle,
    isAttachment = false
  ): Promise<SuperHSProduct> {
    const accessToken = await this.appAuthService.fetchToken().toPromise()
    const asset = {
      Active: true,
      Title: isAttachment ? 'Product_Attachment' : null,
      File: file.File,
      FileName: file.Filename,
    } as AssetUpload
    const newAsset: Asset = await HeadStartSDK.Upload.UploadAsset(
      asset,
      accessToken
    )
    await ContentManagementClient.Assets.SaveAssetAssignment(
      { ResourceType: 'Products', ResourceID: productID, AssetID: newAsset.ID },
      accessToken
    )
    return await HeadStartSDK.Products.Get(productID, accessToken)
  }

  async removeFile(file: Asset): Promise<void> {
    const accessToken = await this.appAuthService.fetchToken().toPromise()
    // Remove the image assignment, then remove the image
    await ContentManagementClient.Assets.DeleteAssetAssignment(
      file.ID,
      this.kitProductStatic.ID,
      'Products',
      null,
      null,
      accessToken
    )
    await ContentManagementClient.Assets.Delete(file.ID, accessToken)
    if (file.Type === 'Image') {
      this.kitProductStatic.Images = this.kitProductStatic.Images.filter(
        (i) => i.ID !== file.ID
      )
    } else {
      this.kitProductStatic.Attachments = this.kitProductStatic.Attachments.filter(
        (a) => a.ID !== file.ID
      )
    }
    this.refreshProductData(this.kitProductStatic)
  }
  unstageFile(index: number, fileType: string): void {
    if (fileType === 'image') {
      this.imageFiles.splice(index, 1)
    } else {
      this.staticContentFiles.splice(index, 1)
    }
    this.checkForChanges()
  }
  getDocumentName(event: KeyboardEvent): void {
    this.documentName = (event.target as HTMLInputElement).value
  }

  openConfirm(content): void {
    this.modalService.open(content, { ariaLabelledBy: 'confirm-modal' })
  }
}
