import { Injectable } from '@angular/core'
import { CanActivate, Router } from '@angular/router'
import * as jwtDecode from 'jwt-decode'
import { of, Observable } from 'rxjs'
import { OcTokenService } from '@ordercloud/angular-sdk'
import { AppStateService } from '@app-seller/shared/services/app-state/app-state.service'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { DecodedOrderCloudToken } from '@app-seller/shared'

@Injectable({
  providedIn: 'root',
})
export class CanEditMySupplierGuard implements CanActivate {
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
    const canEdit = this.canEditMySupplier()

    if (!canEdit) {
      this.appAuthService.logout()
      this.router.navigate(['/login'])
    }

    this.appStateService.isLoggedIn.next(canEdit)
    return of(canEdit)
  }

  private canEditMySupplier(): boolean {
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

    return decodedToken.role.includes('HSMeSupplierAdmin')
  }
}
