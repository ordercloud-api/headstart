import { Injectable } from '@angular/core'
import { HSLineItem, HSOrder, HSProduct } from '@ordercloud/headstart-sdk'
import { AppConfig } from 'src/app/models/environment.types'
import { CurrentUserService } from '../current-user/current-user.service'
import { RouteService } from '../route/route.service'
import {
  SitecoreCDPEvent,
  CDPEventType,
  SitecoreCDPSearchEvent,
  SitecoreCDPIdentifyEvent,
  SitecoreCDPAddEvent,
  SitecoreCDPPurchaseEvent,
} from './sitecore-cdp.types'

declare let _boxeverq: any
declare let Boxever: any

/** Track browser events (especially commerce events) and forward to Sitecore CDP (Boxever)*/
@Injectable({
  providedIn: 'root',
})
export class SitecoreCDPTrackingService {
  constructor(
    private appConfig: AppConfig,
    private userService: CurrentUserService,
    private routeService: RouteService
  ) {
    if (!appConfig.useSitecoreCDP) {
      return
    }
    this.loadCDPTracker()
    this.routeService.onUrlChange((_) => {
      this.newPageViewed()
    })
  }

  /**
   * Capture IDENTITY events wherever in the site that the guest provides data that might help identify them. It is common for a single browser session to have multiple IDENTITY events.
   * */
  identify(): void {
    if (!this.appConfig.useSitecoreCDP) {
      return
    }
    const event = this.buildBaseEvent('IDENTITY') as SitecoreCDPIdentifyEvent

    const user = this.userService.get()
    event.email = user.Email
    event.firstname = user.FirstName
    event.lastname = user.LastName
    event.phone = user.Phone
    event.identifiers = {
      provider: 'ordercloud',
      id: this.userService.getUniqueReportingID(),
    }

    this.sendEventToCDP(event)
  }

  /**
   * The ADD event captures the product details when a user adds the product(s) to their online cart.
   */
  addToCart(lineItem: HSLineItem): void {
    if (!this.appConfig.useSitecoreCDP) {
      return
    }
    const event = this.buildBaseEvent('ADD') as SitecoreCDPAddEvent

    event.product = {
      type: '',
      item_id: lineItem.ProductID,
      name: lineItem.Product.Name,
      orderedAt: new Date().toISOString(),
      quantity: lineItem.Quantity,
      price: lineItem.UnitPrice,
      productId: lineItem.ProductID,
      currency: this.userService.get()?.Locale?.Currency || 'USD',
      originalPrice: lineItem.UnitPrice,
      referenceId: lineItem.ID,
    }

    this.sendEventToCDP(event)
  }

  /**
   *  The VIEW event is a simple but important event, because it captures the guest's action of viewing a page. You must capture VIEW events on all pages where you want to track guest behavior.
   *  https://doc.sitecore.com/cdp/en/developers/sitecore-customer-data-platform--data-model-2-1/send-a-view-event-to-the-sitecore-cdp.html
   */
  newPageViewed(): void {
    if (!this.appConfig.useSitecoreCDP) {
      return
    }
    const event = this.buildBaseEvent('VIEW')
    this.sendEventToCDP(event)
  }

  /**
   * The SEARCH event captures the user's action of searching for a product.
   */
  productSearched(searchTerm: string): void {
    if (!this.appConfig.useSitecoreCDP) {
      return
    }
    const event = this.buildBaseEvent('SEARCH') as SitecoreCDPSearchEvent

    event.product_name = searchTerm
    event.product_type = ''

    this.sendEventToCDP(event)
  }

  orderPlaced(order: HSOrder, lineItems: HSLineItem[]): void {
    if (!this.appConfig.useSitecoreCDP) {
      return
    }
    const event = this.buildBaseEvent(
      'ORDER_CHECKOUT'
    ) as SitecoreCDPPurchaseEvent

    event.order = {
      referenceId: order.ID,
      orderedAt: new Date().toISOString(),
      status: 'PURCHASED',
      currencyCode: order.Currency,
      price: order.Total,
      paymentType: 'Card',
      orderItems: lineItems.map((lineItem) => {
        return {
          referenceId: lineItem.ID,
          price: lineItem.LineTotal,
          name: lineItem.Product.Name,
          productId: lineItem.ProductID,
          quantity: lineItem.Quantity,
        }
      }),
    }

    this.sendEventToCDP(event)
  }

  /**
   * You can send a CLEAR_CART event to ensure that Sitecore CDP ignores any products or contacts that have been passed to Sitecore CDP during the browser session.
   * https://doc.sitecore.com/cdp/en/developers/sitecore-customer-data-platform--data-model-2-1/send-a-clear-cart-event-to-sitecore-cdp.html
   */
  clearCart(): void {
    if (!this.appConfig.useSitecoreCDP) {
      return
    }
    const event = this.buildBaseEvent('CLEAR_CART')
    this.sendEventToCDP(event)
  }

  private buildBaseEvent(type: CDPEventType): SitecoreCDPEvent {
    const currentUser = this.userService.get()
    return {
      channel: this.isBrowserMobile() ? 'MOBILE_WEB' : 'WEB',
      type,
      language: currentUser?.Locale?.Language || 'en-US',
      currency: currentUser?.Locale?.Currency || 'USD',
      page: this.routeService.getActiveUrl(),
      pos: this.appConfig.baseUrl,
      browser_id: Boxever.getID(),
    }
  }

  private isBrowserMobile(): boolean {
    let check = false
    ;(function (a) {
      if (
        /(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(
          a
        ) ||
        /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(
          a.substr(0, 4)
        )
      )
        check = true
    })(navigator.userAgent || navigator.vendor || (window as any).opera)
    return check
  }

  private sendEventToCDP(event: SitecoreCDPEvent): void {
    _boxeverq.push(function () {
      Boxever.eventCreate(event, function (data) {}, 'json')
    })
  }

  private loadCDPTracker(): void {
    const clientKey = this.appConfig.sitecoreCDPApiClient
    const target = this.appConfig.sitecoreCDPTargetEndpoint
    const domain = this.appConfig.baseUrl
    const clientVersion = '1.4.8'
    const node = document.createElement('script')
    node.type = 'text/javascript'
    node.async = true
    // Tracker installation https://doc.sitecore.com/cdp/en/developers/sitecore-customer-data-platform--data-model-2-1/javascript-tagging-examples-for-web-pages.html
    node.innerHTML = `
            // Define the Boxever queue
            var _boxeverq = _boxeverq || [];

            // Define the Boxever settings
            var _boxever_settings = {
                client_key: '${clientKey}', // Replace with your client key
                target: '${target}', // Replace with your API target endpoint specific to your data center region
                cookie_domain: '${domain}' // Replace with the top level cookie domain of the website that is being integrated e.g ".example.com" and not "www.example.com"
            };
            // Import the Boxever library asynchronously
            (function() {
                var s = document.createElement('script'); s.type = 'text/javascript'; s.async = true;
                s.src = 'https://d1mj578wat5n4o.cloudfront.net/boxever-${clientVersion}.min.js';
                var x = document.getElementsByTagName('script')[0]; x.parentNode.insertBefore(s, x);
            })();
        `
    document.getElementsByTagName('head')[0].appendChild(node)
  }
}
