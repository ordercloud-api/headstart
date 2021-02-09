import { Injectable } from '@angular/core'
import { CanActivate } from '@angular/router'
import { CurrentUserService } from '../../services/current-user/current-user.service'

@Injectable({
  providedIn: 'root',
})
export class IsProfiledUserGuard implements CanActivate {
  constructor(private currentUser: CurrentUserService) {}

  canActivate(): boolean {
    return !this.currentUser.isAnonymous()
  }
}
