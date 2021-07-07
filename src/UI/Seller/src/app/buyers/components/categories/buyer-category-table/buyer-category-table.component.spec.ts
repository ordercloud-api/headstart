import { BuyerCategoryService } from './../../../../shared/services/buyer/buyer-category-service'
import { BuyerCategoryTableComponent } from './buyer-category-table.component'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'
import { ActivatedRoute, Router } from '@angular/router'
import { HttpClient } from '@angular/common/http'
import { BuyerService } from '../../buyers/buyer.service'
import { of } from 'rxjs'

describe('BuyerCategoryTableComponent', () => {
  let component: BuyerCategoryTableComponent
  let fixture: ComponentFixture<BuyerCategoryTableComponent>

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
      declarations: [BuyerCategoryTableComponent],
      providers: [
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: HttpClient, useValue: httpClient },
        { provide: BuyerCategoryService, useValue: buyerService },
        { provide: BuyerService, useValue: buyerService },
      ],
    }).compileComponents()
  }))
  beforeEach(() => {
    fixture = TestBed.createComponent(BuyerCategoryTableComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })
  beforeEach(() => {
    fixture = TestBed.createComponent(BuyerCategoryTableComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
