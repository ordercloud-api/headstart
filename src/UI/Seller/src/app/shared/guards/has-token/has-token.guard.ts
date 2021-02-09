import { Injectable } from '@angular/core'
import { CanActivate, Router } from '@angular/router'
import * as jwtDecode from 'jwt-decode'
import { of, Observable } from 'rxjs'
import { map } from 'rxjs/operators'
import { OcTokenService } from '@ordercloud/angular-sdk'
import { AppStateService } from '@app-seller/shared/services/app-state/app-state.service'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { DecodedOrderCloudToken } from '@app-seller/shared'

@Injectable({
  providedIn: 'root',
})
export class HasTokenGuard implements CanActivate {
  constructor(
    private ocTokenService: OcTokenService,
    private router: Router,
    private appAuthService: AppAuthService,
    private appStateService: AppStateService
  ) {}
  canActivate(): Observable<boolean> {
    /**
     * very simple test to make sure a token exists,
     * can be parsed and has a valid expiration time
     *
     * shouldn't be depended on for actual token validation.
     * if the token is actually not valid it will fail on a call
     * and the refresh-token interceptor will handle it correctly
     */
    const isAccessTokenValid = this.isTokenValid()
    const refreshTokenExists =
      this.ocTokenService.GetRefresh() &&
      this.appAuthService.getRememberStatus()
    if (!isAccessTokenValid && refreshTokenExists) {
      return this.appAuthService.refresh().pipe(map(() => true))
    }

    if (!isAccessTokenValid) {
      this.router.navigate(['/login'])
    }

    this.appStateService.isLoggedIn.next(isAccessTokenValid)
    return of(isAccessTokenValid)
  }

  private isTokenValid(): boolean {
    const token = this.ocTokenService.GetAccess()

    if (!token) {
      return false
    }

    let decodedToken: DecodedOrderCloudToken
    try {
      decodedToken = jwtDecode(token)
    } catch (e) {
      decodedToken = null
    }
    if (!decodedToken) {
      return false
    }

    const expiresIn = decodedToken.exp * 1000
    return Date.now() < expiresIn
  }
}
