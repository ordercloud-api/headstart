import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SpecFormSelectComponent } from './spec-form-select.component';

describe('SpecFormSelectComponent', () => {
  let component: SpecFormSelectComponent;
  let fixture: ComponentFixture<SpecFormSelectComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [SpecFormSelectComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SpecFormSelectComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
