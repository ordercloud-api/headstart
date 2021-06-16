import { CurrentUserService } from './../../../shared/services/current-user/current-user.service'
import { HttpClient } from '@angular/common/http'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { OrderService } from '@app-seller/orders/order.service'
import {
  OcLineItemService,
  OcOrderService,
  OcShipmentService,
  OcSupplierAddressService,
} from '@ordercloud/angular-sdk'
import { of } from 'rxjs'

import { OrderShipmentsComponent } from './order-shipments.component'

describe('OrderShipmentsComponent', () => {
  let component: OrderShipmentsComponent
  let fixture: ComponentFixture<OrderShipmentsComponent>

  const httpClient = {}

  const lineItemService = {}
  const ocOrderService = {}
  const ocSupplierAddressService = {}
  const orderService = { isQuoteOrder() {} }
  const appAuthService = { getOrdercloudUserType() {} }
  const ocShipmentService = {}
  const currentUserService = {}

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [OrderShipmentsComponent],
      providers: [
        {
          provide: OcSupplierAddressService,
          useValue: ocSupplierAddressService,
        },
        { provide: OcShipmentService, useValue: ocShipmentService },
        { provide: HttpClient, useValue: httpClient },
        { provide: OcLineItemService, useValue: lineItemService },
        { provide: OcOrderService, useValue: ocOrderService },
        { provide: CurrentUserService, useValue: currentUserService },
        { provide: OrderService, useValue: orderService },
        { provide: AppAuthService, useValue: appAuthService },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(OrderShipmentsComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
