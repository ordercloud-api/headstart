import { Injectable } from '@angular/core'
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse,
} from '@angular/common/http'
import { Observable, throwError } from 'rxjs'
import { catchError, filter, flatMap } from 'rxjs/operators'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'

/**
 * handle 401 unauthorized responses gracefully
 * by attempting to refresh token
 */
@Injectable({
  providedIn: 'root',
})
export class RefreshTokenInterceptor implements HttpInterceptor {
  constructor(private appAuthService: AppAuthService) {}
  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error) => {
        // rethrow any non auth errors
        if (!this.isAuthError(error)) {
          return throwError(error)
        } else {
          // if a refresh attempt failed recently then ignore (3 seconds)`
          if (this.appAuthService.failedRefreshAttempt) {
            return
          }

          // ensure there is no outstanding request already fetching token
          // if there is then wait for the token to resolve
          const refreshToken = this.appAuthService.refreshToken.getValue()
          if (refreshToken || this.appAuthService.fetchingRefreshToken) {
            return this.appAuthService.refreshToken.pipe(
              filter((token) => token !== ''),
              flatMap((token) => {
                request = request.clone({
                  setHeaders: { Authorization: `Bearer ${token}` },
                })
                return next.handle(request)
              })
            )
          } else {
            // attempt refresh for new token
            return this.appAuthService.refresh().pipe(
              flatMap((token) => {
                request = request.clone({
                  setHeaders: { Authorization: `Bearer ${token}` },
                })
                return next.handle(request)
              })
            )
          }
        }
      })
    )
  }

  isAuthError(error: any): boolean {
    return (
      error instanceof HttpErrorResponse &&
      error.url.indexOf('ordercloud.io') > -1 &&
      error.status === 401
    )
  }
}
