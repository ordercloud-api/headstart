import { Component, Input, Output, EventEmitter } from '@angular/core'
import { OcCategoryService, Category } from '@ordercloud/angular-sdk'
import {
  faTimes,
  faAngleRight,
  faTimesCircle,
} from '@fortawesome/free-solid-svg-icons'

@Component({
  selector: 'product-category-assignment',
  templateUrl: './product-category-assignment.component.html',
  styleUrls: ['./product-category-assignment.component.scss'],
})
export class ProductCategoryAssignment {
  @Input()
  productID = ''

  _assignedCategoriesStatic: Category[][] = []
  _assignedCategoriesEditable: Category[][] = []

  _catalogID = ''

  @Input()
  set assignedCategoriesStatic(value: Category[][]) {
    this.resetCategoryAssignment(value)
  }
  @Input()
  set catalogID(value: string) {
    this._catalogID = value
    this.getTopLevelCategories()
  }

  @Output()
  assignmentsUpdated = new EventEmitter()
  // could this all have been done drier with the ability to handle any number of category depths probably
  // just decided to keep do it with seperate variables for each level for simiplicity since there are only 3 levels

  highLevelOptions: Category[] = []
  midLevelOptions: Category[] = []
  lowLevelOptions: Category[] = []

  highLevelSelection = ''
  midLevelSelection = ''
  lowLevelSelection = ''

  showMidLevel = false
  showLowLevel = false

  assignedCategoryIDsStatic: string[] = []
  assignedCategoryIDsEditable: string[] = []

  faAngleRight = faAngleRight
  faTimesCircle = faTimesCircle

  constructor(private ocCategoryService: OcCategoryService) {}

  async getTopLevelCategories(): Promise<void> {
    const highLevelCategories = await this.ocCategoryService
      .List(this._catalogID, { pageSize: 100, depth: '1' })
      .toPromise()
    this.highLevelOptions = highLevelCategories.Items
  }

  resetCategoryAssignment(categorys: Category[][]): void {
    this._assignedCategoriesStatic = categorys
    this._assignedCategoriesEditable = this._assignedCategoriesStatic
  }

  async getSubcategories(categoryID: string): Promise<Category[]> {
    return (
      await this.ocCategoryService
        .List(this._catalogID, {
          depth: '1',
          pageSize: 100,
          filters: { ParentID: categoryID },
        })
        .toPromise()
    ).Items
  }

  async selectHighLevelCategory(event: any): Promise<void> {
    const categoryID = event.target.value
    this.highLevelSelection = categoryID
    this.midLevelSelection = ''
    this.lowLevelSelection = ''
    this.showMidLevel = false
    this.showLowLevel = false
    const subCategories = await this.getSubcategories(categoryID)
    this.midLevelOptions = subCategories
  }

  async selectMidLevelCategory(event: any): Promise<void> {
    const categoryID = event.target.value
    this.midLevelSelection = categoryID
    this.lowLevelSelection = ''
    this.showLowLevel = false
    const subCategories = await this.getSubcategories(categoryID)
    this.lowLevelOptions = subCategories
  }

  selectLowLevelCategory(event: any): void {
    const categoryID = event.target.value
    this.lowLevelSelection = categoryID
  }

  addAssignment(): void {
    const newAssignmentHierarchy = [
      this.highLevelOptions.find((h) => h.ID === this.highLevelSelection),
    ]
    if (this.midLevelSelection) {
      newAssignmentHierarchy.push(
        this.midLevelOptions.find((m) => m.ID === this.midLevelSelection)
      )
    }
    if (this.lowLevelSelection) {
      newAssignmentHierarchy.push(
        this.lowLevelOptions.find((m) => m.ID === this.lowLevelSelection)
      )
    }
    this._assignedCategoriesEditable = [
      ...this._assignedCategoriesEditable,
      newAssignmentHierarchy,
    ]
    this.assignmentsUpdated.emit(this._assignedCategoriesEditable)
  }

  getAssignedCategoryDislay(categoryHierarchy: Category[]): string {
    return categoryHierarchy.map((c) => c.Name).join('->')
  }
  removeAssignment(indexToRemove: number): void {
    const assignedCategoriesCopy = JSON.parse(
      JSON.stringify(this._assignedCategoriesEditable)
    )
    assignedCategoriesCopy.splice(indexToRemove, 1)
    this._assignedCategoriesEditable = assignedCategoriesCopy
    this.assignmentsUpdated.emit(assignedCategoriesCopy)
  }
}
