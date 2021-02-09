import { ProductTableComponent } from './product-table.component'
import { async, ComponentFixture, TestBed } from '@angular/core/testing'
import { Router, ActivatedRoute } from '@angular/router'
import { ProductService } from '@app-seller/products/product.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { OcSupplierService } from '@ordercloud/angular-sdk'
import { UserContext } from '@app-seller/config/user-context'
import { of } from 'rxjs/internal/observable/of'

describe('ProductTableComponent', () => {
  let component: ProductTableComponent
  let fixture: ComponentFixture<ProductTableComponent>

  const userContext: UserContext = {
    Me: {},
    UserRoles: ['SupplierReader'],
    UserType: 'type',
  }
  const productService = {
    isSupplierUser() {
      return false
    },
    resourceSubject: of({}),
    getParentResourceID() {
      return 1
    },
    getParentOrSecondaryIDParamName() {},
  }
  const currentUserService = {
    getUserContext() {
      return userContext
    },
  }
  const ocSupplierService = {
    List() {
      return of({ Meta: { TotalPages: 1 }, Items: [{}] })
    },
  }
  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl'),
    url: { startsWith() {} },
    routerState: { snapshot: { url: 'url' } },
  }
  const activatedRoute = { queryParams: of({}), params: of({}) }

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ProductTableComponent],
      providers: [
        { provide: ProductService, useValue: productService },
        { provide: CurrentUserService, useValue: currentUserService },
        { provide: OcSupplierService, useValue: ocSupplierService },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(ProductTableComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
