import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core'
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router'
import { takeWhile, filter } from 'rxjs/operators'
import { faHome } from '@fortawesome/free-solid-svg-icons'

@Component({
  selector: 'resource-breadcrumbs-component',
  templateUrl: './resource-breadcrumbs.component.html',
  styleUrls: ['./resource-breadcrumbs.component.scss'],
})
export class ResourceBreadcrumbsComponent implements OnInit, OnDestroy {
  faHome = faHome
  breadCrumbs: {
    displayText: string
    route: string
  }[] = []

  private alive = true
  constructor(
    private router: Router,
    private changeDetectorRef: ChangeDetectorRef,
    private activatedRoute: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.listenToRouteChanges()
  }

  private listenToRouteChanges(): void {
    this.router.events
      .pipe(takeWhile(() => this.alive))
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        this.setBreadCrumbs()
      })

    this.activatedRoute.params
      .pipe(takeWhile(() => this.alive))
      .subscribe(() => {
        this.setBreadCrumbs()
      })
  }

  private setBreadCrumbs(): void {
    // basically we are just taking off the portion of the url after the selected route piece
    // in the future breadcrumb logic might need to be more complicated than this
    const urlPieces = this.router.url
      .split('/')
      .filter((p) => p)
      .map((p) => {
        if (p.includes('?')) {
          return p.slice(0, p.indexOf('?'))
        } else {
          return p
        }
      })
    this.breadCrumbs = urlPieces.map((piece, index) => {
      const route = `/${urlPieces.slice(0, index + 1).join('/')}`
      return {
        displayText: piece,
        route,
      }
    })
    this.breadCrumbs = this.breadCrumbs.filter(
      (breadCrumb) => breadCrumb.displayText !== 'clone'
    )
    this.changeDetectorRef.detectChanges()
  }

  ngOnDestroy(): void {
    this.alive = false
  }
}
