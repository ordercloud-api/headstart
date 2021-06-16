import { AppAuthService } from './../../../auth/services/app-auth.service'
import { MiddlewareAPIService } from '@app-seller/shared/services/middleware-api/middleware-api.service'
import { HttpClient } from '@angular/common/http'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'
import { ActivatedRoute, Router } from '@angular/router'
import { BuyerService } from '@app-seller/buyers/components/buyers/buyer.service'
import { BuyerCatalogService } from '@app-seller/buyers/components/catalogs/buyer-catalog.service'
import { OrderService } from '@app-seller/orders/order.service'
import {
  OcLineItemService,
  OcOrderService,
  OcPaymentService,
} from '@ordercloud/angular-sdk'
import { of } from 'rxjs'

import { OrderDetailsComponent } from './order-details.component'

describe('OrderDetailsComponent', () => {
  let component: OrderDetailsComponent
  let fixture: ComponentFixture<OrderDetailsComponent>

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
  const lineItemService = {}
  const ocOrderService = {}
  const ocPaymentService = {}
  const orderService = { isQuoteOrder() {} }
  const middlewareService = {}
  const appAuthService = { getOrdercloudUserType() {} }

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [OrderDetailsComponent],
      providers: [
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: HttpClient, useValue: httpClient },
        { provide: OcLineItemService, useValue: lineItemService },
        { provide: OcOrderService, useValue: ocOrderService },
        { provide: OcPaymentService, useValue: ocPaymentService },
        { provide: OrderService, useValue: orderService },
        { provide: MiddlewareAPIService, useValue: middlewareService },
        { provide: AppAuthService, useValue: appAuthService },

        { provide: BuyerService, useValue: buyerService },
      ],
    }).compileComponents()
  }))
  beforeEach(() => {
    fixture = TestBed.createComponent(OrderDetailsComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
