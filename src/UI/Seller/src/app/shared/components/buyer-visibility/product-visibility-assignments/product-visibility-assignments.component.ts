import { Component, OnInit, Input, OnChanges } from '@angular/core'
import {
  Buyer,
  Buyers,
  ProductAssignment,
  ProductCatalogAssignment,
  Catalogs,
} from 'ordercloud-javascript-sdk'
import { faExclamationCircle } from '@fortawesome/free-solid-svg-icons'
import { HSProduct } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'product-visibility-assignments-component',
  templateUrl: './product-visibility-assignments.component.html',
  styleUrls: ['./product-visibility-assignments.component.scss'],
})
export class ProductVisibilityAssignments implements OnInit, OnChanges {
  @Input()
  product: HSProduct
  buyers: Buyer[]
  add: ProductAssignment[]
  del: ProductAssignment[]
  _productCatalogAssignmentsStatic: ProductCatalogAssignment[]
  _productCatalogAssignmentsEditable: ProductCatalogAssignment[]
  areChanges = false
  requestedUserConfirmation = false
  faExclamationCircle = faExclamationCircle

  constructor() {}

  ngOnInit(): void {
    this.getBuyers()
    this.getProductCatalogAssignments(this.product)
  }

  ngOnChanges(): void {
    this.getProductCatalogAssignments(this.product)
  }

  requestUserConfirmation(): void {
    this.requestedUserConfirmation = true
  }

  async getBuyers(): Promise<void> {
    const buyers = await Buyers.List()
    this.buyers = buyers.Items
  }

  async getProductCatalogAssignments(product: HSProduct): Promise<void> {
    const productCatalogAssignments = await Catalogs.ListProductAssignments({
      productID: product && product.ID,
    })
    this._productCatalogAssignmentsStatic = productCatalogAssignments.Items
    this._productCatalogAssignmentsEditable = productCatalogAssignments.Items
  }

  isAssigned(buyer: Buyer): boolean {
    return (
      this._productCatalogAssignmentsEditable &&
      this._productCatalogAssignmentsEditable.some(
        (productAssignment) =>
          productAssignment.CatalogID === buyer.DefaultCatalogID
      )
    )
  }

  checkForProductCatalogAssignmentChanges(): void {
    this.add = this._productCatalogAssignmentsEditable.filter(
      (assignment) =>
        !JSON.stringify(this._productCatalogAssignmentsStatic).includes(
          assignment.CatalogID
        )
    )
    this.del = this._productCatalogAssignmentsStatic.filter(
      (assignment) =>
        !JSON.stringify(this._productCatalogAssignmentsEditable).includes(
          assignment.CatalogID
        )
    )
    this.areChanges = this.add.length > 0 || this.del.length > 0
    if (!this.areChanges) this.requestedUserConfirmation = false
  }

  discardProductCatalogAssignmentChanges(): void {
    this._productCatalogAssignmentsEditable =
      this._productCatalogAssignmentsStatic
    this.checkForProductCatalogAssignmentChanges()
  }
}
