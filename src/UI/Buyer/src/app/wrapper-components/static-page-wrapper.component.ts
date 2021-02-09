import { Component } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { StaticPageService } from '../services/static-page/static-page.service'

@Component({
  template: '<ocm-static-page [page]="page"></ocm-static-page>',
})
export class StaticPageWrapperComponent {
  constructor(
    private activatedRoute: ActivatedRoute,
    private staticPagesService: StaticPageService
  ) {}

  // TODO: add PageDocument type to cms library so this is strongly typed
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  get page(): any {
    const url = this.activatedRoute.snapshot.params.staticPageUrl
    return this.staticPagesService.pages.find((page) => page.Doc.Url === url)
  }
}
