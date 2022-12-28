import { Component, OnInit } from '@angular/core'
import { RouteConfig } from 'src/app/models/shared.types'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './profile-nav.component.html',
  styleUrls: ['./profile-nav.component.scss'],
})
export class OCMProfileNav implements OnInit {
  profileRoutes: RouteConfig[] = []

  constructor(private context: ShopperContextService) {}

  ngOnInit(): void {
    const isSSO =
      this.context.currentUser.isSSO() ||
      !this.context.currentUser.hasRoles('PasswordReset')
    this.profileRoutes = this.context.router.getProfileRoutes()
    if (isSSO)
      this.profileRoutes = this.profileRoutes.filter(
        (r) => r.routerCall !== 'toChangePassword'
      )
  }
}
