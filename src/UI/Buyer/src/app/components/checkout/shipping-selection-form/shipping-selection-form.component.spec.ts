import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ShippingSelectionFormComponent } from './shipping-selection-form.component';

describe('ShippingSelectionFormComponent', () => {
  let component: ShippingSelectionFormComponent;
  let fixture: ComponentFixture<ShippingSelectionFormComponent>;

  beforeEach(async(() => {
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
