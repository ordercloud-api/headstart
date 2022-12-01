import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OrderReturnRefundInputComponent } from './order-return-refund-input.component';

describe('OrderReturnRefundInputComponent', () => {
  let component: OrderReturnRefundInputComponent;
  let fixture: ComponentFixture<OrderReturnRefundInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ OrderReturnRefundInputComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(OrderReturnRefundInputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
