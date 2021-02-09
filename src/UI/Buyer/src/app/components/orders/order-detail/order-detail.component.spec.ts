import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of, BehaviorSubject } from 'rxjs';
import { OcOrderService } from '@ordercloud/angular-sdk';
import { ParamMap, ActivatedRoute, convertToParamMap } from '@angular/router';
import { AppPaymentService } from 'src/app/shared/services/app-payment/app-payment.service';
import { OrderDetailsComponent } from 'src/app/order/containers/order-detail/order-detail.component';

describe('OrderDetailsComponent', () => {
  let component: OrderDetailsComponent;
  let fixture: ComponentFixture<OrderDetailsComponent>;

  const mockOrderID = 'MockGetOrder123';
  const orderService = {
    Get: jasmine.createSpy('Get').and.returnValue(of(null)),
    ListPromotions: jasmine.createSpy('ListPromotions').and.returnValue(of(null)),
    ListApprovals: jasmine.createSpy('ListApprovals').and.returnValue(of({ Items: [{ Comments: [] }] })),
  };
  const appPaymentService = {
    getPayments: jasmine.createSpy('getPayments').and.returnValue(of(null)),
  };

  const paramMap = new BehaviorSubject<ParamMap>(convertToParamMap({ orderID: mockOrderID }));

  const activatedRoute = {
    data: of({ orderResolve: { order: { ID: 'mockOrder' } } }),
    paramMap,
  };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [OrderDetailsComponent],
      providers: [
        { provide: OcOrderService, useValue: orderService },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: AppPaymentService, useValue: appPaymentService },
      ],
      schemas: [NO_ERRORS_SCHEMA], // Ignore template errors: remove if tests are added to test template
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OrderDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    beforeEach(() => {
      spyOn(component as any, 'getPromotions');
      component.ngOnInit();
    });
    it('should call getPromotions', () => {
      expect(component['getPromotions']).toHaveBeenCalled();
    });
  });

  describe('getPromotions', () => {
    it('should call OrderService.ListPromotions with order id param', () => {
      component['getPromotions']('id');
      expect(orderService.ListPromotions).toHaveBeenCalledWith('outgoing', mockOrderID);
    });
  });

  describe('getPayments', () => {
    it('should call AppPaymentService', () => {
      component['getPayments']('id');
      expect(appPaymentService.getPayments).toHaveBeenCalled();
    });
  });

  describe('getApprovals', () => {
    it('should call OrderService.ListApprovals', () => {
      component['getApprovals']('id');
      expect(orderService.ListApprovals).toHaveBeenCalled();
    });
  });
});
