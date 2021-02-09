import { Component } from '@angular/core'
import { RouteConfig } from 'src/app/models/shared.types'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './profile-nav.component.html',
  styleUrls: ['./profile-nav.component.scss'],
})
export class OCMProfileNav {
  profileRoutes: RouteConfig[] = []

  constructor(public context: ShopperContextService) {
    const isSSO =
      context.currentUser.isSSO() ||
      !context.currentUser.hasRoles('PasswordReset')
    this.profileRoutes = context.router.getProfileRoutes()
    if (isSSO)
      this.profileRoutes = this.profileRoutes.filter(
        (r) => r.routerCall !== 'toChangePassword'
      )
  }
}
