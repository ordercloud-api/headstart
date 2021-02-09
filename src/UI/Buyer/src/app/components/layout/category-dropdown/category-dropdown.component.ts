import { Component, Input, Output, EventEmitter } from '@angular/core'
import { Category } from 'ordercloud-javascript-sdk'
import { faCaretRight } from '@fortawesome/free-solid-svg-icons'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './category-dropdown.component.html',
  styleUrls: ['./category-dropdown.component.scss'],
})
export class OCMCategoryDropdown {
  faCaretRight = faCaretRight
  activeCatID = ''
  activeSubCatID = ''

  @Output() closeDropdown = new EventEmitter()
  @Input() set categories(value: Category[]) {
    this.parentCategories = this.assignCategories(value, 'parent')
    this.subCategories = this.assignCategories(value, 'subCategory')
    this.subSubCategories = this.assignCategories(value, 'subSubCategory')
  }

  parentCategories: Category[] = []
  subCategories: Category[] = []
  subSubCategories: Category[] = [] // Intentionally restricted to three levels.

  constructor(private context: ShopperContextService) {}

  assignCategories(categories: Category[], level: string): any {
    const subCategories = []
    const subSubCategories = []

    if (!categories || !level) return categories

    const parentCategories = categories.filter(
      (category) => category.ParentID === null
    )

    categories.forEach((category) => {
      for (const parentCategory of parentCategories) {
        if (parentCategory.ID === category.ParentID) {
          subCategories.push(category)
        }
      }
    })

    categories.forEach((category) => {
      for (const subCategory of subCategories) {
        if (subCategory.ID === category.ParentID) {
          subSubCategories.push(category)
        }
      }
    })

    if (level === 'parent') return parentCategories
    else if (level === 'subCategory') return subCategories
    else if (level === 'subSubCategory') return subSubCategories
  }

  checkForChildren(category: Category): boolean {
    return category.ChildCount > 0
  }

  setActiveCategory(categoryID: string): void {
    this.context.router.toProductList({ categoryID })
    this.closeDropdown.emit(false)
  }

  hoverSetActiveParentCat(categoryID: string): void {
    this.activeCatID = categoryID
  }

  hoverSetActiveSubCat(categoryID: string): void {
    this.activeSubCatID = categoryID
  }
}
