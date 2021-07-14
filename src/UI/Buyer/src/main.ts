import { enableProdMode } from '@angular/core'
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic'

import { AppModule } from './app/app.module'
import { environment } from './environments/environment.deploy'
import { environment as environmentLocal } from './environments/environment.local'

declare var __webpack_public_path__: string;

//  set the __webpack_public_path__ depending on if the first path after the base url is the storefront name.
//  Do this for automatic deployments in which files are copied to a folder in blob storage named the storefront name
//  This allows assets to be referenced correctly.
console.log('deployed environment')
console.log(environment)

console.log('local environment')
console.log(environmentLocal)

if (document.location.pathname.split('/')[1] === environment.storefrontName) {
  console.log('setting __webpack_public_path__')
  __webpack_public_path__ = `${window.location.origin}/${environment?.storefrontName}/`
}

declare var __webpack_public_path__: string;


if (environment.hostedApp) {
  enableProdMode()
}

platformBrowserDynamic()
  .bootstrapModule(AppModule)
  .catch((err) => console.error(err))
