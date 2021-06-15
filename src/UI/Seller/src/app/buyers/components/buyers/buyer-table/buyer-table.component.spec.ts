import { HttpClient } from '@angular/common/http'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'
import { ActivatedRoute, Router } from '@angular/router'
import { of } from 'rxjs'
import { BuyerService } from '../buyer.service'

import { BuyerTableComponent } from './buyer-table.component'

describe('BuyerTableComponent', () => {
  let component: BuyerTableComponent
  let fixture: ComponentFixture<BuyerTableComponent>

  const router = {
    routerState: { snapshot: { url: 'url/url2' } },
    navigateByUrl: jasmine.createSpy('navigateByUrl'),
    url: '/my-',
  }
  const activatedRoute = { params: of({}) }
  const httpClient = {}
  const buyerService = {
    getParentOrSecondaryIDParamName() {
      return '1'
    },
    getParentResourceID() {
      return 1
    },
    isSupplierUser() {
      return true
    },
    getMyResource() {
      return {}
    },
    copyResource() {},
    resourceSubject: of({}),
  }

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [BuyerTableComponent],
      providers: [
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: HttpClient, useValue: httpClient },
        { provide: BuyerService, useValue: buyerService },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(BuyerTableComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
