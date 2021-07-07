import { SupplierUserTableComponent } from './supplier-user-table.component'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'
import { BehaviorSubject, from } from 'rxjs'
import { ActivatedRoute, Router } from '@angular/router'
import { SupplierUserService } from '../supplier-user.service'
import { SupplierService } from '../../suppliers/supplier.service'
import { OcSupplierService, ListPage } from '@ordercloud/angular-sdk'

describe('SupplierUserTableComponent', () => {
  let component: SupplierUserTableComponent
  let fixture: ComponentFixture<SupplierUserTableComponent>
  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl'),
    url: { startsWith() { } },
    routerState: { snapshot: { url: 'url' } },
  }
  const resourceSubjectMock = new BehaviorSubject<ListPage<any>>(
    undefined
  )

  const activatedRoute = {
    snapshot: { queryParams: {} },
    params: from([{ id: 1 }]),
  }
  const supplierUserService = {
    isSupplierUser() {
      return true
    },
    getParentResourceID() {
      return 1234
    },
    resourceSubject: resourceSubjectMock.asObservable(),
    getParentOrSecondaryIDParamName() {
      return 'name'
    },
  }
  const supplierService = {
    isSupplierUser() {
      return true
    },
  }
  const ocSupplierService = {}

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [SupplierUserTableComponent],
      imports: [],
      providers: [
        { provide: SupplierUserService, useValue: supplierUserService },
        { provide: Router, useValue: router },
        {
          provide: ActivatedRoute,
          useValue: activatedRoute,
        },
        { provide: SupplierService, useValue: supplierService },
        { provide: OcSupplierService, useValue: ocSupplierService },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(SupplierUserTableComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
