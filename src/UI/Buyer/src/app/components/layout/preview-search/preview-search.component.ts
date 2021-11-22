import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core'
import { ReflektionProductSearchResponse } from 'src/app/services/reflektion/models'
import { ReflektionService } from 'src/app/services/reflektion/reflektion.service'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './preview-search.component.html',
  styleUrls: ['./preview-search.component.scss'],
})
export class OCMPreviewSearch implements OnInit {
  @Input() searchTerm?: string
  @Input() results?: ReflektionProductSearchResponse
  @Output() close = new EventEmitter()

  constructor(
    public context: ShopperContextService,
    private reflektion: ReflektionService
  ) {}

  ngOnInit(): void {}

  toProductDetails(productID: string): void {
    this.close.emit(null)
    this.context.router.toProductDetails(productID)
  }

  toFullSearch(searchText: string) {
    this.close.emit(null)
    this.context.router.toProductList({ search: searchText.toLowerCase() })
  }

  async hoverOnSearchPrase(searchPhrase: string) {
    const userID = this.context.currentUser.isAnonymous()
      ? null
      : this.context.currentUser.get().ID
    const data = await this.reflektion.searchPreviewProducts(
      searchPhrase,
      userID
    )
    this.results.content = data.content
  }
}
