import { Injectable } from '@angular/core'
import { CanActivate, Router } from '@angular/router'
import * as jwtDecode from 'jwt-decode'
import { AppStateService } from '@app-seller/shared/services/app-state/app-state.service'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { DecodedToken, Tokens } from 'ordercloud-javascript-sdk'

@Injectable({
  providedIn: 'root',
})
export class IsSellerGuard implements CanActivate {
  constructor(
    private router: Router,
    private appAuthService: AppAuthService,
    private appStateService: AppStateService
  ) {}
  canActivate(): boolean {
    /**
     * very simple test to make sure a token exists,
     * can be parsed and has a valid expiration time
     *
     * shouldn't be depended on for actual token validation.
     * if the token is actually not valid it will fail on a call
     * and the refresh-token interceptor will handle it correctly
     */
    const isSeller = this.isSeller()

    if (!isSeller) {
      this.appAuthService.logout()
      this.router.navigate(['/login'])
    }

    this.appStateService.isLoggedIn.next(isSeller)
    return isSeller
  }

  private isSeller(): boolean {
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

    return decodedToken.usrtype === 'admin'
  }
}
