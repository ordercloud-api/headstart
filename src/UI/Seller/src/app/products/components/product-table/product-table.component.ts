import { Component, ChangeDetectorRef, NgZone, OnInit } from '@angular/core'
import { Product, OcSupplierService } from '@ordercloud/angular-sdk'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Router, ActivatedRoute } from '@angular/router'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { ProductService } from '@app-seller/products/product.service'
import { UserContext } from '@app-seller/models/user.types'
import { HSProduct } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'app-product-table',
  templateUrl: './product-table.component.html',
  styleUrls: ['./product-table.component.scss'],
})
export class ProductTableComponent
  extends ResourceCrudComponent<Product>
  implements OnInit {
  userContext: UserContext
  filterConfig: any
  constructor(
    private productService: ProductService,
    private currentUserService: CurrentUserService,
    private ocSupplierService: OcSupplierService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedRoute: ActivatedRoute,
    ngZone: NgZone
  ) {
    super(changeDetectorRef, productService, router, activatedRoute, ngZone)
    this.buildFilterConfig()
  }

  async buildFilterConfig(): Promise<void> {
    this.userContext = await this.currentUserService.getUserContext()

    const filters = [
      {
        Display: 'ADMIN.FILTERS.STATUS',
        Path: 'xp.Status',
        Items: [
          { Text: 'ADMIN.FILTER_OPTIONS.DRAFT', Value: 'Draft' },
          { Text: 'ADMIN.FILTER_OPTIONS.PUBLISHED', Value: 'Published' },
        ],
        Type: 'Dropdown',
      },
    ]
    if (
      this.userContext.UserRoles.includes('SupplierReader') ||
      this.userContext.UserRoles.includes('SupplierAdmin')
    ) {
      const supplierFilter = await this.buildSupplierFilter()
      filters.push(supplierFilter)
    }
    this.filterConfig = {
      Filters: filters,
    }
  }

  async buildSupplierFilter(): Promise<any> {
    const supplierListPage = await this.ocSupplierService
      .List({ pageSize: 100, sortBy: ['Name'], filters: { Active: 'true' } })
      .toPromise()
    let suppliers = supplierListPage.Items
    if (supplierListPage.Meta.TotalPages > 1) {
      for (let i = 2; i <= supplierListPage.Meta.TotalPages; i++) {
        const newSuppliers = await this.ocSupplierService
          .List({
            pageSize: 100,
            sortBy: ['Name'],
            filters: { Active: 'true' },
            page: i,
          })
          .toPromise()
        suppliers = suppliers.concat(newSuppliers.Items)
      }
    }
    const supplierFilterOptions = suppliers.map((s) => {
      return { Text: s.Name, Value: s.ID }
    })

    return {
      Display: 'ADMIN.FILTERS.SUPPLIER',
      Path: 'DefaultSupplierID',
      Items: supplierFilterOptions,
      Type: 'Dropdown',
    }
  }
  
  updateResourceInList(product: Product): void {
    const index = this.resourceList?.Items?.findIndex(item => item.ID === product.ID) 
    if(index !== -1) {
      this.resourceList.Items[index] = product
    }
  }
}
