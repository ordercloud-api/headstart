import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OCMSpecForm } from './spec-form.component';

describe('SpecFormComponent', () => {
  let component: OCMSpecForm;
  let fixture: ComponentFixture<OCMSpecForm>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [OCMSpecForm],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OCMSpecForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
