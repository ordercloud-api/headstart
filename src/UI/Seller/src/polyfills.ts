/***************************************************************************************************
 * Load `$localize` onto the global scope - used if i18n tags appear in Angular templates.
 */
import '@angular/localize/init'
/**
 * This file includes polyfills needed by Angular and is loaded before the app.
 * You can add your own extra polyfills to this file.
 *
 * This file is divided into 2 sections:
 *   1. Browser polyfills. These are applied before loading ZoneJS and are sorted by browsers.
 *   2. Application imports. Files imported after ZoneJS that should be loaded before your main
 *      file.
 *
 * The current setup is for so-called "evergreen" browsers; the last versions of browsers that
 * automatically update themselves. This includes Safari >= 10, Chrome >= 55 (including Opera),
 * Edge >= 13 on the desktop, and iOS 10 and Chrome on mobile.
 *
 * Learn more in https://angular.io/guide/browser-support
 */

/***************************************************************************************************
 * BROWSER POLYFILLS
 */

/** IE10 and IE11 requires the following for NgClass support on SVG elements */
// import 'classlist.js';  // Run `npm install --save classlist.js`.

/** IE10 and IE11 requires the following for the Reflect API. */
import 'core-js/es7/reflect'

/**
 * Required to support Web Animations `@angular/platform-browser/animations`.
 * Needed for: All but Chrome, Firefox and Opera. http://caniuse.com/#feat=web-animation
 **/
// import 'web-animations-js';  // Run `npm install --save web-animations-js`.

/***************************************************************************************************
 * Zone JS is required by default for Angular itself.
 */
import 'zone.js' // Included with Angular CLI.

/***************************************************************************************************
 * APPLICATION IMPORTS
 */

/**
 * The following is required by algoliasearch-client-javascript,
 * which was broken by angular v6:
 * https://github.com/algolia/algoliasearch-client-javascript/issues/691
 */
;(window as any).process = { env: {} }

/**
 * this polyfill is to allow us to use avatax clientside
 * I will make a PR to the library to allow clientside and serverside
 * usage, if accepted we can remove this polyfill
 */
// this polyfill is used on the buy app, but ignored here as it overrides the buffer object used in the quill on the product form
// (window as any).Buffer = (stringToEncode) => ({
//   toString: () => btoa(stringToEncode),
// });

/**
 * polyfill for childNode.remove() to work in IE.
 *
 * see https://developer.mozilla.org/en-US/docs/Web/API/ChildNode/remove
 *
 * from:https://github.com/jserz/js_piece/blob/master/DOM/ChildNode/remove()/remove().md
 */
;((arr) => {
  arr.forEach((item) => {
    if (item.hasOwnProperty('remove')) {
      return
    }
    Object.defineProperty(item, 'remove', {
      configurable: true,
      enumerable: true,
      writable: true,
      value: function remove() {
        if (this.parentNode !== null) {
          this.parentNode.removeChild(this)
        }
      },
    })
  })
})([Element.prototype, CharacterData.prototype, DocumentType.prototype])
