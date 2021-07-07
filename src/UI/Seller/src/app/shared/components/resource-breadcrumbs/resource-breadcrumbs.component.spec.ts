import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'
import { ActivatedRoute, Router } from '@angular/router'
import { of } from 'rxjs/internal/observable/of'

import { ResourceBreadcrumbsComponent } from './resource-breadcrumbs.component'

describe('ResourceBreadcrumbsComponent', () => {
  let component: ResourceBreadcrumbsComponent
  let fixture: ComponentFixture<ResourceBreadcrumbsComponent>
  const router = {
    events: of({}),
    navigateByUrl: jasmine.createSpy('navigateByUrl'),
    url: 'url/',
    routerState: { snapshot: { url: 'url/' } },
  }
  const activatedRoute = { queryParams: of({}), params: of({}) }

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ResourceBreadcrumbsComponent],
      providers: [
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(ResourceBreadcrumbsComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
