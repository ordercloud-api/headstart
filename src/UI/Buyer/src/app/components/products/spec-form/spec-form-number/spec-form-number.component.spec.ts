import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { SpecFormNumberComponent } from './spec-form-number.component';

describe('SpecFormInputComponent', () => {
  let component: SpecFormNumberComponent;
  let fixture: ComponentFixture<SpecFormNumberComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [SpecFormNumberComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SpecFormNumberComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
