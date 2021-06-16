import { AppAuthService } from './../../../../auth/services/app-auth.service'
import { BuyerEditComponent } from './../../buyers/buyer-edit/buyer-edit.component'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'
import { ActivatedRoute, Router } from '@angular/router'
import { HttpClient } from '@angular/common/http'
import { BuyerService } from '../../buyers/buyer.service'
import { of } from 'rxjs/internal/observable/of'
import { BuyerTempService } from '@app-seller/shared/services/middleware-api/buyer-temp.service'

describe('BuyerCategoryEdit', () => {
  let component: BuyerEditComponent
  let fixture: ComponentFixture<BuyerEditComponent>

  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl'),
    url: '/my-',
    routerState: { snapshot: { url: 'https://test' } },
  }
  const activatedRoute = { params: of({}) }
  const httpClient = {}
  const appAuthService = {}
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
      declarations: [BuyerEditComponent],
      providers: [
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: HttpClient, useValue: httpClient },
        { provide: BuyerService, useValue: buyerService },
        { provide: BuyerTempService, useValue: buyerService },
        { provide: AppAuthService, useValue: appAuthService },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(BuyerEditComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
