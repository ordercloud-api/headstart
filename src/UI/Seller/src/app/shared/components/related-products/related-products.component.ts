/* eslint-disable @typescript-eslint/no-unsafe-member-access */
/* eslint-disable @typescript-eslint/no-unsafe-call */
/* eslint-disable @typescript-eslint/explicit-module-boundary-types */
/* eslint-disable @typescript-eslint/no-unsafe-assignment */
/* eslint-disable prettier/prettier */
import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core'
import { faTrash } from '@fortawesome/free-solid-svg-icons'
import { NgbModal } from '@ng-bootstrap/ng-bootstrap'
import { Product, Products } from 'ordercloud-javascript-sdk'
import { uniq } from 'lodash'
import { Router } from '@angular/router'

@Component({
  selector: 'related-products-component',
  templateUrl: './related-products.component.html',
  styleUrls: ['./related-products.component.scss'],
})
export class RelatedProductsComponent implements OnInit {
  @Input() product: Product
  @Input() dataIsSaving: boolean
  @Output() updateList = new EventEmitter<Product>()
  PLACEHOLDER_PRODUCT_IMAGE
  faTrash = faTrash
  relatedProducts: Product[] = []
  selectedRelatedProducts: Product[] = []
  stagedRelatedProducts: string[] = []
  isCreatingNew: boolean;
  modalInstance
  constructor(private modalService: NgbModal, private router: Router) { }

  async ngOnInit(): Promise<void> {
    this.isCreatingNew = this.router.url.includes('new')
    if (!this.product) {
      this.product = {} as Product
    }
    if (this.product?.xp?.RelatedProducts?.length) {
      const selectedProducts = await Products.List({
        filters: { ID: this.product.xp.RelatedProducts.join('|') },
      })
      this.selectedRelatedProducts = selectedProducts.Items
    }
    void this.searchedRelatedProducts()
  }

  openModal(content): void {
    try {
      this.modalInstance = this.modalService.open(content)
    } catch {
      // modal was dismissed
    }
  }

  async searchedRelatedProducts(searchTerm?: string) {
    const result = await Products.List({ search: searchTerm, pageSize: 5 })
    this.relatedProducts = result.Items
  }

  toggleStagedRelatedProducts(productID: string): void {
    const stagedIndex = this.stagedRelatedProducts.indexOf(productID)
    const isAlreadyStaged = stagedIndex > -1
    if (isAlreadyStaged) {
      this.stagedRelatedProducts.splice(stagedIndex, 1)
    } else {
      this.stagedRelatedProducts.push(productID)
    }
  }

  cancel() {
    this.stagedRelatedProducts = []
    this.modalInstance.dismiss()
  }

  async removeProductFromRelatedGroup(productID: string) {
    const index = this.selectedRelatedProducts.map(p => p.ID).indexOf(productID)
    if (index > -1) {
      this.selectedRelatedProducts.splice(index, 1)
      if (!this.product.xp) {
        this.product.xp = {}
      }
      this.product.xp.RelatedProducts = this.selectedRelatedProducts.map(p => p.ID);
      let partialProduct: Partial<Product> = {}
      partialProduct.xp = {}
      partialProduct.xp.RelatedProducts = this.product.xp.RelatedProducts
      this.product = await Products.Patch(this.product.ID, partialProduct)
      this.stagedRelatedProducts.push(productID)
    }
  }

  async addSelectedRelatedProducts() {
    if (!this.product.xp) {
      this.product.xp = {}
    }
    const allRelatedProducts = uniq([
      ...(this.product?.xp?.RelatedProducts || []),
      ...this.stagedRelatedProducts,
    ])
    this.product.xp.RelatedProducts = allRelatedProducts
    const selectedProducts = await Products.List({
      filters: { ID: allRelatedProducts.join('|') },
    })
    let partialProduct: Partial<Product> = {}
    partialProduct.xp = {}
    partialProduct.xp.RelatedProducts = this.product.xp.RelatedProducts
    this.product = await Products.Patch(this.product.ID, partialProduct)
    this.selectedRelatedProducts = selectedProducts.Items
    this.stagedRelatedProducts = []
    this.modalInstance.close()
  }
}