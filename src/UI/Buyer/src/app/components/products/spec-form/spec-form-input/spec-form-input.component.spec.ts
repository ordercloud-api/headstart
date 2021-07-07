import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { SpecFormInputComponent } from './spec-form-input.component';

describe('SpecFormInputComponent', () => {
  let component: SpecFormInputComponent;
  let fixture: ComponentFixture<SpecFormInputComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [SpecFormInputComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SpecFormInputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
