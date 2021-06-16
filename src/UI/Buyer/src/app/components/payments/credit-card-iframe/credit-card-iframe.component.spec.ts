import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { CreditCardIframeComponent } from './credit-card-iframe.component';

describe('CreditCardIframeComponent', () => {
  let component: CreditCardIframeComponent;
  let fixture: ComponentFixture<CreditCardIframeComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CreditCardIframeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreditCardIframeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
