import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { SpecFormCheckboxComponent } from './spec-form-checkbox.component';

describe('SpecFormSelectComponent', () => {
  let component: SpecFormCheckboxComponent;
  let fixture: ComponentFixture<SpecFormCheckboxComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [SpecFormCheckboxComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SpecFormCheckboxComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
