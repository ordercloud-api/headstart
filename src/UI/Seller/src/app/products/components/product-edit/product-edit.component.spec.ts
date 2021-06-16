import { ProductEditComponent } from './product-edit.component'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'
import { ChangeDetectorRef, NgZone } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { OrderService } from '@app-seller/orders/order.service'
import { of } from 'rxjs'
import { ProductService } from '@app-seller/products/product.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import {
  OcSupplierAddressService,
  OcProductService,
  OcAdminAddressService,
  OcTokenService,
} from '@ordercloud/angular-sdk'
import { HttpClient } from '@angular/common/http'
import { MiddlewareAPIService } from '@app-seller/shared/services/middleware-api/middleware-api.service'

describe('ProductEditComponent', () => {
  let component: ProductEditComponent
  let fixture: ComponentFixture<ProductEditComponent>

  const router = {}
  const orderService = { resourceSubject: of({}) }
  const changeDetectorRef = {}
  const activatedRoute = { queryParams: of({}), params: of({}) }
  const ngZone = {}
  const currentUserService = {}
  const appAuthService = {}
  const ocSupplierAddressService = {}
  const ocProductService = {}
  const ocAdminAddressService = {}
  const productService = {}
  const middlewareApiService = {}
  const httpClient = {}
  const ocTokenService = {}

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ProductEditComponent],
      providers: [
        { provide: Router, useValue: router },
        { provide: OrderService, useValue: orderService },
        { provide: ChangeDetectorRef, useValue: changeDetectorRef },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: NgZone, useValue: ngZone },
        { provide: AppAuthService, useValue: appAuthService },
        { provide: CurrentUserService, useValue: currentUserService },
        {
          provide: OcSupplierAddressService,
          useValue: ocSupplierAddressService,
        },
        { provide: OcProductService, useValue: ocProductService },
        { provide: OcAdminAddressService, useValue: ocAdminAddressService },
        { provide: ProductService, useValue: productService },
        { provide: MiddlewareAPIService, useValue: middlewareApiService },
        { provide: HttpClient, useValue: httpClient },
        { provide: OcTokenService, useValue: ocTokenService },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(ProductEditComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })
})
