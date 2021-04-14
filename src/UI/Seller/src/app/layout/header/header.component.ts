import { Component, OnInit, Inject } from '@angular/core'
import { applicationConfiguration } from '@app-seller/config/app.config'
import {
  faBoxOpen,
  faSignOutAlt,
  faUser,
  faUsers,
  faMapMarkerAlt,
  faSitemap,
  faUserCircle,
  faEnvelope,
} from '@fortawesome/free-solid-svg-icons'
import { MeUser, OcTokenService } from '@ordercloud/angular-sdk'
import { Router, NavigationEnd } from '@angular/router'
import { AppConfig, AppStateService, HSRoute } from '@app-seller/shared'
import { getHeaderConfig } from './header.config'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'

@Component({
  selector: 'layout-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent implements OnInit {
  user: MeUser
  organizationName: string
  isSupplierUser: boolean
  isCollapsed = true
  faBoxOpen = faBoxOpen
  faUser = faUser
  faSignOutAlt = faSignOutAlt
  faUsers = faUsers
  faMapMarker = faMapMarkerAlt
  faSitemap = faSitemap
  faUserCircle = faUserCircle
  faEnvelope = faEnvelope
  activeTitle = ''
  headerConfig: HSRoute[]
  hasProfileImg = false
  currentUserInitials: string

  constructor(
    private ocTokenService: OcTokenService,
    private router: Router,
    private appStateService: AppStateService,
    private appAuthService: AppAuthService,
    private currentUserService: CurrentUserService,
    @Inject(applicationConfiguration) protected appConfig: AppConfig
  ) {
    this.setUpSubs()
  }

  async ngOnInit(): Promise<void> {
    this.headerConfig = getHeaderConfig(
      this.appAuthService.getUserRoles(),
      this.appAuthService.getOrdercloudUserType()
    )
    await this.getCurrentUser()
    this.setCurrentUserInitials(this.user)
    this.urlChange(this.router.url)
  }

  async getCurrentUser() {
    this.isSupplierUser = await this.currentUserService.isSupplierUser()
    if (this.isSupplierUser) {
      this.getSupplierOrg()
    } else {
      this.organizationName = this.appConfig.sellerName
    }
  }

  async getSupplierOrg() {
    const mySupplier = await this.currentUserService.getMySupplier()
    this.organizationName = mySupplier.Name
  }

  setUpSubs(): void {
    this.currentUserService.userSubject.subscribe((user) => {
      this.user = user
      this.setCurrentUserInitials(this.user)
    })
    this.currentUserService.profileImgSubject.subscribe((img) => {
      this.hasProfileImg = Object.keys(img).length > 0
    })
    this.router.events.subscribe((ev) => {
      if (ev instanceof NavigationEnd) {
        this.urlChange(ev.url)
      }
    })
  }

  urlChange = (url: string) => {
    const activeNavGroup = this.headerConfig.find((grouping) => {
      return (
        (url.includes(grouping.route) && grouping.subRoutes) ||
        grouping.route === url
      )
    })
    this.activeTitle = activeNavGroup && activeNavGroup.title
  }

  logout() {
    this.ocTokenService.RemoveAccess()
    this.appStateService.isLoggedIn.next(false)
    this.router.navigate(['/login'])
  }

  toAccount(): void {
    this.router.navigate(['account'])
  }

  toNotifications(): void {
    this.router.navigate(['account/notifications'])
  }

  setCurrentUserInitials(user: MeUser): void {
    const firstFirst = user?.FirstName?.substr(0, 1)
    const firstLast = user?.LastName?.substr(0, 1)
    this.currentUserInitials = `${firstFirst}${firstLast}`
  }
}
