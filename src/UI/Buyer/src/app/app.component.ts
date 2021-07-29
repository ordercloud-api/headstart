import { Component, OnInit } from '@angular/core'
import { ShopperContextService } from './services/shopper-context/shopper-context.service'

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  showHeader = false
  // TODO: this shouln't have hard coded routes. its gross.
  hiddenRoutes = ['/login', '/register', '/forgot-password', '/reset-password']

  constructor(
    public context: ShopperContextService,
  ) { }

  ngOnInit(): void {
    this.context.router.setPageTitle();
    this.context.router.onUrlChange((url) => {
      this.showHeader = !this.hiddenRoutes.some((el) => url.includes(el))
      if (!url.includes('products/')) {
        this.context.router.setPageTitle(this.getPageTitle(url));
      }
    })
  }

  ngAfterViewChecked(): void {
    this.updateImageSources();
  }

  getPageTitle(url: string): string {
    //  handle case where there are query params. Don't need these in page title
    if (url.includes('?')) {
      url = url.split("?")[0];
    }
    const routeArray = url.split("/")
    const title = routeArray[routeArray.length - 1];
    return title.charAt(0).toUpperCase() + title.slice(1)
  }

  updateImageSources(): void {
    var prefix = '<PLACEHOLDER>'
    // Only update image src values when using the standard endpoint for the Azure Storage Account
    if (document.location.hostname.indexOf('core.windows.net') !== -1) {
      var images = document.getElementsByTagName('img')    
      for (let i = 0; i < images.length; i++) {
        var imageSRC = images[i].getAttribute('src')
        if (
          imageSRC &&
          !imageSRC.includes(prefix)
        ) {
          // Check for the use of absolute URLs.
          // Update only if using relative URLs.
          var r = new RegExp('^(?:[a-z]+:)?//', 'i');
          if(!r.test(imageSRC)){
            var newSRC = prefix + '/' + imageSRC
            var srcArray = newSRC.split("/")
            srcArray = srcArray.filter(Boolean)
            images[i].src = srcArray.join("/")
          }
        }
      }
    }
  }
}
