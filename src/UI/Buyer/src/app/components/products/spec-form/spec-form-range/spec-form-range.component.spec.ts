import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { SpecFormRangeComponent } from './spec-form-range.component';

describe('SpecFormInputComponent', () => {
  let component: SpecFormRangeComponent;
  let fixture: ComponentFixture<SpecFormRangeComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [SpecFormRangeComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SpecFormRangeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
