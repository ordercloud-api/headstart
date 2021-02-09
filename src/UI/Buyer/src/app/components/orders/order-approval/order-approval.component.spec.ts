import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OrderApprovalComponent } from './order-approval.component';
import { of } from 'rxjs';
import { Router } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { OcOrderService } from '@ordercloud/angular-sdk';
import { NO_ERRORS_SCHEMA } from '@angular/core';

describe('OrderApprovalComponent', () => {
  let component: OrderApprovalComponent;
  let fixture: ComponentFixture<OrderApprovalComponent>;
  const mockOrderID = 'orderID';
  const orderService = {
    Approve: jasmine.createSpy('Approve').and.returnValue(of(null)),
    Decline: jasmine.createSpy('Decline').and.returnValue(of(null)),
  };
  const modalService = {
    open: jasmine.createSpy('open').and.returnValue(null),
    close: jasmine.createSpy('close').and.returnValue(null),
  };
  const toasterService = {
    success: jasmine.createSpy('success').and.returnValue(null),
  };
  const router = { navigateByUrl: jasmine.createSpy('navigateByUrl') };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [OrderApprovalComponent],
      imports: [ReactiveFormsModule],
      providers: [
        { provide: ToastrService, useValue: toasterService },
        { provide: OcOrderService, useValue: orderService },
        { provide: Router, useValue: router },
      ],
      schemas: [NO_ERRORS_SCHEMA], // Ignore template errors: remove if tests are added to test template
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OrderApprovalComponent);
    component = fixture.componentInstance;
    component.orderID = mockOrderID;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('open modal', () => {
    beforeEach(() => {
      component.openModal(true);
    });
    it('should call open model', () => {
      expect(modalService.open).toHaveBeenCalledWith(component.modalID);
    });
    it('should set approve value', () => {
      expect(component.approved).toEqual(true);
    });
  });
  describe('submitReview', () => {
    it('Should call Approve if approve is true', () => {
      component.approved = true;
      component.submitReview();
      expect(orderService.Approve).toHaveBeenCalledWith('outgoing', mockOrderID, {
        Comments: component.form.value.comments,
        AllowResubmit: false,
      });
    });
    it('Should call Decline if approve is false', () => {
      component.approved = false;
      component.submitReview();
      expect(orderService.Approve).toHaveBeenCalledWith('outgoing', mockOrderID, {
        Comments: component.form.value.comments,
        AllowResubmit: false,
      });
    });
    it('Should do a bunch of things after submitting the review', () => {
      component.submitReview();
      expect(toasterService.success).toHaveBeenCalled();
      expect(modalService.close).toHaveBeenCalled();
      expect(router.navigateByUrl).toHaveBeenCalled();
    });
  });
});
