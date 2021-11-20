import { faBullhorn } from '@fortawesome/free-solid-svg-icons'
import { Component, OnInit } from '@angular/core'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { HSMeProduct } from '@ordercloud/headstart-sdk'
import {
  hsHomePageHeroBannerWidget,
  hsHomePageTopProducts,
  ReflektionService,
} from 'src/app/services/reflektion/reflektion.service'
import { DomSanitizer, SafeHtml } from '@angular/platform-browser'

@Component({
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class OCMHomePage implements OnInit {
  featuredProducts: HSMeProduct[]
  faBullhorn = faBullhorn
  URL = '../../../assets/jumbotron.svg'
  topProducts: HSMeProduct[]
  heroBannerHtml: SafeHtml

  constructor(
    private context: ShopperContextService,
    private reflectionService: ReflektionService,
    private domSanitizer: DomSanitizer
  ) {}

  async ngOnInit(): Promise<void> {
    const userID = this.context.currentUser.isAnonymous
      ? null
      : this.context.currentUser.get().ID
    const data = await this.reflectionService.getHomePageWidgetData(userID)
    // lol this is pretty hacky
    const css = `<style>${data[hsHomePageHeroBannerWidget].css.devices.pc.content}</style>`
    const html = `<div data-rfkid class="rfk2_banner rfk2_hs-homepage-herobanner">${data[hsHomePageHeroBannerWidget].html.devices.pc.content}</div>`
    this.heroBannerHtml = this.domSanitizer.bypassSecurityTrustHtml(css + html)
    this.topProducts = data[hsHomePageTopProducts]
  }
}
