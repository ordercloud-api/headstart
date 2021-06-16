import { OcTokenService, OcSupplierUserService, OcSupplierService, ListPage } from '@ordercloud/angular-sdk';
import { AppAuthService } from './../../../../auth/services/app-auth.service';
import { HttpClientModule } from '@angular/common/http';
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { MiddlewareAPIService } from '@app-seller/shared/services/middleware-api/middleware-api.service';
import { SupplierService } from '../supplier.service';

import { SupplierTableComponent } from './supplier-table.component';
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component';
import { BehaviorSubject, from } from 'rxjs';

describe('SupplierTableComponent', () => {
  let component: SupplierTableComponent;
  let fixture: ComponentFixture<SupplierTableComponent>;

  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl'),
    url: { startsWith() { } },
    routerState: { snapshot: { url: 'url' } },
  };
  const activatedRoute = {
    snapshot: { queryParams: {} },
    params: from([{ id: 1 }]),
  };
  const resourceSubjectMock = new BehaviorSubject<ListPage<any>>(undefined);
  const supplierService = {
    isSupplierUser() {
      return true;
    },
    getParentResourceID() {
      return 1234;
    },
    resourceSubject: resourceSubjectMock.asObservable(),
    getParentOrSecondaryIDParamName() {
      return 'name';
    },
  };

  const middlewareApiService = {
    getSupplierFilterConfig() {
      return { Items: [] };
    },
  };

  const resourceCrudComponent = {};
  const appAuthService = {};
  const ocSupplierUserService = {};
  const ocSupplierService = {};

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [SupplierTableComponent],
      imports: [HttpClientModule],
      providers: [
        { provide: Router, useValue: router },
        {
          provide: ActivatedRoute,
          useValue: activatedRoute,
        },
        { provide: SupplierService, useValue: supplierService },
        { provide: MiddlewareAPIService, useValue: middlewareApiService },
        { provide: AppAuthService, useValue: appAuthService },
        { provide: OcSupplierUserService, useValue: ocSupplierUserService },
        { provide: OcSupplierService, useValue: ocSupplierService },
        { provide: ResourceCrudComponent, useValue: resourceCrudComponent },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(SupplierTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
