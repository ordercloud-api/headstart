import { Component, ChangeDetectorRef, Inject, Input } from '@angular/core'
import { ActivatedRoute, Router } from '@angular/router'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { AccountContent } from '@app-seller/shared/components/account-content/account-content.component'
import { MeUser } from '@ordercloud/angular-sdk'
import { faEdit } from '@fortawesome/free-solid-svg-icons'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { AppConfig } from '@app-seller/models/environment.types'

@Component({
  selector: 'account-summary',
  templateUrl: './account-summary.component.html',
  styleUrls: ['./account-summary.component.scss'],
})
export class AccountSummaryComponent extends AccountContent {
  @Input()
  user: MeUser
  faEdit = faEdit
  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    changeDetectorRef: ChangeDetectorRef,
    currentUserService: CurrentUserService,
    @Inject(applicationConfiguration) appConfig: AppConfig,
    appAuthService: AppAuthService,
  ) {
    super(
      router,
      activatedRoute,
      changeDetectorRef,
      appConfig,
      appAuthService,
      currentUserService,
    )
  }
}
