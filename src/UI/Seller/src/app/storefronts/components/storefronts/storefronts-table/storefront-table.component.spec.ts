import { StorefrontsService } from './../storefronts.service'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'

import { StorefrontTableComponent } from './storefront-table.component'
import { HttpClient } from '@angular/common/http'
import { Router, ActivatedRoute } from '@angular/router'
import { of } from 'rxjs'

describe('StorefrontTableComponent', () => {
  let component: StorefrontTableComponent
  let fixture: ComponentFixture<StorefrontTableComponent>
  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl'),
    url: '/my-',
    routerState: { snapshot: { url: 'https://test' } },
  }
  const activatedRoute = { params: of({}) }
  const httpClient = {}
  const storefrontService = {
    isSupplierUser() {
      return true
    },
    getMyResource() {
      return {}
    },
    copyResource() {},
    getParentResourceID() {
      return 1
    },
    getParentOrSecondaryIDParamName() {
      return 'TestID'
    },
    resourceSubject: of({}),
  }

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [StorefrontTableComponent],
      providers: [
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: HttpClient, useValue: httpClient },
        { provide: StorefrontsService, useValue: storefrontService },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(StorefrontTableComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
