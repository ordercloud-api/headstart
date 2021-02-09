import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SpecFormTextAreaComponent } from './spec-form-textarea.component';

describe('SpecFormTextAreaComponent', () => {
  let component: SpecFormTextAreaComponent;
  let fixture: ComponentFixture<SpecFormTextAreaComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [SpecFormTextAreaComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SpecFormTextAreaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
