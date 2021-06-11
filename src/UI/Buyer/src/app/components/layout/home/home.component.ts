import { faBullhorn } from '@fortawesome/free-solid-svg-icons'
import { Component, OnInit } from '@angular/core'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { HSMeProduct } from '@ordercloud/headstart-sdk'

@Component({
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class OCMHomePage implements OnInit {
  featuredProducts: HSMeProduct[]
  faBullhorn = faBullhorn
  URL = '../../../assets/jumbotron.svg'

  constructor(
    private context: ShopperContextService,
  ) { }

  async ngOnInit(): Promise<void> {
    const user = this.context.currentUser.get()
    if (!user?.UserGroups?.length) {
      this.featuredProducts = []
    } else {
      const products = await this.context.tempSdk.listMeProducts({
        filters: { 'xp.Featured': true },
      })
      this.featuredProducts = products.Items
    }
  }

  toSupplier(supplier: string): void {
    this.context.router.toProductList({
      activeFacets: { Supplier: supplier.toLocaleLowerCase() },
    })
  }
}
