import { Component, Input, Output, EventEmitter } from '@angular/core'
import { FormControl } from '@angular/forms'
import {
  SupplierCategoryConfig,
  SupplierCategoryConfigFilters,
  SupplierCategorySelection,
} from '@app-seller/models/category.types'

export const areAllCategoriesComplete = (
  categorySelections: SupplierCategorySelection[]
): boolean => {
  return !categorySelections?.some((category) => {
    return !category.ServiceCategory || !category.VendorLevel
  })
}

export const areDuplicateCategories = (
  categorySelections: SupplierCategorySelection[]
): boolean => {
  return categorySelections?.some((selection) =>
    isADuplicateCategory(selection, categorySelections)
  )
}

export const isADuplicateCategory = (
  categorySelection: SupplierCategorySelection,
  categorySelections: SupplierCategorySelection[]
): boolean => {
  const categorySelectionsFlat = categorySelections.map(
    (selection) => `${selection.ServiceCategory}${selection.VendorLevel}`
  )
  return (
    categorySelectionsFlat.filter(
      (selectionFlat) =>
        selectionFlat ===
        `${categorySelection.ServiceCategory}${categorySelection.VendorLevel}`
    ).length > 1
  )
}

export const isSecondDuplicateCategory = (
  categorySelection: SupplierCategorySelection,
  categorySelections: SupplierCategorySelection[],
  index: number
): boolean => {
  const categorySelectionsFlat = categorySelections.map(
    (selection) => `${selection.ServiceCategory}${selection.VendorLevel}`
  )
  const indexOfFirstAppearanceOfCategory = categorySelectionsFlat.indexOf(
    `${categorySelection.ServiceCategory}${categorySelection.VendorLevel}`
  )
  return indexOfFirstAppearanceOfCategory < index
}

@Component({
  selector: 'supplier-category-select-component',
  templateUrl: './supplier-category-select.component.html',
  styleUrls: ['./supplier-category-select.component.scss'],
})
export class SupplierCategorySelectComponent {
  canAddAnotherCategory = true
  _categorySelectionsControl: FormControl
  _categorySelections: SupplierCategorySelection[]

  _vendorLevelConfig: SupplierCategoryConfigFilters
  _serviceCatagoryConfig: SupplierCategoryConfigFilters
  _categoriesDisabled: boolean

  isSecondDuplicateCategory = isSecondDuplicateCategory
  areNoCategories = false

  @Input()
  set categorySelectionsControl(value: FormControl) {
    this.updateCategoryValidation(value.value)
    this._categorySelectionsControl = value
    this._categorySelections = value.value
    this._categoriesDisabled = value.status === 'DISABLED'
    this._categorySelectionsControl.valueChanges.subscribe(
      (categorySelections) => {
        this._categorySelections = categorySelections
        this._categoriesDisabled = categorySelections.status === 'DISABLED'
        this.updateCategoryValidation(categorySelections)
      }
    )
  }
  @Input()
  set filterConfig(value: SupplierCategoryConfig) {
    if (value?.Filters) {
      this._vendorLevelConfig = value.Filters.find(
        (filter) => filter.Display === 'Vendor Level'
      )
      this._serviceCatagoryConfig = this.getSortedCategories(value)
    }
  }
  @Output()
  selectionsChanged = new EventEmitter()

  removeCategory(index: number): void {
    const newCategorySelection = JSON.parse(
      JSON.stringify(this._categorySelections)
    )
    newCategorySelection.splice(index, 1)
    this.updateCategory(newCategorySelection)
  }

  addCategory(): void {
    if (
      !this._serviceCatagoryConfig?.Items?.length ||
      !this._vendorLevelConfig?.Items?.length
    ) {
      throw new Error(
        'Application not configured with categories. Please contact an admin'
      )
    }
    const newCategorySelection = [
      ...(this._categorySelections || []),
      {
        ServiceCategory: this._serviceCatagoryConfig.Items[0].Text,
        VendorLevel: this._vendorLevelConfig.Items[0].Text,
      },
    ]
    this.updateCategory(newCategorySelection)
  }

  getSortedCategories(
    supplierCategoryConfig: SupplierCategoryConfig
  ): SupplierCategoryConfigFilters {
    let result: SupplierCategoryConfigFilters

    if (supplierCategoryConfig?.Filters?.length > 0) {
      result = supplierCategoryConfig.Filters.find(
        (filter) => filter.Display === 'Service Category'
      )
      result?.Items?.sort((a, b) =>
        a.Text.toLowerCase() > b.Text.toLowerCase() ? 1 : -1
      )
    }

    return result
  }

  makeSelection(event: any, field: string, index: number): void {
    const newCategorySelection = this._categorySelections
    newCategorySelection[index][field] = event.target.value
    this.updateCategory(newCategorySelection)
  }

  updateCategory(newCategorySelection: SupplierCategorySelection[]) {
    this.updateCategoryValidation(newCategorySelection)
    this.selectionsChanged.emit({
      field: 'xp.Categories',
      value: newCategorySelection,
    })
    this._categorySelectionsControl.setValue(newCategorySelection)
  }

  updateCategoryValidation(
    newCategorySelection: SupplierCategorySelection[]
  ): void {
    this.areNoCategories = !newCategorySelection?.length
    this.canAddAnotherCategory = areAllCategoriesComplete(newCategorySelection)
  }
}
