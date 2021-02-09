import { Injectable, PLATFORM_ID, Inject } from '@angular/core'
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
} from '@angular/common/http'
import { Observable } from 'rxjs'
import { isPlatformBrowser } from '@angular/common'

/**
 * append headers to disable IE11's aggressive caching of GET requests
 * see here for more info on bug: https://medium.com/@tltjr/disabling-internet-explorer-caching-in-your-angular-5-application-3e148f4437ad
 *
 * we've augmented the suggested solution to check  first if the browser
 * is IE11 and to ensure the request is a 'GET' as those are the affected calls
 * we'd like to keep caching mechanisms for other browsers and request types because
 * some things should rightfully be cached
 */
@Injectable({
  providedIn: 'root',
})
export class CacheInterceptor implements HttpInterceptor {
  constructor(@Inject(PLATFORM_ID) private platformId: Record<string, any>) {}
  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const hasIE11 = false
    if (isPlatformBrowser(this.platformId)) {
      window.navigator.userAgent.includes('Trident/')
    }
    if (hasIE11 && request.method === 'GET') {
      request = request.clone({
        setHeaders: {
          'Cache-Control': 'no-cache',
          Pragma: 'no-cache',
          Expires: 'Sat, 01 Jan 2000 00:00:00 GMT',
        },
      })
    }
    return next.handle(request)
  }
}
