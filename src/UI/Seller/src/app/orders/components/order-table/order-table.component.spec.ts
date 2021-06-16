import { ChangeDetectorRef, NgZone } from '@angular/core'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'
import { Router, ActivatedRoute } from '@angular/router'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { OrderService } from '@app-seller/orders/order.service'
import { of } from 'rxjs'

import { OrderTableComponent } from './order-table.component'

describe('OrderTableComponent', () => {
  let component: OrderTableComponent
  let fixture: ComponentFixture<OrderTableComponent>

  const router = {}
  const orderService = { resourceSubject: of({}) }
  const changeDetectorRef = {}
  const activatedRoute = { queryParams: of({}), params: of({}) }
  const ngZone = {}
  const appAuthService = {}

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [OrderTableComponent],
      providers: [
        { provide: Router, useValue: router },
        { provide: OrderService, useValue: orderService },
        { provide: ChangeDetectorRef, useValue: changeDetectorRef },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: NgZone, useValue: ngZone },
        { provide: AppAuthService, useValue: appAuthService },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(OrderTableComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })
})
