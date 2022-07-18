import { Injectable } from '@angular/core'
import { CanActivate, Router } from '@angular/router'
import * as jwtDecode from 'jwt-decode'
import { of, Observable } from 'rxjs'
import { Tokens } from 'ordercloud-javascript-sdk'
import { AppStateService } from '@app-seller/shared/services/app-state/app-state.service'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { DecodedToken } from 'ordercloud-javascript-sdk'

@Injectable({
  providedIn: 'root',
})
export class CanEditMySupplierGuard implements CanActivate {
  constructor(
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
    const canEdit = this.canEditMySupplier()

    if (!canEdit) {
      this.appAuthService.logout()
      this.router.navigate(['/login'])
    }

    this.appStateService.isLoggedIn.next(canEdit)
    return of(canEdit)
  }

  private canEditMySupplier(): boolean {
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

    return decodedToken.role.includes('HSMeSupplierAdmin')
  }
}
