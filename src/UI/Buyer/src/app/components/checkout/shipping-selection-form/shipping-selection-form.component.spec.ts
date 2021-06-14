import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ShippingSelectionFormComponent } from './shipping-selection-form.component';

describe('ShippingSelectionFormComponent', () => {
  let component: ShippingSelectionFormComponent;
  let fixture: ComponentFixture<ShippingSelectionFormComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ShippingSelectionFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShippingSelectionFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
