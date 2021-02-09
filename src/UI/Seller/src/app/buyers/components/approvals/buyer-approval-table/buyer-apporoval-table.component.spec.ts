import { BuyerApprovalTableComponent } from './buyer-approval-table.component';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { BuyerApprovalService } from '../buyer-approval.service';
import { ActivatedRoute, Router } from '@angular/router';
import { BehaviorSubject, from } from 'rxjs';
import { BuyerService } from '../../buyers/buyer.service';
import { ListPage } from '@ordercloud/angular-sdk';
import { ResourceType } from '@ordercloud/angular-cms-components/shared/models/resource-type.interface';

describe('BuyerApprovalTableComponent', () => {
  let component: BuyerApprovalTableComponent;
  let fixture: ComponentFixture<BuyerApprovalTableComponent>;

  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl'),
    url: { startsWith() {} },
    routerState: { snapshot: { url: 'url' } },
  };
  const resourceSubjectMock = new BehaviorSubject<ListPage<ResourceType>>(undefined);

  const activatedRoute = { snapshot: { queryParams: {} }, params: from([{ id: 1 }]) };
  const buyerApprovalService = {
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
  const buyerService = {
    isSupplierUser() {
      return true;
    },
  };
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [BuyerApprovalTableComponent],
      imports: [],
      providers: [
        { provide: BuyerApprovalService, useValue: buyerApprovalService },
        { provide: Router, useValue: router },
        {
          provide: ActivatedRoute,
          useValue: activatedRoute,
        },
        { provide: BuyerService, useValue: buyerService },
      ],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BuyerApprovalTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
