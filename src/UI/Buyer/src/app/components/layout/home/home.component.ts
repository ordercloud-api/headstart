import { faBullhorn } from '@fortawesome/free-solid-svg-icons'
import { Component, OnInit } from '@angular/core'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { StaticPageService } from 'src/app/services/static-page/static-page.service'
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
    public staticPageService: StaticPageService
  ) {}

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

  // TODO: add PageDocument type to cms library so this is strongly typed
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  get homePageDoc(): any {
    return this.staticPageService.pages.find((page) => page.Doc.Url === 'home')
  }

  toSupplier(supplier: string): void {
    this.context.router.toProductList({
      activeFacets: { Supplier: supplier.toLocaleLowerCase() },
    })
  }
}
