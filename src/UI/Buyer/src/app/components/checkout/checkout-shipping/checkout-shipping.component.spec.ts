import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { CheckoutShippingComponent } from './checkout-shipping.component';

describe('CheckoutShippingComponent', () => {
  let component: CheckoutShippingComponent;
  let fixture: ComponentFixture<CheckoutShippingComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CheckoutShippingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CheckoutShippingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
