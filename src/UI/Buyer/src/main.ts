import { enableProdMode } from '@angular/core'
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic'

import { AppModule } from './app/app.module'
import { environment } from './environments/environment.deploy'

declare var __webpack_public_path__: string;

//  set the __webpack_public_path__ depending on if the first path after the base url is the storefront name.
//  Do this for automatic deployments in which files are copied to a folder in blob storage named the storefront name
//  This allows assets to be referenced correctly.

if (document.location.pathname.split('/')[1] === environment.storefrontName) {
  __webpack_public_path__ = `${window.location.origin}/${environment?.storefrontName}/`
}

if (environment.hostedApp) {
  enableProdMode()
}

platformBrowserDynamic()
  .bootstrapModule(AppModule)
  .catch((err) => console.error(err))
