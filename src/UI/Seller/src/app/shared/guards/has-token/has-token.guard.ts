import { Injectable } from '@angular/core'
import { CanActivate, Router } from '@angular/router'
import * as jwtDecode from 'jwt-decode'
import { Me, Tokens } from 'ordercloud-javascript-sdk'
import { AppStateService } from '@app-seller/shared/services/app-state/app-state.service'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { DecodedToken } from 'ordercloud-javascript-sdk'

@Injectable({
  providedIn: 'root',
})
export class HasTokenGuard implements CanActivate {
  constructor(
    private router: Router,
    private appAuthService: AppAuthService,
    private appStateService: AppStateService
  ) {}
  async canActivate(): Promise<boolean> {
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
      Tokens.GetRefreshToken() && this.appAuthService.getRememberStatus()
    if (!isAccessTokenValid && refreshTokenExists) {
      await Me.Get() // this will force access token to be refreshed (via the javascript sdk) and if it can't will log the user out
      return true
    }

    if (!isAccessTokenValid) {
      this.router.navigate(['/login'])
    }

    this.appStateService.isLoggedIn.next(isAccessTokenValid)
    return isAccessTokenValid
  }

  private isTokenValid(): boolean {
    const token = Tokens.GetAccessToken()

    if (!token) {
      return false
    }

    let decodedToken: DecodedToken
    try {
      decodedToken = jwtDecode(token) as DecodedToken
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
