import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CheckoutComponent } from 'src/app/checkout/containers/checkout/checkout.component';
import { NgbAccordion, NgbPanel, NgbAccordionConfig } from '@ng-bootstrap/ng-bootstrap';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { ReactiveFormsModule } from '@angular/forms';
import { of, BehaviorSubject } from 'rxjs';
import { OcOrderService, OcPaymentService } from '@ordercloud/angular-sdk';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { AppErrorHandler } from 'src/app/config/error-handling.config';
import { Router } from '@angular/router';

describe('CheckoutComponent', () => {
  let component: CheckoutComponent;
  let fixture: ComponentFixture<CheckoutComponent>;
  const appStateService = {
    orderSubject: new BehaviorSubject({ ID: 'someorderid', LineItemCount: 1 }),
    isAnonSubject: new BehaviorSubject(false),
  };
  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl'),
  };
  let ocOrderService = {
    Get: () => {},
    Submit: () => {},
  };
  const paymentService = {
    List: jasmine.createSpy('List').and.returnValue(of({ Items: [{ ID: 'paymentID' }] })),
  };
  const baseResolveService = {
    resetUser: jasmine.createSpy('restUser').and.returnValue(null),
  };

  const appErrorHandler = { displayError: jasmine.createSpy('displayError') };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [CheckoutComponent, NgbAccordion, NgbPanel],
      imports: [FontAwesomeModule, ReactiveFormsModule],
      providers: [
        NgbAccordionConfig,
        { provide: AppErrorHandler, useValue: appErrorHandler },
        { provide: Router, useValue: router },
        { provide: OcOrderService, useValue: ocOrderService },
        { provide: OcPaymentService, useValue: paymentService },
      ],
      schemas: [NO_ERRORS_SCHEMA], // Ignore template errors: remove if tests are added to test template
    }).compileComponents();
    ocOrderService = TestBed.get(OcOrderService);
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CheckoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit()', () => {
    beforeEach(() => {
      spyOn(component, 'setValidation');
    });
    it('should set panel to shippingAddress if user is profiled', () => {
      appStateService.isAnonSubject.next(false);
      component.ngOnInit();
      expect(component.currentPanel).toEqual('shippingAddress');
    });
    it('should validate login panel if user is profiled', () => {
      appStateService.isAnonSubject.next(false);
      component.ngOnInit();
      expect(component.setValidation).toHaveBeenCalledWith('login', true);
    });
    it('should set panel to login if user is anonymous', () => {
      appStateService.isAnonSubject.next(true);
      component.ngOnInit();
      component.currentPanel = 'login';
    });
    it('should invalidate login panel if user is anonymous', () => {
      appStateService.isAnonSubject.next(true);
      component.ngOnInit();
      expect(component.setValidation).toHaveBeenCalledWith('login', false);
    });
  });

  describe('getValidation()', () => {
    beforeEach(() => {
      component.sections = [
        {
          id: 'login',
          valid: true,
        },
        {
          id: 'shippingAddress',
          valid: true,
        },
        {
          id: 'billingAddress',
          valid: false,
        },
        {
          id: 'payment',
          valid: false,
        },
        {
          id: 'confirm',
          valid: false,
        },
      ];
    });
    it('should get validation for section', () => {
      component.sections.forEach(section => {
        expect(component.getValidation(section.id)).toBe(section.valid);
      });
    });
  });

  describe('setValidation()', () => {
    it('should get validation for section', () => {
      component.setValidation('login', true);
      expect(component.sections.find(x => x.id === 'login').valid).toEqual(true);
      component.setValidation('shippingAddress', false);
      expect(component.sections.find(x => x.id === 'shippingAddress').valid).toEqual(false);
      component.setValidation('payment', false);
      expect(component.sections.find(x => x.id === 'payment').valid).toEqual(false);
    });
  });

  describe('toSection()', () => {
    it('should go to a valid section', () => {
      // validate previous panels
      component.setValidation('login', true);
      component.setValidation('shippingAddress', true);
      component.currentPanel = 'billingAddress';

      component.toSection('payment');
      fixture.detectChanges();

      expect(component.currentPanel).toEqual('payment');
    });
  });

  describe('submitOrder()', () => {
    beforeEach(() => {
      spyOn(ocOrderService, 'Submit').and.returnValue(of(null));
    });
    it('should throw error if order has already been submitted', () => {
      spyOn(ocOrderService, 'Get').and.returnValue(of({ IsSubmitted: true }));
      component.submitOrder();
      expect(ocOrderService.Submit).not.toHaveBeenCalled();
      expect(appErrorHandler.displayError).toHaveBeenCalledWith({
        message: 'Order has already been submitted',
      });
    });
    it('should call submit if order has not been submitted', () => {
      spyOn(ocOrderService, 'Get').and.returnValue(of({ IsSubmitted: false }));
      component.submitOrder();
      expect(ocOrderService.Submit).toHaveBeenCalled();
      expect(router.navigateByUrl).toHaveBeenCalledWith('order-confirmation/someorderid');
      expect(baseResolveService.resetUser).toHaveBeenCalled();
    });
  });
});
