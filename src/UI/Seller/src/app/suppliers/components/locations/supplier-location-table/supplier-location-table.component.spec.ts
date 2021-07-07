import { SupplierService } from './../../suppliers/supplier.service'
import { SupplierAddressService } from './../supplier-address.service'
import { SupplierLocationTableComponent } from './supplier-location-table.component'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'
import { HttpClient } from '@angular/common/http'
import { Router, ActivatedRoute } from '@angular/router'
import { of } from 'rxjs'

describe('SupplierLocationTableComponent', () => {
  let component: SupplierLocationTableComponent
  let fixture: ComponentFixture<SupplierLocationTableComponent>

  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl'),
    url: '/my-',
    routerState: { snapshot: { url: 'https://test' } },
  }
  const activatedRoute = { params: of({}) }
  const httpClient = {}
  const supplierAddressService = {
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

  const supplierService = {}

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [SupplierLocationTableComponent],
      providers: [
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: HttpClient, useValue: httpClient },
        { provide: SupplierAddressService, useValue: supplierAddressService },
        { provide: SupplierService, useValue: supplierService },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(SupplierLocationTableComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
