import { Component, OnInit } from '@angular/core'
import { ListPage, HSMeProduct } from '@ordercloud/headstart-sdk'

@Component({
  template: `
    <ocm-home-page [featuredProducts]="featuredProducts.Items"></ocm-home-page>
  `,
})
export class HomeWrapperComponent implements OnInit {
  featuredProducts: ListPage<HSMeProduct>

  constructor() {}

  ngOnInit(): void {
    this.featuredProducts = { Items: [] }
  }
}
