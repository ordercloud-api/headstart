import { BuyerLocationService } from './../buyer-location.service'
import { BuyerLocationTableComponent } from './buyer-location-table.component'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'
import { ActivatedRoute, Router } from '@angular/router'
import { HttpClient } from '@angular/common/http'
import { of } from 'rxjs'
import { BuyerService } from '../../buyers/buyer.service'

describe('BuyerLocationTable', () => {
  let component: BuyerLocationTableComponent
  let fixture: ComponentFixture<BuyerLocationTableComponent>
  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl'),
    url: '/my-',
    routerState: { snapshot: { url: 'https://test' } },
  }
  const activatedRoute = { params: of({}) }
  const httpClient = {}
  const buyerService = {
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
      declarations: [BuyerLocationTableComponent],
      providers: [
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: HttpClient, useValue: httpClient },
        { provide: BuyerLocationService, useValue: buyerService },
        { provide: BuyerService, useValue: buyerService },
      ],
    }).compileComponents()
  }))
  beforeEach(() => {
    fixture = TestBed.createComponent(BuyerLocationTableComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
