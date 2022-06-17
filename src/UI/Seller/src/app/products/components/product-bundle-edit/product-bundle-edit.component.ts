import {
  Component,
  Input,
  OnInit,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges,
} from '@angular/core'
import { faTrash } from '@fortawesome/free-solid-svg-icons'
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap'
import { Product, Products } from 'ordercloud-javascript-sdk'
import { cloneDeep, isEqual, set, uniq } from 'lodash'
import { Router } from '@angular/router'
import { HSProduct } from '@ordercloud/headstart-sdk'
import { FormBuilder, Validators } from '@angular/forms'
import { TypedFormGroup } from 'ngx-forms-typed'
import { FileHandle } from '@app-seller/shared'
import { DeleteFileEvent } from '../product-image-upload/product-image-upload.component'
import { AssetService } from '@app-seller/shared/services/assets/asset.service'
import { ValidateNoSpecialCharactersAndSpaces } from '@app-seller/validators/validators'
import { PLACEHOLDER_PRODUCT_IMAGE } from '@app-seller/shared/services/assets/asset.helper'

type ProductBundleFormValue = {
  Active: boolean
  Name: string
  ID: string
}
type ProductBundleForm = TypedFormGroup<ProductBundleFormValue>
@Component({
  selector: 'app-product-bundle-edit',
  templateUrl: './product-bundle-edit.component.html',
  styleUrls: ['./product-bundle-edit.component.scss'],
})
export class ProductBundleEditComponent implements OnInit, OnChanges {
  PLACEHOLDER_PRODUCT_IMAGE = PLACEHOLDER_PRODUCT_IMAGE
  @Input() product: HSProduct
  @Input() dataIsSaving: boolean
  @Output() updateList = new EventEmitter<Product>()
  productOriginal: HSProduct
  form: ProductBundleForm
  faTrash = faTrash
  bundledProducts: Product[] = []
  selectedBundledProducts: Product[] = []
  stagedBundledProducts: string[] = []
  stagedImages: FileHandle[] = []
  stagedDocuments: FileHandle[] = []
  isCreatingNew: boolean
  modalInstance: NgbModalRef
  areChanges: boolean

  constructor(
    private modalService: NgbModal,
    private router: Router,
    private formBuilder: FormBuilder,
    private assetService: AssetService
  ) {}

