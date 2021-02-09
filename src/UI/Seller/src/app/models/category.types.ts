import { Category } from '@ordercloud/angular-sdk'

// CategoryTreeNode structures category data to be consumed by the 3rd party lib TreeComponent
export class CategoryTreeNode {
  parent: CategoryTreeNode
  category: AssignedCategory
  // id, name, and children are required by TreeComponent
  id: string
  name: string
  children: CategoryTreeNode[]
}

export interface AssignedCategory extends Category {
  Assigned?: boolean
  AssignedByParent?: boolean
}

export interface SupplierCategoryConfigFilters {
  Display: string
  Path: string
  Items: any[]
}
export interface SupplierCategoryConfig {
  id: string
  timeStamp: string
  HSName: string
  Filters: Array<SupplierCategoryConfigFilters>
}

export interface SupplierCategorySelection {
  ServiceCategory: string
  VendorLevel: string
}
