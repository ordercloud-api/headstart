/* eslint-disable max-lines-per-function */
import {
  HttpErrorResponse, HttpEvent, HttpHandler,

  HttpInterceptor, HttpRequest
} from '@angular/common/http'
import { Injectable } from '@angular/core'
import { Observable, throwError } from 'rxjs'
import { catchError, filter, flatMap } from 'rxjs/operators'
import { ApplicationInsightsService } from 'src/app/services/application-insights/application-insights.service'
import { TokenHelperService } from 'src/app/services/token-helper/token-helper.service'
import { AuthService } from '../../services/auth/auth.service'

/**
 * handle 401 unauthorized responses gracefully
 * by attempting to refresh token
 */
@Injectable({
  providedIn: 'root',
})
export class RefreshTokenInterceptor implements HttpInterceptor {
  constructor(
    private appAuthService: AuthService, 
    private appInsightsSerivce: ApplicationInsightsService, 
    private tokenHelper: TokenHelperService
  ) {}
  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error) => {
        const decodedToken = this.tokenHelper.getDecodedOCToken()
        // rethrow any non auth errors
        if (!this.isAuthError(error)) {
          return throwError(error)
        } else {
          // if a refresh attempt failed recently then ignore (3 seconds)`
          if (this.appAuthService.failedRefreshAttempt) {
            this.appInsightsSerivce.trackAuthErrorEvents(decodedToken, error)
            return throwError(error)
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
              }),
              catchError((err) => {
                this.appInsightsSerivce.trackAuthErrorEvents(decodedToken, err)
                return throwError(err)
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
              }),
              catchError((err) => {
                this.appInsightsSerivce.trackAuthErrorEvents(decodedToken, err)
                return throwError(err)
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
      error.url.includes('ordercloud.io') &&
      error.status === 401
    )
  }
}
