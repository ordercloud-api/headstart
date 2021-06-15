import { Component, Input, Output, EventEmitter } from '@angular/core'
import { Category } from 'ordercloud-javascript-sdk'
import {
  faSearch,
  faShoppingCart,
  faPhone,
  faQuestionCircle,
  faUserCircle,
  faSignOutAlt,
  faBoxOpen,
  faHome,
  faBars,
  faTimes,
  faCaretRight,
} from '@fortawesome/free-solid-svg-icons'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { CurrentUser } from 'src/app/models/profile.types'

@Component({
  templateUrl: './category-dropdown.component.html',
  styleUrls: ['./category-dropdown.component.scss'],
})
export class OCMCategoryDropdown {
  activeCatID = ''
  activeSubCatID = ''
  user: CurrentUser
  isAnonymous: boolean

  @Output() closeDropdown = new EventEmitter()
  @Input() set categories(value: Category[]) {
    this.parentCategories = this.assignCategories(value, 'parent')
    this.subCategories = this.assignCategories(value, 'subCategory')
    this.subSubCategories = this.assignCategories(value, 'subSubCategory')
  }

  parentCategories: Category[] = []
  subCategories: Category[] = []
  subSubCategories: Category[] = [] // Intentionally restricted to three levels.

  faSearch = faSearch
  faShoppingCart = faShoppingCart
  faPhone = faPhone
  faQuestionCircle = faQuestionCircle
  faSignOutAlt = faSignOutAlt
  faUserCircle = faUserCircle
  faHome = faHome
  faBars = faBars
  faTimes = faTimes
  faBoxOpen = faBoxOpen
  faCaretRight = faCaretRight
  flagIcon: string

  constructor(
    private context: ShopperContextService,
  ) { }

  ngOnInit(): void {
    this.isAnonymous = this.context.currentUser.isAnonymous()
    this.context.currentUser.onChange((user) => (this.user = user))
    this.flagIcon = this.getCurrencyFlag()
  }

  getCurrencyFlag(): string {
    const rates = this.context.exchangeRates.Get()
    const currentUser = this.context.currentUser.get()
    const myRate = rates?.Items.find((r) => r.Currency === currentUser.Currency)
    return myRate?.Icon
  }

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
