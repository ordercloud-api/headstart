import { SellerUserService } from '@app-seller/sellers/seller-admin.service'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'

import { SellerUserTableComponent } from './seller-user-table.component'
import { Router, ActivatedRoute } from '@angular/router'
import { of } from 'rxjs'
import { UserContext } from '@app-seller/shared'

describe('SellerUserTableComponent', () => {
  let component: SellerUserTableComponent
  let fixture: ComponentFixture<SellerUserTableComponent>

  const userContext: UserContext = {
    Me: {},
    UserRoles: ['SupplierReader'],
    UserType: 'type',
  }
  const sellerUserService = {
    isSupplierUser() {
      return false
    },
    resourceSubject: of({}),
    getParentResourceID() {
      return 1
    },
    getParentOrSecondaryIDParamName() { },
  }
  const currentUserService = {
    getUserContext() {
      return userContext
    },
  }
  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl'),
    url: { startsWith() { }, split() { } },
    routerState: { snapshot: { url: 'url' } },
  }
  const activatedRoute = { queryParams: of({}), params: of({}) }

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [SellerUserTableComponent],
      providers: [
        {
          provide: SellerUserService,
          useValue: sellerUserService,
        },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(SellerUserTableComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
