import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StatusFilterComponent } from 'src/app/order/components/status-filter/order-status-filter.component';
import { ReactiveFormsModule } from '@angular/forms';
import { OrderStatus } from 'src/app/order/models/order-status.model';
import { OrderStatusDisplayPipe } from 'src/app/ocm-default-components/pipes/order-status-display/order-status-display.pipe';

describe('StatusFilterComponent', () => {
  let component: StatusFilterComponent;
  let fixture: ComponentFixture<StatusFilterComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [StatusFilterComponent, OrderStatusDisplayPipe],
      imports: [ReactiveFormsModule],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StatusFilterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    beforeEach(() => {
      component.ngOnInit();
    });
    it('should initialize status to show non-Unsubmitted orders', () => {
      expect(component.form.value).toEqual({
        status: '!Unsubmitted',
      });
    });
    it('should set statuses correctly', () => {
      expect(component['statuses']).toEqual([
        OrderStatus.Open,
        OrderStatus.AwaitingApproval,
        OrderStatus.Completed,
        // OrderStatus.Declined,
      ]);
    });
  });

  describe('selectStatus', () => {
    beforeEach(() => {
      spyOn(component.selectedStatus, 'emit');
    });
    it('should emit status from form', () => {
      component.form.controls['status'].setValue('Open');
      component['selectStatus']();
      expect(component.selectedStatus.emit).toHaveBeenCalledWith('Open');
    });
  });
});
