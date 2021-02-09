import { BuyerPaymentService } from './../buyer-payment.service'
import { BuyerPaymentTableComponent } from './buyer-payment-table.component'
import { async, ComponentFixture, TestBed } from '@angular/core/testing'
import { HttpClient } from '@angular/common/http'
import { Router, ActivatedRoute } from '@angular/router'
import { of } from 'rxjs'
import { BuyerService } from '../../buyers/buyer.service'

describe('BuyerPaymentTableComponent', () => {
  let component: BuyerPaymentTableComponent
  let fixture: ComponentFixture<BuyerPaymentTableComponent>

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

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [BuyerPaymentTableComponent],
      providers: [
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: HttpClient, useValue: httpClient },
        { provide: BuyerPaymentService, useValue: buyerService },
        { provide: BuyerService, useValue: buyerService },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(BuyerPaymentTableComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
