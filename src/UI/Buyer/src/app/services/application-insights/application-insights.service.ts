import { Injectable } from '@angular/core'
import { ActivatedRouteSnapshot, ResolveEnd, Router } from '@angular/router'
import {
  ApplicationInsights,
  IEventTelemetry,
} from '@microsoft/applicationinsights-web'
import { Subscription } from 'rxjs'
import { filter } from 'rxjs/operators'
import { AppConfig } from 'src/app/models/environment.types'
import { DecodedOCToken } from 'src/app/models/profile.types'
import { ShopperContextService } from '../shopper-context/shopper-context.service'

@Injectable({
  providedIn: 'root',
})
export class ApplicationInsightsService {
  appInsights: ApplicationInsights = null
  routerSubscription: Subscription

  constructor(
    private appConfig: AppConfig,
    private context: ShopperContextService,
    private router: Router
  ) {
    let isDeployed = false
    if (window) {
      isDeployed = window.location.hostname !== 'localhost'
    }
    if (this.appConfig.appInsightsInstrumentationKey && isDeployed) {
      this.appInsights = new ApplicationInsights({
        config: {
          instrumentationKey: this.appConfig.appInsightsInstrumentationKey,
          enableAutoRouteTracking: true,
        },
      })
      this.appInsights.loadAppInsights()
      this.createRouterSubscription()
    }
  }

  public setUserID(username: string): void {
    if (this.appInsights) {
      this.appInsights.setAuthenticatedUserContext(username, null, true)
    }
  }

  public clearUser(): void {
    if (this.appInsights) {
      this.appInsights.clearAuthenticatedUserContext()
    }
  }

  public trackAuthErrorEvents(
    decodedToken?: DecodedOCToken,
    error?: any
  ): void {
    const currentUser = this.context.currentUser.get()
    const event: IEventTelemetry = {
      name: 'AuthError',
      properties: {
        userId: decodedToken?.usr ?? currentUser?.ID,
        buyerId: currentUser?.Buyer?.ID,
        tokenIssuedAt: decodedToken?.nbf,
        tokenExpAt: decodedToken?.exp,
        error,
      },
    }
    this.trackEvent(event)
  }

  private trackEvent(event: IEventTelemetry): void {
    if (this.appInsights) {
      this.appInsights.trackEvent(event)
    }
  }

  private createRouterSubscription(): void {
    this.router.events
      .pipe(filter((event) => event instanceof ResolveEnd))
      .subscribe((event: ResolveEnd) => {
        const activatedComponent = this.getActivatedComponent(event.state.root)
        if (activatedComponent) {
          this.logPageView(
            `${activatedComponent.name} ${this.getRouteTemplate(
              event.state.root
            )}`,
            event.urlAfterRedirects
          )
        } else {
          this.logPageView(null, event.urlAfterRedirects)
        }
      })
  }

  private logPageView(name?: string, uri?: string): void {
    this.appInsights.trackPageView({ name, uri })
  }

  private getActivatedComponent(snapshot: ActivatedRouteSnapshot): any {
    if (snapshot.firstChild) {
      return this.getActivatedComponent(snapshot.firstChild)
    }
    return snapshot.component
  }

  private getRouteTemplate(snapshot: ActivatedRouteSnapshot): string {
    let path = ''
    if (snapshot.routeConfig) {
      path += snapshot.routeConfig.path
    }
    if (snapshot.firstChild) {
      return path + this.getRouteTemplate(snapshot.firstChild)
    }
    return path
  }
}
