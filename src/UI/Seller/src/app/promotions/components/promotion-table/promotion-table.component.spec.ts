import { PromotionService } from '@app-seller/promotions/promotion.service'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'
import { Router, ActivatedRoute } from '@angular/router'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { of } from 'rxjs'

import { PromotionTableComponent } from './promotion-table.component'
import { UserContext } from '@app-seller/shared'

describe('PromotionTableComponent', () => {
  let component: PromotionTableComponent
  let fixture: ComponentFixture<PromotionTableComponent>
  const userContext: UserContext = {
    Me: {},
    UserRoles: ['SupplierReader'],
    UserType: 'type',
  }
  const promotionService = {
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
      declarations: [PromotionTableComponent],
      providers: [
        { provide: PromotionService, useValue: promotionService },
        { provide: CurrentUserService, useValue: currentUserService },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(PromotionTableComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
