import { SupplierEditComponent } from './supplier-edit.component'
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'
import { HttpClient } from '@angular/common/http'
import { ChangeDetectorRef, NgZone } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { OcSupplierUserService } from '@ordercloud/angular-sdk'
import { of } from 'rxjs'
import { SupplierService } from '../supplier.service'

describe('SupplierEditComponent', () => {
  let component: SupplierEditComponent
  let fixture: ComponentFixture<SupplierEditComponent>

  const router = {}
  const supplierService = { resourceSubject: of({}) }
  const changeDetectorRef = {}
  const activatedRoute = { queryParams: of({}), params: of({}) }
  const ngZone = {}
  const ocSupplierUserService = {
    List() {
      return of({})
    },
  }
  const appAuthService = {}

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [SupplierEditComponent],
      providers: [
        { provide: Router, useValue: router },
        { provide: SupplierService, useValue: supplierService },
        { provide: ChangeDetectorRef, useValue: changeDetectorRef },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: NgZone, useValue: ngZone },
        { provide: AppAuthService, useValue: appAuthService },
        { provide: OcSupplierUserService, useValue: ocSupplierUserService },
      ],
    }).compileComponents()
  }))

  beforeEach(() => {
    fixture = TestBed.createComponent(SupplierEditComponent)
    component = fixture.componentInstance
    component.isCreatingNew = true
    fixture.detectChanges()
  })
})