  async ngOnInit(): Promise<void> {
    this.isCreatingNew = this.router.url.includes('new')
    if (!this.product) {
      this.product = {} as HSProduct
    }
    this.form = this.createForm(this.product)
    if (this.product?.xp?.BundledProducts?.length) {
      const selectedProducts = await Products.List({
        filters: { ID: this.product.xp.BundledProducts.join('|') },
      })
      this.selectedBundledProducts = selectedProducts.Items
    }
    void this.searchedBundledProducts()
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.product) {
      this.form = this.createForm(this.product)
      this.productOriginal = cloneDeep(this.product) // use as a reference for discarding changes
    }
  }

  updateProductModel(event: Event): void {
    const input = event.target as HTMLInputElement
    let value: string | number | boolean
    if (input.type === 'text') {
      value = input.value
    } else if (input.type === 'number') {
      value = Number(input.value)
    } else if (input.type === 'checkbox') {
      value = Boolean(input.checked)
    }
    set(this.product, input.name, value)
    this.checkForChanges()
  }

  createForm(product: HSProduct): ProductBundleForm {
    return this.formBuilder.group({
      Active: [product.Active || false],
      Name: [product.Name, [Validators.required, Validators.maxLength(100)]],
      ID: [
        product.ID,
        [ValidateNoSpecialCharactersAndSpaces, Validators.maxLength(100)],
      ],
    }) as ProductBundleForm
  }

  openModal(content: unknown): void {
    try {
      this.modalInstance = this.modalService.open(content)
    } catch {
      // modal was dismissed
    }
  }

  async searchedBundledProducts(searchTerm?: string): Promise<void> {
    const result = await Products.List({ search: searchTerm, pageSize: 5 })
    this.bundledProducts = result.Items
  }

  toggleStagedBundledProducts(productID: string): void {
    const stagedIndex = this.stagedBundledProducts.indexOf(productID)
    const isAlreadyStaged = stagedIndex > -1
    if (isAlreadyStaged) {
      this.stagedBundledProducts.splice(stagedIndex, 1)
    } else {
      this.stagedBundledProducts.push(productID)
    }
  }

  cancel(): void {
    this.stagedBundledProducts = []
    this.modalInstance.dismiss()
  }

  removeProductFromBundle(productID: string): void {
    const index = this.selectedBundledProducts
      .map((p) => p.ID)
      .indexOf(productID)
    if (index > -1) {
      this.selectedBundledProducts.splice(index, 1)
      if (!this.product.xp) {
        this.product.xp = {}
      }
      this.product.xp.BundledProducts = this.selectedBundledProducts.map(
        (p) => p.ID
      )
    }
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
    this.product = await this.assetService.deleteAssetUpdateProduct(
      this.product,
      event.FileUrl,
      event.AssetType
    )
    this.productOriginal = cloneDeep(this.product)
    this.checkForChanges()
    this.updateList.emit(this.product)
  }

  checkForChanges(): void {
    this.areChanges =
      !isEqual(this.productOriginal, this.product) ||
      this.stagedBundledProducts.length > 0 ||
      this.stagedDocuments.length > 0 ||
      this.stagedImages.length > 0
  }

  handleDiscardChanges(): void {
    this.product = cloneDeep(this.productOriginal)
    this.stagedBundledProducts = []
    this.stagedDocuments = []
    this.stagedImages = []
    this.areChanges = false
  }

  handleResetChangesAfterUpdate(updatedProduct: HSProduct): void {
    this.product = updatedProduct
    this.productOriginal = cloneDeep(updatedProduct)
    this.stagedBundledProducts = []
    this.stagedDocuments = []
    this.stagedImages = []
    this.areChanges = false
  }

  async addSelectedProductsToBundle(): Promise<void> {
    if (!this.product.xp) {
      this.product.xp = {}
    }
    const allBundledProducts = uniq([
      ...(this.product?.xp?.BundledProducts || []),
      ...this.stagedBundledProducts,
    ])
    this.product.xp.BundledProducts = allBundledProducts
    const selectedProducts = await Products.List({
      filters: { ID: allBundledProducts.join('|') },
    })
    this.selectedBundledProducts = selectedProducts.Items
    this.stagedBundledProducts = []
    this.modalInstance.close()
  }

  async createProduct(): Promise<void> {
    this.product.xp.ProductType = 'Bundle'
    await this.uploadImages()
    await this.uploadDocuments()
    const product = await Products.Create(this.product)
    this.router.navigateByUrl(`/products/${product.ID}`)
  }

  async updateProduct(): Promise<void> {
    this.product.xp.ProductType = 'Bundle'
    await this.uploadImages()
    await this.uploadDocuments()
    const product = await Products.Save(this.product.ID, this.product)
    this.handleResetChangesAfterUpdate(product)
    this.updateList.emit(product)
  }

  async deleteProduct(): Promise<void> {
    await Products.Delete(this.product.ID)
    this.router.navigateByUrl(`/products`)
  }

  async uploadImages(): Promise<void> {
    if (this.stagedImages.length) {
      const images = await this.assetService.uploadImageFiles(this.stagedImages)
      this.stagedImages = []
      this.product.xp.Images = [...(this.product.xp.Images || []), ...images]
    }
  }

  async uploadDocuments(): Promise<void> {
    if (this.stagedDocuments.length) {
      const documents = await this.assetService.uploadDocumentFiles(
        this.stagedDocuments
      )
      this.stagedDocuments = []
      this.product.xp.Documents = [
        ...(this.product.xp.Documents || []),
        ...documents,
      ]
    }
  }
}
